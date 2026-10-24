using FTRuntime;
using PixeLadder.SimpleTooltip;
using QFramework;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 玩家敌人基类
/// </summary>
public abstract class BattleEntityController : BaseBattleController, IBuffable, IController
{
    /// <summary>
    /// 全局精灵动画倍速（1~3倍，默认1）
    /// 影响玩家和敌方所有精灵动画的播放速度
    /// </summary>
    public static float BattleAnimationSpeed { get; set; } = 3f;

    // IBuffable
    public Dictionary<int, BuffInstance> ActiveBuffs { get; } = new Dictionary<int, BuffInstance>();

    // buff缓存
    protected SortedDictionary<int, BuffInstance> sortedBuffsDict;
    // 能力状态缓存
    protected SortedDictionary<int, BuffInstance> sortedStateDict;

    // buff对象缓存 - 键改为字符串，包含后缀区分正负
    protected Dictionary<string, GameObject> _cachedStateIcons = new Dictionary<string, GameObject>();
    protected Dictionary<string, GameObject> _cachedBuffIcons = new Dictionary<string, GameObject>();

    // buff管理器
    protected BuffEffectManager _buffEffectManager;

    // buff路径
    protected const string BUFF_SPRITE_PATH = "Sprites/Fight/Buffs/";


    protected Dictionary<string, GameObject> _currentPetsAction;
    protected Coroutine _currentAnimationCoroutine;
    protected bool _hitFrameTriggered = false;

    // SwfClip动画完成信号（新类型动画用）
    private bool _swfClipActionCompleted = false;
    private SwfClipController _currentSwfClipController;

    // 技能动画打击帧缓存
    public Dictionary<string, string> _skillAnimationFrameDictionary = new Dictionary<string, string>();

    protected const int PHYSICAL_ATTACK_TYPE = 1;
    protected const int PHYSICAL_SA_TYPE = 2;
    protected const int PHYSICAL_CP_TYPE = 4;
    // buff返回码
    protected const float NO_NEED_ARGS = -10001f;

    protected readonly Dictionary<int, string> _skillAnimationMap = new Dictionary<int, string>
    {
        {PHYSICAL_ATTACK_TYPE, "attack"},
        {PHYSICAL_SA_TYPE, "sa"},
        {PHYSICAL_CP_TYPE, "cp"}
    };

    // 缓存Canvas
    private Canvas _cachedCanvas;

    // 动画帧系统
    protected IAnimationFrameSystem animationFrameSystem;


    protected virtual void Start()
    {
        _cachedCanvas = GameObject.Find("FightCanvas")?.GetComponent<Canvas>();

        if (_cachedCanvas == null)
        {
            SeerGoLogger.LogWarning("Canvas not found!");
        }

        animationFrameSystem = this.GetSystem<IAnimationFrameSystem>();

        if (_buffEffectManager == null)
        {
            _buffEffectManager = new BuffEffectManager();
        }

        sortedBuffsDict = new SortedDictionary<int, BuffInstance>();
        sortedStateDict = new SortedDictionary<int, BuffInstance>();
    }

    protected virtual void OnDestroy()
    {
        ClearCachedIcons();
    }

    /// <summary>
    /// 更新HpUI
    /// </summary>
    public abstract void UpdateHpUI();

    /// <summary>
    /// 获取名字
    /// </summary>
    public abstract string GetEntityName();

    /// <summary>
    /// 获取当前精灵数据
    /// </summary>
    public abstract PetData GetCurrentPetData();

    /// <summary>
    /// Buff状态Panel
    /// </summary>
    protected abstract Transform GetBuffPanelTransform();

    /// <summary>
    /// 能力状态Panel
    /// </summary>
    protected abstract Transform GetStatePanelTransform();

    /// <summary>
    /// 精灵被击败
    /// </summary>
    protected abstract void OnDefeat();

    public SortedDictionary<int, BuffInstance> GetCurrentSortBuffDict()
    {
        return sortedBuffsDict;
    }

