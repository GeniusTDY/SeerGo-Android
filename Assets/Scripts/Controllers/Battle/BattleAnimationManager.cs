using FTRuntime;
using SeerGoGame.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 战斗动画管理器 - 负责一次性预加载双方全部动画，缓存后按需显示/隐藏
/// 动画类型: idle, attack, sa, cp, hited
/// 兼容两种动画类型：
///   - 旧类型: Animator + SpriteRenderer + SpritePivotAligner (pfa_-1, pfa_3)
///   - 新类型: SwfClip + SwfClipController + SwfClipFrameEvents (其他pfa_*)
/// </summary>
public class BattleAnimationManager
{
    // 预加载的所有动画后缀
    private static readonly string[] AnimationSuffixes = { "idle", "attack", "sa", "cp", "hited" };

    // 统一动画缓存：Key = "petId_animSuffix" (如 "1_idle", "1_attack")
    private readonly Dictionary<string, GameObject> playerPetsAction = new Dictionary<string, GameObject>();
    private readonly Dictionary<string, GameObject> enemyPetsAction = new Dictionary<string, GameObject>();

    // ID缓存：已加载的精灵ID
    private readonly HashSet<int> loadedPlayerPetIds = new HashSet<int>();
    private readonly HashSet<int> loadedEnemyPetIds = new HashSet<int>();

    // 精灵动画类型记录：Key = petId，true=新类型(SwfClip), false=旧类型(Animator)
    private readonly Dictionary<int, bool> petAnimationTypeCache = new Dictionary<int, bool>();

    // 动画队列
    private readonly Queue<IEnumerator> animationQueue = new Queue<IEnumerator>();
    private bool isPlayingAnimation = false;

    // 引用
    private readonly MonoBehaviour owner;
    private readonly Transform parentTransform;
    private readonly System.Action onQueueComplete;

    public BattleAnimationManager(MonoBehaviour owner, Transform parentTransform, System.Action onQueueComplete)
    {
        this.owner = owner;
        this.parentTransform = parentTransform;
        this.onQueueComplete = onQueueComplete;
    }

    #region 公共属性访问器
    public Dictionary<string, GameObject> PlayerPetsAction => playerPetsAction;
    public Dictionary<string, GameObject> EnemyPetsAction => enemyPetsAction;
    public HashSet<int> LoadedPlayerPetIds => loadedPlayerPetIds;
    public HashSet<int> LoadedEnemyPetIds => loadedEnemyPetIds;

    /// <summary>
    /// 判断指定精灵是否使用新动画类型（SwfClip）
    /// </summary>
    public bool IsNewAnimationType(int petId)
    {
        return petAnimationTypeCache.TryGetValue(petId, out bool isNew) && isNew;
    }
    #endregion

    #region 动画队列管理
    public void EnqueueAnimation(IEnumerator animation)
    {
        animationQueue.Enqueue(animation);
        if (!isPlayingAnimation)
        {
            owner.StartCoroutine(ProcessAnimationQueue());
        }
    }

    private IEnumerator ProcessAnimationQueue()
    {
        isPlayingAnimation = true;
        while (animationQueue.Count > 0)
        {
            IEnumerator currentAnimation = animationQueue.Dequeue();
            yield return owner.StartCoroutine(currentAnimation);
        }
        isPlayingAnimation = false;
        onQueueComplete?.Invoke();
    }
    #endregion

    #region 精灵预加载（一次性加载全部动画）
    /// <summary>
    /// 预加载玩家精灵的全部动画（idle + attack + sa + cp + hited）
    /// </summary>
    public void LoadPlayerPet(int petId)
    {
        LoadAllAnimations(petId, false, playerPetsAction, loadedPlayerPetIds);
    }

    /// <summary>
    /// 预加载敌方精灵的全部动画（idle + attack + sa + cp + hited）
    /// </summary>
    public void LoadEnemyPet(int petId)
    {
        LoadAllAnimations(petId, true, enemyPetsAction, loadedEnemyPetIds);
    }