    // 添加buff
    public void AddBuff(BuffInstance buff)
    {
        SeerGoLogger.Log($"<color=cyan>[AddBuff] {GetEntityName()} 尝试添加buff: {buff.Config.name}(ID:{buff.Config.id}), 回合数:{buff.RemainingTurns}</color>");

        var targetDict = buff.Config.tab == 9 ? sortedStateDict : sortedBuffsDict;
        BuffInstance buffInstance = null;

        if (ActiveBuffs.TryGetValue(buff.Config.id, out var existingBuff))
        {
            buffInstance = HandleExistingBuff(existingBuff, buff);
        }
        else
        {
            AddNewBuff(buff);
            buffInstance = buff;
        }

        if (buffInstance != null)
        {
            targetDict[buff.Config.id] = buffInstance;
        }

        SeerGoLogger.Log($"<color=green>[AddBuff] {GetEntityName()} buff添加完成。当前ActiveBuffs数量: {ActiveBuffs.Count}</color>");

        // 更新BuffUI
        UpdateBuffUI();
    }

    private BuffInstance HandleExistingBuff(BuffInstance existingBuff, BuffInstance newBuff)
    {
        BuffStackType stackType = newBuff.Config.GetStackType();
        string entityName = GetEntityName();

        switch (stackType)
        {
            case BuffStackType.None:
                SeerGoLogger.Log($"{entityName}已有 {newBuff.Config.name}，不叠加也不刷新");
                return null;

            case BuffStackType.Stack:
                if (newBuff.MaxStackCount == -1)
                {
                    existingBuff.EffectValue += newBuff.EffectValue;
                }
                else
                {
                    existingBuff.EffectValue = Math.Clamp(
                        existingBuff.EffectValue + newBuff.EffectValue,
                        -newBuff.MaxStackCount,
                        newBuff.MaxStackCount);
                }
                existingBuff.RemainingTurns = newBuff.RemainingTurns;
                SeerGoLogger.Log($"{entityName} {newBuff.Config.name} 效果叠加至{existingBuff.EffectValue}, 持续 {existingBuff.RemainingTurns} 回合");
                return existingBuff;

            case BuffStackType.Refresh:
                existingBuff.RemainingTurns = newBuff.RemainingTurns;
                existingBuff.EffectValue = newBuff.EffectValue;
                SeerGoLogger.Log($"{entityName} {newBuff.Config.name} 持续时间刷新为 {existingBuff.RemainingTurns} 回合,数值刷新{existingBuff.EffectValue}");
                return existingBuff;

            case BuffStackType.Replace:
                ActiveBuffs[newBuff.Config.id] = newBuff;
                SeerGoLogger.Log($"{entityName} {newBuff.Config.name}被替换，新持续时间:{newBuff.RemainingTurns} 回合");
                return newBuff;
            default:
                return null;
        }
    }

    private void AddNewBuff(BuffInstance buff)
    {
        ActiveBuffs[buff.Config.id] = buff;
        SeerGoLogger.Log($"{GetEntityName()}获得了{buff.Config.name}持续{buff.RemainingTurns} 回合");
    }

    public void RemoveBuff(int buffId)
    {
        if (ActiveBuffs.Remove(buffId))
        {
            sortedBuffsDict.Remove(buffId);
            sortedStateDict.Remove(buffId);
            SeerGoLogger.Log($"{GetEntityName()}的{buffId}  号Buff已移除");

            UpdateBuffUI();
        }
    }

    protected void UpdateBuffUI()
    {
        HideAllCachedIcons();
        CreateStateIcons();
        CreateBuffIcons();
    }

    private void CreateStateIcons()
    {
        int stateIndex = 0;
        foreach (var kvp in sortedStateDict)
        {
            if (kvp.Value.EffectValue == 0) continue;

            // 为状态图标生成带正负标记的缓存键
            string suffix = kvp.Value.EffectValue > 0 ? "_UP" : "_DOWN";
            string cacheKey = $"{kvp.Key}{suffix}";

            var icon = GetOrCreateBuffIcon(
                GetStatePanelTransform(),
                _cachedStateIcons,
                $"{GetEntityName()}State{kvp.Key}{suffix}",
                kvp.Key,
                kvp.Value,
                suffix
            );

            if (icon != null)
            {
                icon.transform.SetSiblingIndex(stateIndex);
                stateIndex++;
            }
        }
    }

    private void CreateBuffIcons()
    {
        int buffIndex = 0;
        foreach (var kvp in sortedBuffsDict)
        {
            // 普通buff使用id作为缓存键
            string cacheKey = kvp.Key.ToString();

            var icon = GetOrCreateBuffIcon(
                GetBuffPanelTransform(),
                _cachedBuffIcons,
                $"{GetEntityName()}Buff{kvp.Key}",
                kvp.Key,
                kvp.Value
            );

            if (icon != null)
            {
                icon.transform.SetSiblingIndex(buffIndex);
                buffIndex++;
            }
        }
    }

    protected void HideAllCachedIcons()
    {
        foreach (var icon in _cachedStateIcons.Values)
        {
            if (icon != null) icon.SetActive(false);
        }

        foreach (var icon in _cachedBuffIcons.Values)
        {
            if (icon != null) icon.SetActive(false);
        }
    }

    protected GameObject GetOrCreateBuffIcon(Transform parent, Dictionary<string, GameObject> cache,
        string name, int buffId, BuffInstance buff, string suffix = "")
    {
        // 生成缓存键：状态buff使用id+后缀，普通buff只使用id
        string cacheKey = buff.Config.tab == 9 ? $"{buffId}{suffix}" : buffId.ToString();

        if (cache.TryGetValue(cacheKey, out var cachedIcon) && cachedIcon != null)
        {
            cachedIcon.SetActive(true);

            string buffName = buff.Config.name;
            if (buff.Config.tab == 9)
            {
                if (buff.EffectValue > 0) buffName += $"+{buff.EffectValue}";
                else if (buff.EffectValue < 0) buffName += $"{buff.EffectValue}";
            }

            string desc = buff.RemainingTurns != -1 ? $"持续{buff.RemainingTurns}回合" : null;
            TooltipTrigger.AddTooltip(cachedIcon, desc, buffName, Color.yellow, null, _cachedCanvas);

            return cachedIcon;
        }

        return CreateNewBuffIcon(parent, cache, name, buffId, buff, suffix, cacheKey);
    }

    private GameObject CreateNewBuffIcon(Transform parent, Dictionary<string, GameObject> cache,
        string name, int buffId, BuffInstance buff, string suffix, string cacheKey)
    {
        // 不显示
        if (!buff.Config.isShow)
            return null;

        GameObject imageObject = new GameObject(name);
        Image image = imageObject.AddComponent<Image>();

        var sprite = ResourcesManager.Instance.Load<Sprite>(BUFF_SPRITE_PATH + buffId + suffix);
        if (sprite != null)
        {
            image.sprite = sprite;

            string buffName = buff.Config.name;
            if (buff.Config.tab == 9)
            {
                if (buff.EffectValue > 0) buffName += $"+{buff.EffectValue}";
                else if (buff.EffectValue < 0) buffName += $"{buff.EffectValue}";
            }

            string desc = buff.RemainingTurns != -1 ? $"持续{buff.RemainingTurns}回合" : null;
            TooltipTrigger.AddTooltip(imageObject, desc, buffName, Color.yellow, null, _cachedCanvas);
        }
        else
        {
            SeerGoLogger.LogWarning($"未找到buff图标: {BUFF_SPRITE_PATH + buffId + suffix}");
        }

        imageObject.transform.SetParent(parent, false);
        cache[cacheKey] = imageObject;
        return imageObject;
    }

    private void ClearCachedIcons()
    {
        foreach (var icon in _cachedStateIcons.Values)
        {
            if (icon != null) Destroy(icon);
        }

        foreach (var icon in _cachedBuffIcons.Values)
        {
            if (icon != null) Destroy(icon);
        }
        foreach (var action in _currentPetsAction.Values)
        {
            if (action != null)
            {
                // 清理SwfClipController停止事件订阅
                var ctrl = action.GetComponent<SwfClipController>();
                if (ctrl != null)
                    ctrl.OnStopPlayingEvent -= OnSwfClipActionCompleted;
                Destroy(action);
            }
        }

        _currentSwfClipController = null;
        _cachedStateIcons.Clear();
        _cachedBuffIcons.Clear();
        _currentPetsAction.Clear();
    }