    /// <summary>
    /// 一次性加载指定精灵的所有动画类型，自动检测新旧动画类型并走对应路径
    /// </summary>
    private void LoadAllAnimations(int petId, bool isEnemy,
        Dictionary<string, GameObject> petsActionDict, HashSet<int> loadedPetIds)
    {
        // 已加载则跳过
        string idleKey = $"{petId}_idle";
        if (petsActionDict.ContainsKey(idleKey))
            return;

        var config = PetPrefabConfigManager.Instance.GetConfigById(petId);
        if (config == null)
        {
            SeerGoLogger.LogWarning($"未找到精灵 {petId} 的预制体配置");
            return;
        }

        // 检测是否同一只精灵对战（需要绕过对象池创建独立实例）
        bool isSamePetBothSides = isEnemy
            ? loadedPlayerPetIds.Contains(petId)
            : loadedEnemyPetIds.Contains(petId);

        // 计算目标位置和缩放
        Vector3 targetPos = isEnemy
            ? new Vector3(config.flipPosition.x, config.position.y, 0f)
            : new Vector3(config.position.x, config.position.y, 0f);
        float scaleX = isEnemy ? -config.scale : config.scale;
        Vector3 petScale = new Vector3(scaleX, config.scale, config.scale);

        // 先加载idle来判断动画类型
        bool isNewType = false;
        GameObject firstObj = TryLoadPetPrefab(petId, "idle", isSamePetBothSides);

        if (firstObj != null)
        {
            isNewType = firstObj.GetComponent<SwfClipFrameEvents>() != null;
            petAnimationTypeCache[petId] = isNewType;

            if (isNewType)
                SetupNewTypeAnimation(firstObj, petId, "idle", targetPos, petScale, petsActionDict, true);
            else
                SetupOldTypeAnimation(firstObj, petId, "idle", targetPos, petScale, petsActionDict, true);
        }

        // 加载其余动画
        string[] otherSuffixes = { "attack", "sa", "cp", "hited" };
        foreach (string suffix in otherSuffixes)
        {
            GameObject petObj = TryLoadPetPrefab(petId, suffix, isSamePetBothSides);

            if (petObj != null)
            {
                if (isNewType)
                    SetupNewTypeAnimation(petObj, petId, suffix, targetPos, petScale, petsActionDict, false);
                else
                    SetupOldTypeAnimation(petObj, petId, suffix, targetPos, petScale, petsActionDict, false);
            }
        }

        loadedPetIds.Add(petId);
    }