    public void OnTurnStart(BattleEntityController opponent)
    {
        SeerGoLogger.Log($"{GetEntityName()}当前有 <color=red>{ActiveBuffs.Count}</color> 个buff");
        ProcessBuffEffects(_buffEffectManager.ProcessTurnStart, opponent);
    }

    public void OnTurnEnd(BattleEntityController opponent)
    {

        SeerGoLogger.Log($"<color=yellow>[OnTurnEnd] {GetEntityName()}开始处理回合结束，当前有{ActiveBuffs.Count}个buff</color>");
        if (ActiveBuffs.Count > 0)
        {
            foreach (var kv in ActiveBuffs)
            {
                SeerGoLogger.Log($"<color=yellow>[OnTurnEnd] Buff详情: {kv.Value.Config.name}(ID:{kv.Key}), 剩余回合:{kv.Value.RemainingTurns}, 类型:{kv.Value.Config.type}</color>");
            }
        }

        ProcessBuffEffects(_buffEffectManager.ProcessTurnEnd, opponent);
        SeerGoLogger.Log($"<color=yellow>[OnTurnEnd] {GetEntityName()} buff效果处理完成</color>");

        RemoveExpiredBuffs();
        SeerGoLogger.Log($"<color=yellow>[OnBeforeTurnStart] {GetEntityName()} 过期buff移除完成，剩余{ActiveBuffs.Count}个buff</color>");
    }

    protected void ProcessBuffEffects(System.Action<IBuffable, BattleEntityController, BuffInstance> processor, BattleEntityController opponent)
    {
        // 使用ToList()创建副本，避免在遍历时修改集合导致异常
        var buffList = ActiveBuffs.Values.ToList();
        foreach (var buff in buffList)
        {
            processor(this, opponent, buff);
        }
    }

    protected void RemoveExpiredBuffs()
    {
        List<int> toRemove = new List<int>();

        foreach (var kv in ActiveBuffs)
        {
            // -1 永久回合   kv.Value.EffectValue != 0确保能力状态为0时，buff才可移除
            if (kv.Value.RemainingTurns == -1 && kv.Value.EffectValue != 0) continue;

            kv.Value.RemainingTurns--;

            if (kv.Value.RemainingTurns <= 0)
            {
                toRemove.Add(kv.Key);
            }
        }

        foreach (var buffId in toRemove)
        {
            RemoveBuff(buffId);
        }
        UpdateBuffUI();
    }
    public int GetMaxHp() => CalculateMaxHpWithBuff(GetCurrentPetData().CurrentStatus.MaxHp, "MAXHP");
    public int GetAtk() => CalculateStatWithBuff(GetCurrentPetData()?.CurrentStatus.Atk ?? 1, "ATK");
    public int GetSpAtk() => CalculateStatWithBuff(GetCurrentPetData()?.CurrentStatus.SpAtk ?? 1, "SPATK");
    public int GetDef() => CalculateStatWithBuff(GetCurrentPetData()?.CurrentStatus.Def ?? 1, "DEF");
    public int GetSpDef() => CalculateStatWithBuff(GetCurrentPetData()?.CurrentStatus.SpDef ?? 1, "SPDEF");
    public int GetSpeed() => CalculateStatWithBuff(GetCurrentPetData()?.CurrentStatus.Spd ?? 1, "SPEED");
    public int GetAccuracy(int accuracy) => CalculateStatWithBuff(accuracy, "ACCURACY");
    public int GetCritRate(int critRate) => CalculateCritRateWithBuff(critRate, "CRITRATE");
    public int GetFinalDamage(int damage) => CalculateDamageWithBuff(damage);

    private int CalculateDamageWithBuff(int damage)
    {
        var damageEffectTypes = new List<string>
            {
                "FIRE_DAMAGE_HALVED",
                "PHYSICAL_DAMAGE_HALVED",
                "SPECIAL_DAMAGE_HALVED",
                "TYPE_DAMAGE_INCREASE",
                "DAMAGE_INCREASE",
                "DAMAGE_REDUCED"
            };

        foreach (var buff in ActiveBuffs.Values)
        {
            foreach (var effectType in damageEffectTypes)
            {
                var multiplier = _buffEffectManager.GetStatMultiplier(this, buff, effectType);

                if (multiplier != NO_NEED_ARGS)
                {
                    damage = (int)(damage * multiplier);
                }
            }
        }

        return damage;
    }

    private int CalculateMaxHpWithBuff(int maxHp, string statType)
    {
        foreach (var buff in ActiveBuffs.Values)
        {
            var statMultiplier = _buffEffectManager.GetStatMultiplier(this, buff, statType);

            var maxHpMultiple = _buffEffectManager.GetStatMultiplier(this, buff, "MAXHP_MULTIPLE");
            if (statMultiplier != NO_NEED_ARGS)
                maxHp += (int)statMultiplier;

            if (maxHpMultiple != NO_NEED_ARGS)
                maxHp = (int)(maxHp * (maxHpMultiple - 1f) / maxHpMultiple);
        }

        return Mathf.Max(maxHp, 1);
    }

    // 计算暴击率
    private int CalculateCritRateWithBuff(int critRate, string statType)
    {
        // 1. 确保critRate在有效范围内
        critRate = Mathf.Clamp(critRate, 1, 16);
        // 2. 计算基础暴击率
        float multiplier = critRate / 16f;
        // 3. 累加所有buff的效果
        foreach (var buff in ActiveBuffs.Values)
        {
            // 暴击率返回-10001不处理
            if (_buffEffectManager.GetStatMultiplier(this, buff, statType) != NO_NEED_ARGS)
                // 累加指定属性的倍率
                multiplier += _buffEffectManager.GetStatMultiplier(this, buff, statType);


            // 额外累加暴击率倍率
            float critMultiplier = _buffEffectManager.GetStatMultiplier(this, buff, "CRITICAL");
            if (critMultiplier != NO_NEED_ARGS)
            {
                multiplier += critMultiplier;
            }
        }
        // 4. 计算最终暴击等级，确保在0-16范围内
        int finalCritRate = Mathf.Clamp(Mathf.FloorToInt(multiplier * 16), 0, 16);
        return finalCritRate;
    }

    protected int CalculateStatWithBuff(int baseStat, string statType)
    {
        float multiplier = 1f;
        foreach (var buff in ActiveBuffs.Values)
        {
            if (_buffEffectManager.GetStatMultiplier(this, buff, statType) != NO_NEED_ARGS)
                multiplier *= _buffEffectManager.GetStatMultiplier(this, buff, statType);
            // BOSS属性加成：各BOSS stat只影响对应属性
            string bossStatType = statType switch
            {
                "ATK" => "BOSS_ATK",
                "DEF" => "BOSS_DEF",
                "SPDEF" => "BOSS_SPDEF",
                "SPATK" => "BOSS_SPATK",
                "SPEED" => "BOSS_SPEED",
                _ => null
            };
            if (bossStatType != null)
            {
                float statMultiplier = _buffEffectManager.GetStatMultiplier(this, buff, bossStatType);
                if (statMultiplier != NO_NEED_ARGS)
                    multiplier *= statMultiplier;
            }

        }
        return Mathf.Max(Mathf.FloorToInt(baseStat * multiplier), 1);
    }


    public virtual void TakeDamage(int damage, string prefix = "-")
    {
        var currentPet = GetCurrentPetData();
        if (currentPet == null) return;

        damage = Mathf.Clamp(damage, 0, int.MaxValue);
        int newHp = Mathf.Clamp(currentPet.CurrentStatus.Hp - damage, 0, GetMaxHp());
        currentPet.CurrentStatus.Hp = newHp;

        UpdateHpUI();

        if (newHp <= 0)
        {
            OnDefeat();
        }
    }

    public virtual void Heal(int amount)
    {
        var currentPet = GetCurrentPetData();
        if (currentPet == null) return;

        int newHp = Mathf.Min(currentPet.CurrentStatus.MaxHp, currentPet.CurrentStatus.Hp + amount);
        currentPet.CurrentStatus.Hp = newHp;

        UpdateHpUI();
    }