    /// <summary>
    /// 尝试加载指定精灵的某个动画预制体
    /// </summary>
    private GameObject TryLoadPetPrefab(int petId, string suffix, bool bypassPool)
    {
        string bundleName = $"pfa_{petId}";
        string prefabName = $"{petId}_{suffix}";

        if (bypassPool)
        {
            GameObject prefab = ExternalAssetBundleManager.Instance.LoadPetPrefab(bundleName, prefabName, "");
            return prefab != null
                ? GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity)
                : null;
        }
        else
        {
            return ExternalAssetBundleManager.Instance.InstantiatePet(
                bundleName, prefabName, Vector3.zero, Quaternion.identity);
        }
    }

    /// <summary>
    /// 设置旧类型动画（Animator + SpriteRenderer + SpritePivotAligner）
    /// </summary>
    private void SetupOldTypeAnimation(GameObject petObj, int petId, string suffix,
        Vector3 targetPos, Vector3 petScale,
        Dictionary<string, GameObject> petsActionDict, bool isIdle)
    {
        string key = $"{petId}_{suffix}";
        petObj.transform.SetParent(parentTransform);

        var aligner = petObj.GetComponent<SpritePivotAligner>();
        if (aligner != null)
        {
            aligner.InitializeSync(targetPos, petScale);
        }
        else
        {
            petObj.transform.localScale = petScale;
            petObj.transform.position = targetPos;
        }

        petObj.SetActive(isIdle);

        Animator animator = petObj.GetComponent<Animator>();
        if (animator != null)
            animator.speed = BattleEntityController.BattleAnimationSpeed;

        petsActionDict[key] = petObj;
    }

    /// <summary>
    /// 设置新类型动画（SwfClip + SwfClipController + SwfClipFrameEvents）
    /// 新精灵没有 SpritePivotAligner，直接用 pet_prefab_config 的位置和缩放
    /// </summary>
    private void SetupNewTypeAnimation(GameObject petObj, int petId, string suffix,
        Vector3 targetPos, Vector3 petScale,
        Dictionary<string, GameObject> petsActionDict, bool isIdle)
    {
        string key = $"{petId}_{suffix}";
        petObj.transform.SetParent(parentTransform);

        // 新类型直接设置位置和缩放，无需 SpritePivotAligner
        petObj.transform.localScale = petScale;
        petObj.transform.position = targetPos;

        // 设置 SwfClipController
        var ctrl = petObj.GetComponent<SwfClipController>();
        if (ctrl != null)
        {
            ctrl.rateScale = BattleEntityController.BattleAnimationSpeed;

            if (isIdle)
            {
                ctrl.loopMode = SwfClipController.LoopModes.Loop;
            }
            else
            {
                ctrl.loopMode = SwfClipController.LoopModes.Once;
                // 订阅停止事件，用于非idle动画播完回idle
                ctrl.OnStopPlayingEvent += OnSwfClipActionStopped;
            }
        }

        // 只有idle初始显示，其余隐藏
        petObj.SetActive(isIdle);

        petsActionDict[key] = petObj;
    }

    /// <summary>
    /// SwfClip非idle动作播放完毕回调
    /// </summary>
    private void OnSwfClipActionStopped(SwfClipController ctrl)
    {
        // 子类负责具体切换逻辑（由 BattleEntityController 处理）
    }
    #endregion

    #region 受击动画播放
    /// <summary>
    /// 播放玩家受击动画
    /// </summary>
    public IEnumerator PlayPlayerHitedAnimation(int petId)
    {
        yield return PlayHitedAnimation(petId, playerPetsAction);
    }

    /// <summary>
    /// 播放敌方受击动画
    /// </summary>
    public IEnumerator PlayEnemyHitedAnimation(int petId)
    {
        yield return PlayHitedAnimation(petId, enemyPetsAction);
    }

    private IEnumerator PlayHitedAnimation(int petId, Dictionary<string, GameObject> idleActions)
    {
        string idleKey = $"{petId}_idle";
        string hitedKey = $"{petId}_hited";
        bool isNewType = IsNewAnimationType(petId);

        if (idleActions.TryGetValue(idleKey, out GameObject idleObj))
            idleObj.SetActive(false);

        if (idleActions.TryGetValue(hitedKey, out GameObject hitedObj))
        {
            if (isNewType)
            {
                // 新类型：SwfClip受击
                var hitedCtrl = hitedObj.GetComponent<SwfClipController>();
                if (hitedCtrl != null)
                    hitedCtrl.rateScale = BattleEntityController.BattleAnimationSpeed;

                hitedObj.SetActive(true);
                yield return new WaitForSeconds(0.5f / BattleEntityController.BattleAnimationSpeed);
                hitedObj.SetActive(false);
            }
            else
            {
                // 旧类型：Animator受击
                Animator hitedAnimator = hitedObj.GetComponent<Animator>();
                if (hitedAnimator != null)
                    hitedAnimator.speed = BattleEntityController.BattleAnimationSpeed;

                hitedObj.SetActive(true);
                yield return new WaitForSeconds(0.5f / BattleEntityController.BattleAnimationSpeed);
                hitedObj.SetActive(false);
            }
        }

        if (idleObj != null)
        {
            if (isNewType)
            {
                var idleCtrl = idleObj.GetComponent<SwfClipController>();
                if (idleCtrl != null)
                    idleCtrl.rateScale = BattleEntityController.BattleAnimationSpeed;
            }
            else
            {
                Animator idleAnimator = idleObj.GetComponent<Animator>();
                if (idleAnimator != null)
                    idleAnimator.speed = BattleEntityController.BattleAnimationSpeed;
            }
            idleObj.SetActive(true);
        }
    }
    #endregion

    #region 全局倍速更新
    /// <summary>
    /// 更新全部已加载精灵的倍速（用于运行时变速）
    /// 兼容旧类型(Animator.speed)和新类型(SwfClipController.rateScale)
    /// </summary>
    public void UpdateAllAnimatorSpeed()
    {
        UpdateAnimationSpeedInCache(playerPetsAction);
        UpdateAnimationSpeedInCache(enemyPetsAction);
    }

    private void UpdateAnimationSpeedInCache(Dictionary<string, GameObject> cache)
    {
        foreach (var kvp in cache)
        {
            if (kvp.Value != null)
            {
                // 新类型：SwfClipController.rateScale
                var swfCtrl = kvp.Value.GetComponent<SwfClipController>();
                if (swfCtrl != null)
                {
                    swfCtrl.rateScale = BattleEntityController.BattleAnimationSpeed;
                }

                // 旧类型：Animator.speed
                Animator animator = kvp.Value.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.speed = BattleEntityController.BattleAnimationSpeed;
                }
            }
        }
    }
    #endregion

    #region 清理
    /// <summary>
    /// 清理所有动画相关缓存
    /// </summary>
    public void ClearAllAnimationCaches()
    {
        // 清理前先取消SwfClipController停止事件订阅
        ClearAnimationCacheWithUnsubscribe(playerPetsAction);
        ClearAnimationCacheWithUnsubscribe(enemyPetsAction);

        loadedPlayerPetIds.Clear();
        loadedEnemyPetIds.Clear();
        petAnimationTypeCache.Clear();
        animationQueue.Clear();
    }

    private void ClearAnimationCacheWithUnsubscribe(Dictionary<string, GameObject> cache)
    {
        foreach (var kvp in cache)
        {
            if (kvp.Value != null)
            {
                // 取消SwfClipController事件订阅
                var ctrl = kvp.Value.GetComponent<SwfClipController>();
                if (ctrl != null)
                    ctrl.OnStopPlayingEvent -= OnSwfClipActionStopped;

                Object.Destroy(kvp.Value);
            }
        }
        cache.Clear();
    }
    #endregion
}