    protected IEnumerator PlayAnimationOnce(Dictionary<string, GameObject> petsAction, int petId,
        string animationKey, string animationSuffix)
    {
        string idleKey = $"{petId}_idle";

        // 隐藏idle动画
        if (!petsAction.TryGetValue(idleKey, out GameObject idleObj))
        {
            SeerGoLogger.LogError($"未找到idle动画: {idleKey}");
            yield break;
        }
        idleObj.SetActive(false);

        // 从缓存获取技能动画（已预加载）
        if (!petsAction.TryGetValue(animationKey, out GameObject animationObj))
        {
            SeerGoLogger.LogError($"动画 {animationKey} 未预加载！");
            idleObj.SetActive(true);
            yield break;
        }

        // 判断动画类型：SwfClipFrameEvents存在则走新路径
        SwfClipFrameEvents swfFe = animationObj.GetComponent<SwfClipFrameEvents>();
        if (swfFe != null)
        {
            yield return PlaySwfClipAnimationOnce(animationObj, petId, animationSuffix);
        }
        else
        {
            // 旧路径：Animator
            animationObj.SetActive(true);
            SpriteRenderer spriteRenderer = animationObj.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) spriteRenderer.sortingOrder = 1;
            Animator animator = animationObj.GetComponent<Animator>();

            if (animator != null)
            {
                yield return PlayAnimatorWithHitDetection(spriteRenderer, animator, petId, animationSuffix);
            }
            else
            {
                yield return new WaitForSeconds(1f / BattleAnimationSpeed);
            }

            if (spriteRenderer != null) spriteRenderer.sortingOrder = 0;
            animationObj.SetActive(false);
        }

        // 恢复idle动画时应用倍速
        if (idleObj != null)
        {
            var idleSwfCtrl = idleObj.GetComponent<SwfClipController>();
            if (idleSwfCtrl != null)
            {
                idleSwfCtrl.rateScale = BattleAnimationSpeed;
            }
            Animator idleAnimator = idleObj.GetComponent<Animator>();
            if (idleAnimator != null)
                idleAnimator.speed = BattleAnimationSpeed;
            idleObj.SetActive(true);
        }
    }

    /// <summary>
    /// SwfClip类型动画播放（新类型）
    /// 通过SwfClipFrameEvents触发打击帧，通过SwfClipController.OnStopPlayingEvent检测完成
    /// 每次播放时动态注册打击帧事件（基于HitFramesConfigManager）
    /// </summary>
    private IEnumerator PlaySwfClipAnimationOnce(GameObject animationObj, int petId, string animationSuffix)
    {
        var swfCtrl = animationObj.GetComponent<SwfClipController>();
        var swfClip = animationObj.GetComponent<SwfClip>();
        var swfFe = animationObj.GetComponent<SwfClipFrameEvents>();

        if (swfCtrl == null || swfClip == null)
        {
            SeerGoLogger.LogError($"SwfClip类型动画缺少必要组件: petId={petId}, suffix={animationSuffix}");
            yield return new WaitForSeconds(0.3f / BattleAnimationSpeed);
            animationObj.SetActive(false);
            yield break;
        }

        // 应用倍速
        swfCtrl.rateScale = BattleAnimationSpeed;
        swfCtrl.loopMode = SwfClipController.LoopModes.Once;

        // 清除之前的动态事件，重置帧事件和序列追踪
        if (swfFe != null)
        {
            swfFe.ClearAllEvents();
            swfFe.ResetAll();
            swfFe.OnSequenceChanged();
        }

        // 动态注册打击帧事件（每次播放时注册，避免累积）
        RegisterHitFrameForPlay(swfFe, animationObj, petId, animationSuffix);

        // 回到首帧
        swfClip.ToBeginFrame();

        // 订阅完成事件
        _swfClipActionCompleted = false;
        _currentSwfClipController = swfCtrl;
        swfCtrl.OnStopPlayingEvent += OnSwfClipActionCompleted;

        // 显示并自动播放
        animationObj.SetActive(true);

        // 等待动画播放完成
        while (!_swfClipActionCompleted)
        {
            yield return null;
        }

        // 取消订阅
        swfCtrl.OnStopPlayingEvent -= OnSwfClipActionCompleted;
        _currentSwfClipController = null;

        animationObj.SetActive(false);
    }

    /// <summary>
    /// 为本次播放动态注册打击帧事件（每次只注册一次，播放完ResetAll会清除）
    /// </summary>
    private void RegisterHitFrameForPlay(SwfClipFrameEvents swfFe, GameObject animationObj, int petId, string animationSuffix)
    {
        if (swfFe == null) return;

        int hitFrame = HitFramesConfigManager.Instance.GetHitFrame(petId, animationSuffix);
        if (hitFrame <= 0) return;

        swfFe.AddEvent(hitFrame, true, () =>
        {
            if (!animationObj.activeInHierarchy) return;
            if (!_hitFrameTriggered)
            {
                _hitFrameTriggered = true;
                OnHitFrameTriggered();
            }
        });
    }

    /// <summary>
    /// SwfClip动作播放完成回调
    /// </summary>
    private void OnSwfClipActionCompleted(SwfClipController ctrl)
    {
        if (ctrl == _currentSwfClipController)
        {
            _swfClipActionCompleted = true;
        }
    }

    private IEnumerator PlayAnimatorWithHitDetection(SpriteRenderer spriteRenderer, Animator animator,
        int petId, string animationSuffix)
    {
        // 应用全局倍速
        animator.speed = BattleAnimationSpeed;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float animationLength = stateInfo.length;

        Coroutine checkCoroutine = null;
        if (spriteRenderer != null)
        {
            string hitFrameName = GetSkillHitFrameName(petId, animationSuffix);
            checkCoroutine = StartCoroutine(CheckSpriteEveryFrame(spriteRenderer, hitFrameName));
        }

        // 等待动画实际播放时长（除以倍速）
        yield return new WaitForSeconds(animationLength / BattleAnimationSpeed);

        if (checkCoroutine != null)
        {
            StopCoroutine(checkCoroutine);
        }
    }

    /// <summary>
    /// 逐帧检测精灵动画是否到达打击帧
    /// 使用"越过阈值"检测替代精确匹配，避免倍速时跳帧漏判
    /// </summary>
    protected IEnumerator CheckSpriteEveryFrame(SpriteRenderer spriteRenderer, string targetSpriteName)
    {
        // 解析目标帧号
        if (!int.TryParse(targetSpriteName, out int targetFrameNum))
        {
            yield break;
        }

        int lastFrameNum = -1;

        while (true)
        {
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                string currentSpriteName = spriteRenderer.sprite.name;

                if (int.TryParse(currentSpriteName, out int currentFrameNum))
                {
                    // 判断是否越过了打击帧（无论速度多快，只要帧号从小于目标变成不小于目标就算触发）
                    if (!_hitFrameTriggered && lastFrameNum < targetFrameNum && currentFrameNum >= targetFrameNum)
                    {
                        _hitFrameTriggered = true;
                        OnHitFrameTriggered();
                    }

                    lastFrameNum = currentFrameNum;
                }
            }

            yield return null;
        }
    }

    protected string GetSkillHitFrameName(int petId, string animationSuffix)
    {
        string key = $"{petId}_{animationSuffix}";

        if (_skillAnimationFrameDictionary.ContainsKey(key))
        {
            return _skillAnimationFrameDictionary[key];
        }

        // 先尝试旧的 AnimationFrameSystem（petLabelInfo.json）
        int hitedFrame = animationFrameSystem.GetFrameByLabel(petId, animationSuffix);

        // 如果旧系统没找到，尝试新的 HitFramesConfigManager（hit_frames.csv）
        if (hitedFrame == 0)
        {
            hitedFrame = HitFramesConfigManager.Instance.GetHitFrame(petId, animationSuffix);
        }

        if (hitedFrame != 0)
        {
            _skillAnimationFrameDictionary[key] = hitedFrame.ToString();
        }

        return hitedFrame.ToString();
    }

    /// <summary>
    /// 打击帧触发
    /// </summary>
    protected abstract void OnHitFrameTriggered();
}