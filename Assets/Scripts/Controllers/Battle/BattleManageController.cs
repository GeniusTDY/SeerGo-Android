using QFramework;
using SeerGoGame;
using SeerGoGame.Buffs;
using SeerGoGame.Commands;
using SeerGoGame.Controllers.Battle;
using SeerGoGame.Events;
using SeerGoGame.Helper;
using SeerGoGame.Interfaces;
using SeerGoGame.Managers;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using Random = UnityEngine.Random;

public class BattleManageController : BaseBattleController
{
    // 单例访问
    public static BattleManageController Instance { get; private set; }

    // =============== 字段声明 ===============

    [Header("对战管理器")]
    public PlayerBattleController playerBattleController;  // 玩家战斗控制器
    public MonsterBattleController monsterBattleController;  // 敌方战斗控制器
    public SwitchPetPanelController SwitchPetPanelController;  // 切换精灵面板控制器
    public ItemPanelController ItemPanelController;  // 道具面板控制器
    public LogTextListViewScript logTextManager;  // 战斗日志管理器

    [Header("UI References")]
    public TextMeshProUGUI roundText;  // 回合数显示文本

    [Header("精灵切换事件")]
    public Button SwitchPetsButton;  // 切换精灵按钮
    public Image SwitchPetsButtonImage;  // 切换精灵按钮图片
    public CanvasGroup SwitchPetPanle;  // 切换精灵面板

    [Header("战斗事件")]
    public Button FightButton;  // 战斗按钮
    public Image FightButtonImage;  // 战斗按钮图片
    public CanvasGroup FightSkillPanle;  // 战斗技能面板
    public CanvasGroup FightPetIconPanle;  // 战斗精灵图标面板

    [Header("物品事件")]
    public Button ItemButton;  // 道具按钮
    public Image ItemButtonImage;  // 道具按钮图片
    public CanvasGroup ItemlPanle;  // 道具面板

    [Header("逃跑事件")]
    public Button EscapeButton;  // 逃跑按钮
    public Image EscapeButtonImage;  // 逃跑按钮图片

    [Header("效果动画")]
    public GameObject PlayerOpenPetAnim;  // 玩家放出精灵动画
    public GameObject MonsterOpenPetAnim;  // 敌方放出精灵动画
    public GameObject RecoverHPAnim;  // 恢复HP动画
    public GameObject RecoverPPAnim;  // 恢复PP动画
    public GameObject PlayerCritAnim;  // 玩家暴击动画
    public GameObject MonsterCritAnim;  // 敌方暴击动画
    public GameObject CatchPetAnim;  // 捕捉动画

    [Header("遮罩效果")]
    public CanvasGroup FunctionPanel;  // 功能面板遮罩
    public CanvasGroup SkillPanel;  // 技能面板遮罩

    public Transform FightCanvas;

    // === 状态管理 ===
    private BattleState _state;  // 战斗状态
    private RoundContext _roundContext;  // 回合上下文
    private BattleUIManager uiManager;  // UI管理器
    private BattleAnimationManager animationManager;  // 动画管理器
    private SkillSideEffectManager sideEffectManager;  // 技能副作用管理器

    // === 系统引用 ===
    private IArchiveModel archiveModel;  // 存档数据模型
    private ISkillSystem skillSystem;  // 技能系统
    private BuffSystem buffSystem;  // Buff系统
    private BattleCalcUtilsHelper battleCalcUtilsHelper;  // 伤害计算帮助类
    private IItemModel itemModel;  // 道具模型
    private IPetInfoSystem petInfoSystem;// 精灵信息系统
    // 物品效果管理器
    private ItemEffectManager itemEffectManager;
    // 特性效果管理器
    private NewSeEffectManager newSeEffectManager;

    // === 公共访问器 ===
    public BattleAnimationManager AnimationManager => animationManager;

    // === 常量定义 ===
    private const int SKILL_ACCURACY_RANGE = 100;  // 技能命中范围最大值
    private const int SKILL_CRITRATE_RANGE = 16;  // 技能暴击范围最大值

    public enum SkillEffectsTiming
    {
        OnBeforeDamageCalculation,  // 计算伤害前
        OnAfterDamageCalculation,   // 计算伤害后
        OnBeforeApplyDamage,        // 应用伤害前
        OnAfterApplyDamage,          // 应用伤害后
        OnBeforeCastSkill,           // 施放技能前
        OnAfterCastSkill,            // 施放技能后
        OnSkillHit,                  // 技能命中时
        OnSkillMiss,                 // 技能未命中
        OnBeforeHeal,                // 治疗前
        OnAfterHeal,                 // 治疗后
        OnBeforeStatusEffect,        // 状态效果前
        OnAfterStatusEffect,         // 状态效果后
        OnRoundStart,                // 回合开始
        OnRoundEnd,                  // 回合结束
        OnTurnStart,                 // 回合开始
        OnTurnEnd,                   // 回合结束
        OnAfterBuffTurnEnd,           // buff回合结束处理后
        OnSkillNotHit                 // 技能未命中
    }

    // =============== 生命周期方法 ===============

    /// <summary>
    /// 游戏对象初始化时调用
    /// </summary>
    void Awake()
    {
        // 检查是否已存在实例，避免重复创建
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        _state = new BattleState();
        _roundContext = RoundContext.Current;
        // 确保回合数据已清理（防止从上次战斗残留）
        _roundContext.Reset();

        // 初始化管理器
        sideEffectManager = new SkillSideEffectManager();
        animationManager = new BattleAnimationManager(this, transform, OnAnimationQueueComplete);
        newSeEffectManager = new NewSeEffectManager();
        uiManager = new BattleUIManager(
            SwitchPetsButtonImage,
            FightButtonImage,
            ItemButtonImage,
            EscapeButtonImage,
            FunctionPanel,
            SkillPanel,
            SwitchPetPanle,
            FightPetIconPanle,
            FightSkillPanle,
            ItemlPanle
        );

        // 初始化引用、注册事件、设置按钮监听器
        InitializeReferences();
        RegisterEvents();
        SetupButtonListeners();
        uiManager.Initialize();
    }

    // =============== 初始化方法 ===============

    /// <summary>
    /// 初始化系统引用
    /// </summary>
    private void InitializeReferences()
    {
        if (itemEffectManager == null)
        {
            itemEffectManager = new ItemEffectManager();
        }
        archiveModel = this.GetModel<IArchiveModel>();
        skillSystem = this.GetSystem<ISkillSystem>();
        buffSystem = this.GetSystem<BuffSystem>();
        petInfoSystem = this.GetSystem<IPetInfoSystem>();
        itemModel = this.GetModel<IItemModel>();
        battleCalcUtilsHelper = this.GetSystem<BattleCalcUtilsHelper>();
        //ExternalAssetBundleManager.Instance.SetCustomPath("D:\\Dev\\FFDec\\Test\\Assets\\PC");
        // ExternalAssetBundleManager.Instance.SetCustomPath(Path.Combine(Application.persistentDataPath, "Android/"));
    }

    /// <summary>
    /// 注册事件监听
    /// </summary>
    private void RegisterEvents()
    {
        this.RegisterEvent<FightDataLoadSuccess>(OnFightDataLoad).UnRegisterWhenCurrentSceneUnloaded();
        this.RegisterEvent<BattleUseSkillEvent>(OnBattleUseSkill).UnRegisterWhenCurrentSceneUnloaded();
        this.RegisterEvent<UseItemSuccessEvent>(OnBattleUseItem).UnRegisterWhenCurrentSceneUnloaded();
        this.RegisterEvent<PlayerAttackHitEvent>(OnPlayerAttackHit).UnRegisterWhenCurrentSceneUnloaded();
        this.RegisterEvent<MonsterAttackHitEvent>(OnMonsterAttackHit).UnRegisterWhenCurrentSceneUnloaded();
        this.RegisterEvent<RoundCompleteEvent>(OnRoundComplete).UnRegisterWhenCurrentSceneUnloaded();
        this.RegisterEvent<BattleSwitchPetEvent>(OnBattleSwitchPetClick).UnRegisterWhenCurrentSceneUnloaded();
        this.RegisterEvent<PlayerPetDefeatedEvent>(OnPlayerPetDefeated).UnRegisterWhenCurrentSceneUnloaded();
        this.RegisterEvent<MonsterPetDefeatedEvent>(OnMonsterPetDefeated).UnRegisterWhenCurrentSceneUnloaded();
    }

    /// <summary>
    /// 设置按钮点击监听器
    /// </summary>
    private void SetupButtonListeners()
    {
        SwitchPetsButton.onClick.AddListener(OnSwitchPetsButtonClick);
        FightButton.onClick.AddListener(OnFightButtonClick);
        ItemButton.onClick.AddListener(OnItemButtonClick);
        EscapeButton.onClick.AddListener(OnEscapeButtonClick);
    }

    // =============== 战斗初始化 ===============

    /// <summary>
    /// 战斗数据加载成功事件处理
    /// </summary>
    private void OnFightDataLoad(FightDataLoadSuccess success)
    {
        int battleBGMVolume = archiveModel.CurrentData.SettingsData.BattleBGMVolume;
        AudioManager.Instance.PlayMusicWithSaveVolume("Audio/Fight/FightMusic", battleBGMVolume);
        // 重置回合数据
        _roundContext.Reset();

        _state.IsPlayerTurn = false;
        uiManager.SetMask(false);

        // 获取玩家和敌方精灵ID
        // 玩家寻找下一只血量不为0的宠物
        var petBag = archiveModel.CurrentData.PetBag;
        var currentPet = petBag[_state.CurrentPlayerPetIndex];
        // 取第一个Hp>0宠物的下标
        int availablePetIndex = petBag.FindIndex(pet => pet.CurrentStatus.Hp > 0);

        int playerPetId = availablePetIndex >= 0 ? petBag[availablePetIndex].Id : currentPet.Id;
        _state.CurrentPlayerPetIndex = availablePetIndex;

        // 获取敌方精灵ID
        int enemyPetId = BattleDataManager.SelectedNpc.battle.branch.enemy.pet[_state.CurrentEnemyPetIndex].Id.ToInt();

        // 加载精灵动画
        animationManager.LoadPlayerPet(playerPetId);
        animationManager.LoadEnemyPet(enemyPetId);

        // 初始化敌方数据
        monsterBattleController.InitMonsterData(BattleDataManager.SelectedNpc.battle.branch.enemy.pet);
        monsterBattleController.SetMonsterData(
            BattleDataManager.SelectedNpc.battle.branch.enemy.pet,
            animationManager.EnemyPetsAction,
            _state.CurrentEnemyPetIndex
        );

        // 初始化玩家数据
        playerBattleController.SetPetsData(
            archiveModel.CurrentData,
            animationManager.PlayerPetsAction,
            _state.CurrentPlayerPetIndex
        );

        // 添加战斗日志
        logTextManager.AddPetEnterLog(playerBattleController.GetCurrentPetData().Basic.Name, true);
        logTextManager.AddPetEnterLog(monsterBattleController.GetMonsterDataListByIndex(0).Basic.Name, false);

        // 隐藏玩家精灵，播放出场动画
        TogglePetVisibility(playerPetId, true, false);

        PlayerOpenPetAnim.GetComponent<OneShotAnim>().Play(2.1f, () =>
        {
            TogglePetVisibility(playerPetId, true, true);
            uiManager.SetMask(true);
            _state.IsPlayerTurn = true;
        });

        // 显示敌方精灵
        TogglePetVisibility(enemyPetId, false, true);

        // 更新回合显示
        roundText.text = $"第 {_state.RoundCount} 回合";
        this.SendCommand(new RoundChangeCommand(_state.RoundCount, true));
    }

    // =============== 事件处理 ===============

    /// <summary>
    /// 使用技能事件处理
    /// </summary>
    private void OnBattleUseSkill(BattleUseSkillEvent e)
    {
        if (!_state.CanAcceptInput()) return;

        // 更新UI状态
        uiManager.SetMask(false);
        uiManager.SetButtonSelected(BattleUIManager.ButtonType.Fight);

        // 开始新回合
        _state.StartRound();
        ProcessTurnStart(playerBattleController, monsterBattleController);
        ProcessTurnStart(monsterBattleController, playerBattleController);
        this.SendCommand(new RoundChangeCommand(_state.RoundCount, true));

        // 根据动作类型处理
        if (e._action == "USE_SKILL")
        {
            HandleSkillUseAction(e);
        }
        else if (e._action == "SWITCH_PET")
        {
            HandleSwitchPetAction();
        }
    }

    /// <summary>
    /// 使用道具事件处理
    /// </summary>
    private void OnBattleUseItem(UseItemSuccessEvent e)
    {
        if (!_state.CanAcceptInput()) return;

        SeerGoLogger.Log($"<color=cyan>[BattleManageController] 玩家使用道具ID: {e.ItemId}, 目标精灵索引: {e.PetIndex}</color>");

        // 更新UI状态
        uiManager.SetButtonSelected(BattleUIManager.ButtonType.Fight);
        uiManager.ShowPanel(FightSkillPanle, FightPetIconPanle);
        uiManager.SetMask(false);

        // 开始新回合
        _state.StartRound();
        ProcessTurnStart(playerBattleController, monsterBattleController);
        ProcessTurnStart(monsterBattleController, playerBattleController);
        this.SendCommand(new RoundChangeCommand(_state.RoundCount, true));

        // 处理道具使用
        HandleItemUseAction(e);
    }

    /// <summary>
    /// 玩家攻击命中事件处理
    /// </summary>
    private void OnPlayerAttackHit(PlayerAttackHitEvent e) => HandleAttackHit(true);

    /// <summary>
    /// 敌方攻击命中事件处理
    /// </summary>
    private void OnMonsterAttackHit(MonsterAttackHitEvent e) => HandleAttackHit(false);

    /// <summary>
    /// 回合完成事件处理
    /// </summary>
    private void OnRoundComplete(RoundCompleteEvent e)
    {
        SeerGoLogger.Log($"<color=cyan>[OnRoundComplete] 进入回合完成处理。状态: {_state.GetStateDescription()}</color>");

        if (_state.Phase == BattlePhase.Ended)
        {
            SeerGoLogger.Log("<color=red>[OnRoundComplete] 战斗已结束，跳过回合完成处理</color>");
            return;
        }

        if (_state.IsSwitching())
        {
            SeerGoLogger.Log($"<color=yellow>[OnRoundComplete] 正在切换精灵，跳过回合完成处理。切换类型: {_state.CurrentSwitchType}</color>");
            return;
        }

        // 处理回合结束时的buff效果
        SeerGoLogger.Log("<color=cyan>[OnRoundComplete] 开始处理双方buff...</color>");
        ProcessTurnEnd(playerBattleController, monsterBattleController);
        ProcessTurnEnd(monsterBattleController, playerBattleController);

        if (_roundContext.PlayerSkill != null && _roundContext.IsPlayerSkillHit)
        {
            ApplySkillDamageAndEffects(playerBattleController, monsterBattleController, _roundContext.PlayerSkill, SkillEffectsTiming.OnAfterBuffTurnEnd);
            ApplySeEffectToCaster(playerBattleController, monsterBattleController, _roundContext.PlayerSkill, SkillEffectsTiming.OnAfterBuffTurnEnd);
        }

        if (_roundContext.EnemySkill != null && _roundContext.IsEnemySkillHit)
        {
            ApplySkillDamageAndEffects(monsterBattleController, playerBattleController, _roundContext.EnemySkill, SkillEffectsTiming.OnAfterBuffTurnEnd);
            ApplySeEffectToCaster(monsterBattleController, playerBattleController, _roundContext.EnemySkill, SkillEffectsTiming.OnAfterBuffTurnEnd);
        }

        // 重置回合数据
        _roundContext.Reset();

        // 检查双方精灵状态
        bool playerDefeated = IsPlayerDefeated();
        bool enemyDefeated = IsEnemyDefeated();

        // 双方同时被击败的情况
        if (playerDefeated && enemyDefeated)
        {
            bool playerAllDefeated = AreAllPlayerPetsDefeated();
            bool enemyAllDefeated = AreAllMonstersDefeated();

            if (playerAllDefeated && enemyAllDefeated)
            {
                EndBattle(false);
                return;
            }

            if (playerAllDefeated)
            {
                EndBattle(false);
                return;
            }

            if (enemyAllDefeated)
            {
                EndBattle(true);
                return;
            }

            // 双方都有备用精灵，标记需要切换
            _state.MarkDefeated(true, DefeatType.ByBuff);
            _state.MarkDefeated(false, DefeatType.ByBuff);
            _state.WaitingForPlayerSwitch = true;
            _state.WaitingForEnemySwitch = true;
        }
        else
        {
            // 单方被击败的情况
            if (playerDefeated && AreAllPlayerPetsDefeated())
            {
                EndBattle(false);
                return;
            }

            if (enemyDefeated && AreAllMonstersDefeated())
            {
                EndBattle(true);
                return;
            }

            // 标记单方需要切换精灵
            if (playerDefeated)
            {
                _state.MarkDefeated(true, DefeatType.ByBuff);
                _state.WaitingForPlayerSwitch = true;
            }

            if (enemyDefeated)
            {
                _state.MarkDefeated(false, DefeatType.ByBuff);
                _state.WaitingForEnemySwitch = true;
            }
        }

        // 回合结束，进入下一回合
        _state.EndRound();
        roundText.text = $"第 {_state.RoundCount} 回合";
        this.SendCommand(new RoundChangeCommand(_state.RoundCount, false));

        // 处理精灵切换或开放玩家操作
        if (_state.WaitingForPlayerSwitch || _state.WaitingForEnemySwitch)
        {
            StartCoroutine(HandleSwitchAtRoundStart());
        }
        else
        {
            _state.ResetRoundDefeats();
            uiManager.SetMask(true);
        }
    }

    /// <summary>
    /// 切换精灵点击事件处理
    /// </summary>
    private void OnBattleSwitchPetClick(BattleSwitchPetEvent e)
    {
        if (_state.Phase == BattlePhase.Ended) return;

        // 清除玩家buff并获取需要传递的buff
        var buffsToTransfer = ClearPlayerBuffsOnSwitch();

        // 切换精灵
        SwitchPlayerPet(e.Index, buffsToTransfer);

        // 清理玩家技能使用次数
        _roundContext.ClearAllSkillCounts(true);

        // 根据切换类型处理
        if (_state.CurrentSwitchType == SwitchType.PlayerDefeat)
        {
            HandleDefeatSwitch();
        }
        else if (_state.CurrentSwitchType == SwitchType.PlayerActive)
        {
            HandleActiveSwitch();
        }
    }

    /// <summary>
    /// 玩家精灵被击败事件处理
    /// </summary>
    private void OnPlayerPetDefeated(PlayerPetDefeatedEvent e) =>
        SeerGoLogger.Log("<color=red>[OnPlayerPetDefeated] 玩家精灵被击败事件触发</color>");

    /// <summary>
    /// 敌方精灵被击败事件处理
    /// </summary>
    private void OnMonsterPetDefeated(MonsterPetDefeatedEvent e) =>
        SeerGoLogger.Log("<color=red>[OnMonsterPetDefeated] 敌方精灵被击败事件触发</color>");

    // =============== 回合核心逻辑 ===============

    /// <summary>
    /// 处理使用技能动作
    /// </summary>
    private void HandleSkillUseAction(BattleUseSkillEvent e)
    {
        //_roundContext.Reset();
        // 检查是否有控制类异常
        if (e._skillData != null)
        {
            _roundContext.PlayerSkill = e._skillData;
            _roundContext.PlayerSkillIndex = e._skillIndex;
            //CalculatePlayerSkill();
        }

        // 选择并计算敌方技能，确定先手顺序
        SelectAndCalculateEnemySkill();
        _roundContext.Priority = DetermineTurnOrder();
        animationManager.EnqueueAnimation(ExecuteRoundByTurnOrder());
    }

    /// <summary>
    /// 处理切换精灵动作
    /// </summary>
    private void HandleSwitchPetAction()
    {
        //_roundContext.Reset();
        SelectAndCalculateEnemySkill();
        _roundContext.Priority = TurnPriority.EnemyFirst;  // 切换精灵时敌方先手
        animationManager.EnqueueAnimation(ExecuteRoundByTurnOrder());
    }

    /// <summary>
    /// 处理使用道具动作（战斗画面：动画、弹窗、HP修改）
    /// 注意：道具效果合法性已由 ItemSideEffectManager 校验，
    /// SideEffect 不修改 HP，实际 HP 修改由控制器完成以确保战斗弹窗和药水反转正确
    /// </summary>
    private void HandleItemUseAction(UseItemSuccessEvent e)
    {
        ItemConfig itemConfig = itemModel.GetById(e.ItemId);

        if (itemConfig.type == "healHP")
        {
            if (e.UseSence == "INBATTLE")
            {
                // 从 sideEffectArg 获取恢复量（兼容旧版 effectValue）
                int healAmount = GetHealAmount(itemConfig, e);

                string isReverse = "+";
                // 药水反转：治疗变为扣血
                if (PotionReverseBuffEffect.IsPotionReverse(monsterBattleController))
                {
                    isReverse = "-";
                }
                playerBattleController.TakeDamage(healAmount, isReverse);

                PlayItemEffect(RecoverHPAnim.GetComponent<OneShotAnim>(), 3f);
            }
        }
        else if (itemConfig.type == "healPP")
        {
            if (e.UseSence == "INBATTLE")
            {
                playerBattleController.RefreshSkillButtons();
                PlayItemEffect(RecoverPPAnim.GetComponent<OneShotAnim>(), 2.5f);
            }
        }
        else if (itemConfig.type == "capsule")
        {
            int catchRate = petInfoSystem.GetMonsterById(monsterBattleController.GetCurrentPetData().Id).CatchRate;
            float catchMultiplier = float.TryParse(itemConfig.sideEffectArg, out float sVal) && sVal > 0
                ? sVal : itemConfig.effectValue;
            bool isCatch = battleCalcUtilsHelper.CalcCatchRate(monsterBattleController.GetCurrentPetData(), catchMultiplier, catchRate);
            if (isCatch)
            {
                CatchPetAnim.GetComponent<OneShotAnim>().Play(2f, () =>
                {
                    TogglePetVisibility(monsterBattleController.GetCurrentPetData().Id, false, false);
                    Invoke(nameof(BattleWonByCatch), 2f);
                });
            }
            else
            {
                PlayItemEffect(CatchPetAnim.GetComponent<OneShotAnim>(), 4.5f);
            }
        }
    }

    /// <summary>
    /// 获取HP恢复量：优先 sideEffectArg，兼容旧版
    /// </summary>
    private int GetHealAmount(ItemConfig itemConfig, UseItemSuccessEvent e)
    {
        if (!string.IsNullOrEmpty(itemConfig.sideEffectArg))
        {
            if (int.TryParse(itemConfig.sideEffectArg, out int val) && val > 0)
                return val;
            if (int.TryParse(itemConfig.sideEffectArg, out int full) && full == -1)
                return e.TargetPet.CurrentStatus.MaxHp - e.TargetPet.CurrentStatus.Hp;
        }
        if (itemConfig.type == "heal_full")
            return e.TargetPet.CurrentStatus.MaxHp - e.TargetPet.CurrentStatus.Hp;
        return (int)itemConfig.effectValue;
    }

    /// <summary>
    /// 按照先手顺序执行回合
    /// </summary>
    private IEnumerator ExecuteRoundByTurnOrder()
    {
        _roundContext.PlayerPetId = playerBattleController.GetCurrentPetData().Id;
        _roundContext.EnemyPetId = monsterBattleController.GetCurrentPetId();

        SeerGoLogger.Log($"<color=cyan>[ExecuteRoundByTurnOrder] 开始执行回合。先手: {_roundContext.Priority}</color>");

        if (_roundContext.Priority == TurnPriority.PlayerFirst)
        {
            // 只计算先手方伤害,后手方在执行时才计算
            CalculateSkillDamage(true, _roundContext.PlayerSkill);
            yield return ExecutePlayerFirstRound();
        }
        else
        {
            // 只计算先手方伤害,后手方在执行时才计算
            CalculateSkillDamage(false, _roundContext.EnemySkill);
            yield return ExecuteEnemyFirstRound();
        }

        // _roundContext.Reset() 已在 OnRoundComplete 中调用
    }

    /// <summary>
    /// 执行玩家先手回合
    /// </summary>
    private IEnumerator ExecutePlayerFirstRound()
    {
        // 1. 玩家攻击
        if (_roundContext.PlayerSkill != null)
        {
            yield return PlayPlayerSkillWithEffects(_roundContext.PlayerSkill, _roundContext.PlayerPetId);
        }

        // 2. 检查敌人是否被击败
        bool shouldBreak = false;
        yield return CheckDefeatAndHandle(false, result => shouldBreak = result);
        if (shouldBreak) yield break;

        // 3. 敌人反击 - 只有敌人存活时才计算伤害
        if (!IsPlayerDefeated())
        {
            if (_roundContext.EnemySkill != null)
            {
                // 在执行前才计算敌方伤害,避免提前标记暴击状态
                CalculateSkillDamage(false, _roundContext.EnemySkill);
                yield return PlayMonsterSkillWithEffects(_roundContext.EnemySkill, _roundContext.EnemyPetId);
                yield return CheckDefeatAndHandle(true, result => shouldBreak = result);
                if (shouldBreak) yield break;
            }
        }
    }

    /// <summary>
    /// 执行敌人先手回合
    /// </summary>
    private IEnumerator ExecuteEnemyFirstRound()
    {
        // 1. 敌人攻击
        if (_roundContext.EnemySkill != null)
        {
            yield return PlayMonsterSkillWithEffects(_roundContext.EnemySkill, _roundContext.EnemyPetId);
        }

        // 2. 检查玩家是否被击败
        bool shouldBreak = false;
        yield return CheckDefeatAndHandle(true, result => shouldBreak = result);
        if (shouldBreak) yield break;

        // 3. 玩家反击或敌人切换 - 只有玩家存活时才计算伤害
        if (!IsEnemyDefeated())
        {
            // 检查敌方是否主动切换
            if (_state.CurrentSwitchType == SwitchType.EnemyActive)
            {
                yield return SwitchToNextEnemyPet();
                _state.CurrentSwitchType = SwitchType.None;
                yield break;
            }

            if (_roundContext.PlayerSkill != null)
            {
                // 在执行前才计算玩家伤害,避免提前标记暴击状态
                CalculateSkillDamage(true, _roundContext.PlayerSkill);
                yield return PlayPlayerSkillWithEffects(_roundContext.PlayerSkill, _roundContext.PlayerPetId);
                yield return CheckDefeatAndHandle(false, result => shouldBreak = result);
                if (shouldBreak) yield break;
            }
        }
    }

    /// <summary>
    /// 执行道具使用回合（只有敌方攻击）
    /// </summary>
    private IEnumerator ExecuteItemRound()
    {
        _roundContext.PlayerPetId = playerBattleController.GetCurrentPetData().Id;
        _roundContext.EnemyPetId = monsterBattleController.GetCurrentPetId();

        SeerGoLogger.Log("<color=cyan>玩家使用了道具，敌方开始行动...</color>");
        yield return new WaitForSeconds(0.5f);
        // 药剂反噬检查
        bool shouldBreak = false;
        yield return CheckDefeatAndHandle(true, result => shouldBreak = result);
        if (shouldBreak) yield break;

        if (_roundContext.EnemySkill != null)
        {
            CalculateSkillDamage(false, _roundContext.EnemySkill);
            // 重新检查技能是否有效(可能因控制状态被置空)
            if (_roundContext.EnemySkill != null)
            {
                yield return PlayMonsterSkillWithEffects(_roundContext.EnemySkill, _roundContext.EnemyPetId);

                shouldBreak = false;
                yield return CheckDefeatAndHandle(true, result => shouldBreak = result);
                if (shouldBreak) yield break;
            }
        }

        // _roundContext.Reset() 已在 OnRoundComplete 中调用
    }

    // =============== 技能计算相关 ===============

    /// <summary>
    /// 计算技能伤害
    /// </summary>
    /// <param name="isPlayer">是否为玩家技能</param>
    /// <param name="skillData">技能数据</param>
    private void CalculateSkillDamage(bool isPlayer, SkillData skillData)
    {
        // 是否处于控制状态
        if (ControlTypeBuffEffect.HasControlTypeStatus(isPlayer ? playerBattleController : monsterBattleController))
        {
            if (isPlayer)
            {
                _roundContext.PlayerSkill = null;
                _roundContext.PlayerSkillIndex = -1;
            }
            else
            {
                _roundContext.EnemySkill = null;
                _roundContext.EnemySkillIndex = -1;
            }
            skillData = null;
            return;
        }

        if (skillData == null) return;

        // 技能失效
        if (SkillBlockBuffEffect.IsSkillBlocked(isPlayer ? playerBattleController : monsterBattleController))
        {
            // 2. 标记技能命中状态
            SetDamageValue(isPlayer, 0);
            return;
        }

        if (isPlayer)
        {
            // 技能使用次数
            if (!RoundContext.Current.PlayerSkillUseCount.ContainsKey(skillData.ID.ToInt()))
                RoundContext.Current.ClearAllSkillCounts(true);
            // 初始化技能威力
            _roundContext.PlayerSkillPower = skillData.Power;
        }
        else
        {
            // 技能使用次数
            if (!RoundContext.Current.EnemySkillUseCount.ContainsKey(skillData.ID.ToInt()))
                RoundContext.Current.ClearAllSkillCounts(false);
            // 初始化技能威力
            _roundContext.EnemySkillPower = skillData.Power;
        }

        BattleEntityController caster = isPlayer ? playerBattleController : monsterBattleController;
        BattleEntityController target = isPlayer ? monsterBattleController : playerBattleController;

        // 1. 计算实际命中率并保存到RoundContext（需要先计算，特性效果需要读取）
        int actualAccuracy = GetActualAccuracy(isPlayer, skillData);
        if (isPlayer)
            _roundContext.PlayerSkillAccuracy = actualAccuracy;
        else
            _roundContext.EnemySkillAccuracy = actualAccuracy;

        // 2. 计算暴击率并保存到RoundContext（需要先计算，特性效果需要读取）
        int critRate = caster.GetCritRate(skillData.CritRate);
        if (isPlayer)
            _roundContext.PlayerSkillCritRate = critRate;
        else
            _roundContext.EnemySkillCritRate = critRate;

        // 应用特性效果（在伤害计算前，特性可能修改命中率和暴击率）
        ApplySeEffectToCaster(caster, target, skillData, SkillEffectsTiming.OnBeforeDamageCalculation);
        ApplySeEffectToTarget(target, caster, skillData, SkillEffectsTiming.OnBeforeDamageCalculation);

        ApplySkillDamageAndEffects(caster, target, skillData, SkillEffectsTiming.OnBeforeDamageCalculation);

        // 统一检查各种未命中情况（使用RoundContext中的命中率，允许特性修改）
        int checkAccuracy = isPlayer ? _roundContext.PlayerSkillAccuracy : _roundContext.EnemySkillAccuracy;
        if (!AttackMustHitBuffEffect.HasAttackMustHit(caster) && !skillData.MustHit &&
        ((skillData.Category == 1 && PhyAttackMissBuffEffect.IsPhySkillMiss(target)) ||
        (skillData.Category == 4 && CaAttackMissBuffEffect.IsCaSkillMiss(target)) ||
        !IsProbabilityTriggeredByPercent(checkAccuracy, SKILL_ACCURACY_RANGE)))
        {
            SetDamageValue(isPlayer, -1); // -1 表示未命中
            return;
        }

        // 2. 标记技能命中状态
        MarkSkillHitStatus(isPlayer, true);

        // 3. 判断是否为伤害型技能（非变化技能） 或技能被格挡
        bool isBlocked = BlockBuffEffect.TryBlock(target);
        if (isBlocked || IsNotDamageSkill(skillData))
        {
            // 根据是否抵挡赋值对应的伤害值
            SetDamageValue(isPlayer, isBlocked ? 0 : -2);
            ApplySkillDamageAndEffects(caster, target, skillData, SkillEffectsTiming.OnAfterDamageCalculation);
            ApplySeEffectToTarget(target, caster, skillData, SkillEffectsTiming.OnAfterDamageCalculation);
            return;
        }
        // 清理睡眠buff
        if (isPlayer)
            monsterBattleController.RemoveBuff(8);
        else
            playerBattleController.RemoveBuff(8);

        // 计算基础伤害 + 暴击判断
        int damage = 0;
        int caclCount = isPlayer ? _roundContext.PlayerSkillCaclDamageCount : _roundContext.EnemySkillCaclDamageCount;// 技能伤害计算次数，默认一次

        // 先判断是否暴击,暴击状态在整个连续攻击中保持一致
        bool isCrit = false;
        BattleEntityController casterEntity = isPlayer ? playerBattleController : monsterBattleController;
        bool guaranteedCrit = GuaranteedCritBuffEffect.HasGuaranteedCrit(casterEntity);

        // 使用RoundContext中的暴击率（允许特性效果修改）
        int checkCritRate = isPlayer ? _roundContext.PlayerSkillCritRate : _roundContext.EnemySkillCritRate;
        if (guaranteedCrit || IsProbabilityTriggeredByPercent(checkCritRate, SKILL_CRITRATE_RANGE))
        {
            isCrit = true;
            MarkSkillCritStatus(isPlayer, true);

            if (guaranteedCrit)
            {
                SeerGoLogger.Log($"<color=yellow>[必定暴击] {(isPlayer ? "玩家" : "敌方")}触发必定暴击效果！</color>");
            }
        }

        // 连续攻击每次伤害单独计算,但暴击状态统一
        for (int i = 0; i < caclCount; i++)
        {
            int baseDamage = CalculateBaseDamage(isPlayer, skillData);
            if (isCrit)
            {
                baseDamage *= 2;
            }
            damage += baseDamage;
        }

        // 技能伤害变化
        damage = caster.GetFinalDamage(damage);

        SetDamageValue(isPlayer, damage);
        // 计算伤害后技能效果
        ApplySkillDamageAndEffects(caster, target, skillData, SkillEffectsTiming.OnAfterDamageCalculation);
        ApplySeEffectToTarget(target, caster, skillData, SkillEffectsTiming.OnAfterDamageCalculation);

        // 护盾抵消伤害
        if (isPlayer)
            _roundContext.PlayerDamage = ShieldBuffEffect.CalculateBlock(target, _roundContext.PlayerDamage);
        else
            _roundContext.EnemyDamage = ShieldBuffEffect.CalculateBlock(target, _roundContext.EnemyDamage);
    }

    #region 辅助方法
    private int CalculateBaseDamage(bool isPlayer, SkillData skillData)
    {
        return isPlayer
            ? battleCalcUtilsHelper.GetPlayerPetDamage(
                playerBattleController, monsterBattleController, skillData, skillData.Category)
            : battleCalcUtilsHelper.GetMonsterPetDamage(
                monsterBattleController, playerBattleController, skillData, skillData.Category);
    }

    private int GetActualAccuracy(bool isPlayer, SkillData skillData)
    {
        return isPlayer
            ? playerBattleController.GetAccuracy(skillData.Accuracy)
            : monsterBattleController.GetAccuracy(skillData.Accuracy);
    }

    private bool IsNotDamageSkill(SkillData skillData)
    {
        return skillData.Category == 4; // 4为变化技能
    }


    private void SetDamageValue(bool isPlayer, int value)
    {
        if (isPlayer) _roundContext.PlayerDamage = value;
        else _roundContext.EnemyDamage = value;
    }

    private void MarkSkillHitStatus(bool isPlayer, bool isHit)
    {
        if (isPlayer) _roundContext.IsPlayerSkillHit = isHit;
        else _roundContext.IsEnemySkillHit = isHit;
    }

    private void MarkSkillCritStatus(bool isPlayer, bool isCrit)
    {
        if (isPlayer) _roundContext.IsPlayerSkillCrit = isCrit;
        else _roundContext.IsEnemySkillCrit = isCrit;
    }

    /// <summary>
    /// 应用特性效果到施法者
    /// </summary>
    /// <param name="caster">施法者（自身）</param>
    /// <param name="target">目标</param>
    /// <param name="skillData">技能数据</param>
    /// <param name="timing">触发时机</param>
    private void ApplySeEffectToCaster(BattleEntityController caster, BattleEntityController target, SkillData skillData, SkillEffectsTiming timing)
    {
        if (caster == null || skillData == null) return;

        var petData = caster.GetCurrentPetData();
        if (petData == null) return;

        // 检查是否有特性
        int seId = petData.Feature.FeatureId;
        if (seId <= 0) return;

        // 获取特性配置参数
        var seConfig = NewSeManager.Instance.GetConfigById(seId);
        if (seConfig == null) return;

        string[] args = string.IsNullOrEmpty(seConfig.Args) ? new string[0] : seConfig.Args.Split(' ');
        var skillModel = this.GetModel<ISkillModel>();
        EffectInfo effectInfo = new EffectInfo();

        // 根据时机调用对应的特性效果方法
        switch (timing)
        {
            case SkillEffectsTiming.OnAfterCastSkill:
                newSeEffectManager.OnAfterCastSkill(seId, caster, target, skillData, args, effectInfo, buffSystem);
                break;
            case SkillEffectsTiming.OnBeforeDamageCalculation:
                newSeEffectManager.OnBeforeDamageCalculation(seId, caster, target, skillData, args, effectInfo, buffSystem);
                break;
            case SkillEffectsTiming.OnAfterDamageCalculation:
                newSeEffectManager.OnAfterDamageCalculation(seId, caster, target, skillData, args, effectInfo, buffSystem);
                break;
            case SkillEffectsTiming.OnBeforeApplyDamage:
                newSeEffectManager.OnBeforeApplyDamage(seId, caster, target, skillData, args, effectInfo, buffSystem);
                break;
            case SkillEffectsTiming.OnAfterApplyDamage:
                newSeEffectManager.OnAfterApplyDamage(seId, caster, target, skillData, args, effectInfo, buffSystem);
                break;
            case SkillEffectsTiming.OnAfterBuffTurnEnd:
                newSeEffectManager.OnAfterBuffTurnEnd(seId, caster, target, skillData, args, effectInfo, buffSystem);
                break;
            case SkillEffectsTiming.OnSkillNotHit:
                newSeEffectManager.OnSkillNotHit(seId, caster, target, skillData, args, effectInfo, buffSystem);
                break;
        }
    }

    /// <summary>
    /// 应用目标特性效果（检查被攻击方的特性，如坚硬、回避等）
    /// </summary>
    /// <param name="target">被攻击方</param>
    /// <param name="caster">攻击方</param>
    /// <param name="skillData">技能数据</param>
    /// <param name="timing">触发时机</param>
    private void ApplySeEffectToTarget(BattleEntityController target, BattleEntityController caster, SkillData skillData, SkillEffectsTiming timing)
    {
        if (target == null || skillData == null) return;

        var petData = target.GetCurrentPetData();
        if (petData == null) return;

        // 检查是否有特性
        int seId = petData.Feature.FeatureId;
        if (seId <= 0) return;

        // 获取特性配置参数
        var seConfig = NewSeManager.Instance.GetConfigById(seId);
        if (seConfig == null) return;

        string[] args = string.IsNullOrEmpty(seConfig.Args) ? new string[0] : seConfig.Args.Split(' ');
        EffectInfo effectInfo = new EffectInfo();

        // 根据时机调用对应的特性效果方法
        // 注意：对于目标特性，caster是攻击方，target是被攻击方
        switch (timing)
        {
            case SkillEffectsTiming.OnAfterCastSkill:
                newSeEffectManager.OnAfterCastSkill(seId, target, caster, skillData, args, effectInfo, buffSystem);
                break;
            case SkillEffectsTiming.OnBeforeDamageCalculation:
                newSeEffectManager.OnBeforeDamageCalculation(seId, target, caster, skillData, args, effectInfo, buffSystem);
                break;
            case SkillEffectsTiming.OnAfterDamageCalculation:
                newSeEffectManager.OnAfterDamageCalculation(seId, target, caster, skillData, args, effectInfo, buffSystem);
                break;
            case SkillEffectsTiming.OnBeforeApplyDamage:
                newSeEffectManager.OnBeforeApplyDamage(seId, target, caster, skillData, args, effectInfo, buffSystem);
                break;
            case SkillEffectsTiming.OnAfterApplyDamage:
                newSeEffectManager.OnAfterApplyDamage(seId, target, caster, skillData, args, effectInfo, buffSystem);
                break;
            case SkillEffectsTiming.OnAfterBuffTurnEnd:
                newSeEffectManager.OnAfterBuffTurnEnd(seId, target, caster, skillData, args, effectInfo, buffSystem);
                break;
            case SkillEffectsTiming.OnSkillNotHit:
                newSeEffectManager.OnSkillNotHit(seId, target, caster, skillData, args, effectInfo, buffSystem);
                break;
        }
    }
    #endregion

    /// <summary>
    /// 选择并计算敌方技能
    /// </summary>
    private void SelectAndCalculateEnemySkill()
    {
        var availableSkills = monsterBattleController.GetAvailableSkills();
        // 无技能或正在控制状态
        if (availableSkills == null || availableSkills.Count == 0)
        {
            _roundContext.EnemySkill = null;
            _roundContext.EnemySkillIndex = -1;
            return;
        }
        // 从有PP的技能中随机选择
        var selectedSkill = availableSkills[Random.Range(0, availableSkills.Count)];
        _roundContext.EnemySkillIndex = selectedSkill.index;
        _roundContext.EnemySkill = skillSystem.GetSkillById(selectedSkill.skillId.ToString());

        //CalculateSkillDamage(false, _roundContext.EnemySkill);
    }

    /// <summary>
    /// 判断先手顺序
    /// </summary>
    private TurnPriority DetermineTurnOrder()
    {
        if (_roundContext.PlayerSkill == null || _roundContext.EnemySkill == null)
            return TurnPriority.PlayerFirst;
        // 判断是否有must first
        bool playerHasMustFirst = AttackMustFirstBuffEffect.HasAttackMustFirst(playerBattleController);
        bool enemyHasMustFirst = AttackMustFirstBuffEffect.HasAttackMustFirst(monsterBattleController);

        if (playerHasMustFirst && !enemyHasMustFirst) return TurnPriority.PlayerFirst;
        if (enemyHasMustFirst && !playerHasMustFirst) return TurnPriority.EnemyFirst;

        // 比较先制等级
        _roundContext.PlayerSkillPriority = _roundContext.PlayerSkill.Priority;
        _roundContext.EnemySkillPriority = _roundContext.EnemySkill.Priority;
        // 魔师迪露技能优先级，实际不能这样写，后续优化
        if (PrioritySkillBuffEffect.IsSkillPriority(monsterBattleController))
            _roundContext.EnemySkillPriority = 99999;

        int playerPriority = _roundContext.PlayerSkillPriority;
        int enemyPriority = _roundContext.EnemySkillPriority;

        if (playerPriority != enemyPriority)
            return playerPriority > enemyPriority ? TurnPriority.PlayerFirst : TurnPriority.EnemyFirst;

        // 先制等级相同，比较速度
        int playerSpeed = playerBattleController.GetSpeed();
        int enemySpeed = monsterBattleController.GetSpeed();

        if (playerSpeed != enemySpeed)
            return playerSpeed > enemySpeed ? TurnPriority.PlayerFirst : TurnPriority.EnemyFirst;

        // 速度也相同，随机决定
        return Random.Range(0, 2) == 0 ? TurnPriority.PlayerFirst : TurnPriority.EnemyFirst;
    }

    // =============== 精灵切换相关 ===============

    /// <summary>
    /// 在回合开始时处理精灵切换
    /// </summary>
    private IEnumerator HandleSwitchAtRoundStart()
    {
        SeerGoLogger.Log("<color=cyan>[HandleSwitchAtRoundStart] 开始处理回合开始时的精灵切换</color>");

        // 禁用玩家操作
        uiManager.SetMask(false);
        _state.Phase = BattlePhase.Switching;

        // 处理双方切换（如果双方都需要切换）
        if (_state.WaitingForPlayerSwitch && _state.WaitingForEnemySwitch)
        {
            SeerGoLogger.Log("<color=yellow>[HandleSwitchAtRoundStart] 双方精灵都需要切换</color>");

            // 先切换敌方
            yield return SwitchToNextEnemyPetAfterDefeat();
            _state.WaitingForEnemySwitch = false;
            _state.CurrentSwitchType = SwitchType.None;

            yield return new WaitForSeconds(0.5f);

            // 再显示玩家切换界面
            _state.CurrentSwitchType = SwitchType.PlayerDefeat;
            ShowSwitchPetPanelAfterDefeat();
            // 玩家切换会在OnBattleSwitchPetClick中完成
            yield break;
        }

        // 只有玩家需要切换
        if (_state.WaitingForPlayerSwitch)
        {
            SeerGoLogger.Log("<color=yellow>[HandleSwitchAtRoundStart] 玩家精灵需要切换</color>");
            _state.CurrentSwitchType = SwitchType.PlayerDefeat;
            ShowSwitchPetPanelAfterDefeat();
            // 玩家切换会在OnBattleSwitchPetClick中完成
            yield break;
        }

        // 只有敌方需要切换
        if (_state.WaitingForEnemySwitch)
        {
            SeerGoLogger.Log("<color=yellow>[HandleSwitchAtRoundStart] 敌方精灵需要切换</color>");
            yield return SwitchToNextEnemyPetAfterDefeat();
            _state.WaitingForEnemySwitch = false;
            _state.CurrentSwitchType = SwitchType.None;

            // 敌方切换完成后，开放玩家操作
            _state.ResetRoundDefeats();
            _state.Phase = BattlePhase.Idle;
            _state.IsPlayerTurn = true;
            uiManager.SetMask(true);
            SeerGoLogger.Log("<color=green>[HandleSwitchAtRoundStart] 敌方切换完成，开放玩家操作</color>");
        }
    }

    /// <summary>
    /// 处理被击败后的切换
    /// </summary>
    private void HandleDefeatSwitch()
    {
        _state.CurrentSwitchType = SwitchType.None;
        _state.WaitingForPlayerSwitch = false; // 玩家已完成切换

        // 恢复战斗UI
        uiManager.SetButtonSelected(BattleUIManager.ButtonType.Fight);
        uiManager.ShowPanel(FightSkillPanle, FightPetIconPanle);

        _state.Phase = BattlePhase.Idle;
        _state.IsPlayerTurn = true;

        SeerGoLogger.Log($"<color=cyan>玩家切换完成。等待敌方切换: {_state.WaitingForEnemySwitch}</color>");

        // 检查是否还在等待敌方切换（双方同时死亡的情况）
        if (_state.WaitingForEnemySwitch)
        {
            SeerGoLogger.Log("<color=cyan>玩家已切换，等待敌方切换精灵...</color>");
            StartCoroutine(HandleEnemySwitchAfterPlayerSwitch());
        }
        else
        {
            // 单方死亡，或者敌方已经切换完成，重置击败标记并开放操作
            SeerGoLogger.Log("<color=green>切换完成，开放玩家操作</color>");
            _state.ResetRoundDefeats();
            uiManager.SetMask(true);
        }
    }

    /// <summary>
    /// 玩家切换完成后，处理敌方切换
    /// </summary>
    private IEnumerator HandleEnemySwitchAfterPlayerSwitch()
    {
        if (!_state.WaitingForEnemySwitch) yield break;

        SeerGoLogger.Log("<color=cyan>敌方开始切换精灵...</color>");
        yield return SwitchToNextEnemyPetAfterDefeat();
        _state.WaitingForEnemySwitch = false;

        SeerGoLogger.Log("<color=green>双方切换完成，开放玩家操作</color>");

        // 双方都切换完成，重置击败标记并开放操作
        _state.ResetRoundDefeats();
        _state.Phase = BattlePhase.Idle;
        _state.IsPlayerTurn = true;
        uiManager.SetMask(true);
    }

    /// <summary>
    /// 处理主动切换
    /// </summary>
    private void HandleActiveSwitch()
    {
        _state.CurrentSwitchType = SwitchType.None;
        uiManager.SetButtonSelected(BattleUIManager.ButtonType.Fight);
        uiManager.ShowPanel(FightSkillPanle, FightPetIconPanle);
        this.SendCommand(new BattleUseSkillCommand("Player", _state.CurrentPlayerPetIndex, "SWITCH_PET"));
    }

    /// <summary>
    /// 执行玩家精灵切换
    /// </summary>
    private void SwitchPlayerPet(int newIndex, List<BuffInstance> buffs)
    {
        // 隐藏当前精灵
        TogglePetVisibility(playerBattleController.GetCurrentPetData().Id, true, false);

        // 将传递的buff应用到新精灵
        ApplyTransferredBuffs(playerBattleController, buffs);

        // 更新索引并加载新精灵
        _state.CurrentPlayerPetIndex = newIndex;
        animationManager.LoadPlayerPet(archiveModel.CurrentData.PetBag[_state.CurrentPlayerPetIndex].Id);
        playerBattleController.SetPetsData(archiveModel.CurrentData, animationManager.PlayerPetsAction, _state.CurrentPlayerPetIndex);

        // 添加切换日志
        logTextManager.AddPetEnterLog(playerBattleController.GetCurrentPetData().Basic.Name, true);

        // 显示新精灵
        TogglePetVisibility(playerBattleController.GetCurrentPetData().Id, true, true);
    }

    /// <summary>
    /// 切换到下一个敌人精灵（死亡后自动切换）
    /// </summary>
    private IEnumerator SwitchToNextEnemyPetAfterDefeat()
    {
        _state.CurrentSwitchType = SwitchType.EnemyDefeat;

        int nextIndex = FindNextAliveEnemyIndex();

        if (nextIndex != -1)
        {
            var buffsToTransfer = ClearEnemyBuffsOnSwitch(keepSwitchCountBuffs: true);
            yield return PerformEnemySwitch(nextIndex, buffsToTransfer);
            _state.CurrentSwitchType = SwitchType.None;
        }
        else
        {
            _state.CurrentSwitchType = SwitchType.None;
            BattleWon();  // 所有敌方精灵都被击败，战斗胜利
        }
    }

    /// <summary>
    /// 敌方主动切换精灵（不死亡的情况下切换）
    /// </summary>
    private IEnumerator SwitchToNextEnemyPet()
    {
        var buffsToTransfer = ClearEnemyBuffsOnSwitch(keepSwitchCountBuffs: false);

        int nextIndex = FindNextAliveEnemyIndex();
        if (nextIndex != -1)
        {
            yield return PerformEnemySwitch(nextIndex, buffsToTransfer);
        }
    }

    /// <summary>
    /// 执行敌方精灵切换
    /// </summary>
    private IEnumerator PerformEnemySwitch(int newIndex, List<BuffInstance> buffs)
    {
        yield return new WaitForSeconds(0.3f);
        // 清理敌方技能使用次数
        _roundContext.ClearAllSkillCounts(false);

        // 隐藏当前精灵
        TogglePetVisibility(monsterBattleController.GetCurrentPetId(), false, false);

        // 更新索引
        _state.CurrentEnemyPetIndex = newIndex;
        int newEnemyPetId = BattleDataManager.SelectedNpc.battle.branch.enemy.pet[_state.CurrentEnemyPetIndex].Id.ToInt();

        // 添加切换日志
        logTextManager.AddPetEnterLog(monsterBattleController.GetMonsterDataListByIndex(_state.CurrentEnemyPetIndex).Basic.Name, false);

        ApplyTransferredBuffs(monsterBattleController, buffs);

        // 加载新精灵
        animationManager.LoadEnemyPet(newEnemyPetId);
        monsterBattleController.SetMonsterData(
            BattleDataManager.SelectedNpc.battle.branch.enemy.pet,
            animationManager.EnemyPetsAction,
            _state.CurrentEnemyPetIndex
        );

        // 显示新精灵
        TogglePetVisibility(newEnemyPetId, false, true);
        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>
    /// 查找下一个存活的敌方精灵索引
    /// </summary>
    private int FindNextAliveEnemyIndex()
    {
        var allMonsters = monsterBattleController.GetAllMonsterDataList();

        // 从当前索引+1开始寻找
        for (int i = _state.CurrentEnemyPetIndex + 1; i < allMonsters.Count; i++)
        {
            if (allMonsters[i].CurrentStatus.Hp > 0)
                return i;
        }

        // 从头开始找（跳过当前索引）
        for (int i = 0; i < _state.CurrentEnemyPetIndex; i++)
        {
            if (allMonsters[i].CurrentStatus.Hp > 0)
                return i;
        }

        return -1;  // 没有找到存活的精灵
    }

    /// <summary>
    /// 显示被击败后的切换精灵页面
    /// </summary>
    private void ShowSwitchPetPanelAfterDefeat()
    {
        _state.CurrentSwitchType = SwitchType.PlayerDefeat;
        _state.Phase = BattlePhase.Switching;

        // 显示切换面板
        uiManager.SetButtonSelected(BattleUIManager.ButtonType.SwitchPets);
        uiManager.ShowPanel(SwitchPetPanle);
        SwitchPetPanelController.SetData(archiveModel.CurrentData, _state.CurrentPlayerPetIndex);
    }

    /// <summary>
    /// 将传递的buff应用到新精灵
    /// </summary>
    private void ApplyTransferredBuffs(IBuffable newPet, List<BuffInstance> buffsToTransfer)
    {
        if (buffsToTransfer == null || buffsToTransfer.Count == 0)
            return;

        foreach (var buff in buffsToTransfer)
        {
            newPet.AddBuff(buff);
            SeerGoLogger.Log($"<color=green>[传递Buff] 新精灵获得传递的 {buff.Config.name}，持续{buff.RemainingTurns}回合</color>");
        }
    }

    // =============== Buff管理 ===============

    /// <summary>
    /// 清理切换精灵时的Buff，并收集需要传递给新精灵的buff
    /// </summary>
    private List<BuffInstance> ClearBuffsOnSwitch(IBuffable buffable, bool keepSwitchCountBuffs)
    {
        List<int> toRemove = new List<int>();
        List<BuffInstance> buffsToTransfer = new List<BuffInstance>();

        foreach (var kv in buffable.ActiveBuffs)
        {
            // 如果这是需要传递的buff（switchCount > 0）
            if (keepSwitchCountBuffs && kv.Value.SwitchCount > 0)
            {
                // 复制buff信息，准备传递给新精灵
                var transferBuff = new BuffInstance
                {
                    Config = kv.Value.Config,
                    RemainingTurns = DurationParser.Parse(kv.Value.Config.duration),
                    EffectValue = kv.Value.EffectValue,
                    MaxStackCount = kv.Value.MaxStackCount,
                    Keep = kv.Value.Keep,
                    SwitchCount = kv.Value.SwitchCount - 1, // 递减次数
                    TargetType = kv.Value.TargetType,
                    Args = kv.Value.Args
                };
                buffsToTransfer.Add(transferBuff);
                SeerGoLogger.Log($"<color=cyan>[切换精灵] Buff {kv.Value.Config.name} 将传递给新精灵，剩余切换次数: {transferBuff.SwitchCount}</color>");
            }
            // 清除旧精灵的所有buff
            toRemove.Add(kv.Key);
        }

        foreach (var buffId in toRemove)
        {
            buffable.RemoveBuff(buffId);
        }

        return buffsToTransfer;
    }

    /// <summary>
    /// 清理玩家切换精灵时的Buff，并返回需要传递的buff列表
    /// </summary>
    private List<BuffInstance> ClearPlayerBuffsOnSwitch() => ClearBuffsOnSwitch(playerBattleController, true);

    /// <summary>
    /// 清理敌方切换精灵时的Buff，并返回需要传递的buff列表
    /// </summary>
    private List<BuffInstance> ClearEnemyBuffsOnSwitch(bool keepSwitchCountBuffs) => ClearBuffsOnSwitch(monsterBattleController, keepSwitchCountBuffs);

    /// <summary>
    /// 处理回合开始时的Buff效果
    /// </summary>
    public void ProcessTurnStart(IBuffable character, BattleEntityController opponent) => ProcessTurn(character, opponent, true);

    /// <summary>
    /// 处理回合结束时的Buff效果
    /// </summary>
    public void ProcessTurnEnd(IBuffable character, BattleEntityController opponent) => ProcessTurn(character, opponent, false);

    /// <summary>
    /// 统一处理回合开始/结束时的Buff效果
    /// </summary>
    private void ProcessTurn(IBuffable character, BattleEntityController opponent, bool isTurnStart)
    {
        if (buffSystem == null || character == null) return;

        if (isTurnStart)
            buffSystem.ProcessTurnStart(character, opponent);
        else
            buffSystem.ProcessTurnEnd(character, opponent);
    }

    // =============== 技能效果处理 ===============
    /// <summary>
    /// 应用技能伤害和效果
    /// </summary>
    private void ApplySkillDamageAndEffects(IBuffable caster, IBuffable target, SkillData skillData, SkillEffectsTiming timing)
    {
        var sideEffectIds = skillData.GetSideEffectIds();
        if (sideEffectIds != null && sideEffectIds.Count > 0)
        {
            // 解析参数
            string[] allArgs = string.IsNullOrEmpty(skillData.SideEffectArg) ?
                new string[0] : skillData.SideEffectArg.Split(' ');

            int argIndex = 0;
            var skillModel = this.GetModel<ISkillModel>();

            // 处理每个副作用效果
            foreach (int sideEffectId in sideEffectIds)
            {
                if (skillModel.EffectInfos.TryGetValue(sideEffectId, out var effectInfo))
                {
                    int argsNeeded = effectInfo.argsNum;
                    // 提取当前副作用所需的参数
                    string[] currentArgs;
                    if (argsNeeded > 0 && argIndex + argsNeeded <= allArgs.Length)
                    {
                        currentArgs = new string[argsNeeded];
                        Array.Copy(allArgs, argIndex, currentArgs, 0, argsNeeded);
                        argIndex += argsNeeded;
                    }
                    else
                    {
                        currentArgs = new string[0];
                    }
                    // 根据副作用类型应用buff
                    ApplySideEffectAsBuff(caster, target, sideEffectId, currentArgs, effectInfo, skillData, timing);
                }
            }
        }
    }

    /// <summary>
    /// 将副作用转换为buff应用
    /// </summary>
    private void ApplySideEffectAsBuff(IBuffable caster, IBuffable target, int sideEffectId, string[] args, EffectInfo effectInfo, SkillData skillData, SkillEffectsTiming applyType)
    {
        switch (applyType)
        {
            case SkillEffectsTiming.OnAfterCastSkill:
                sideEffectManager.OnAfterCastSkill(sideEffectId, caster, target, skillData, args, effectInfo, buffSystem);
                break;
            case SkillEffectsTiming.OnBeforeDamageCalculation:
                sideEffectManager.OnBeforeDamageCalculation(sideEffectId, caster, target, skillData, args, effectInfo, buffSystem);
                break;
            case SkillEffectsTiming.OnAfterDamageCalculation:
                sideEffectManager.OnAfterDamageCalculation(sideEffectId, caster, target, skillData, args, effectInfo, buffSystem);
                break;
            case SkillEffectsTiming.OnBeforeApplyDamage:
                sideEffectManager.OnBeforeApplyDamage(sideEffectId, caster, target, skillData, args, effectInfo, buffSystem);
                break;
            case SkillEffectsTiming.OnAfterApplyDamage:
                sideEffectManager.OnAfterApplyDamage(sideEffectId, caster, target, skillData, args, effectInfo, buffSystem);
                break;
            case SkillEffectsTiming.OnAfterBuffTurnEnd:
                sideEffectManager.OnAfterBuffTurnEnd(sideEffectId, caster, target, skillData, args, effectInfo, buffSystem);
                break;
            case SkillEffectsTiming.OnSkillNotHit:
                sideEffectManager.OnSkillNotHit(sideEffectId, caster, target, skillData, args, effectInfo, buffSystem);
                break;
            default:
                SeerGoLogger.LogError($"[技能效果处理] 不支持的applyType: {applyType}");
                break;
        }
        // 使用管理器处理副作用
    }


    // =============== 动画相关 ===============

    /// <summary>
    /// 播放技能动画和效果
    /// </summary>
    private IEnumerator PlaySkillWithEffects(bool isPlayer, SkillData skillData, int petId)
    {
        // 防御性编程:技能数据为null时直接返回
        if (skillData == null)
        {
            SeerGoLogger.LogWarning($"<color=yellow>[PlaySkillWithEffects] {(isPlayer ? "玩家" : "敌方")}技能数据为null,跳过播放</color>");
            yield break;
        }

        var isCrit = isPlayer ? _roundContext.IsPlayerSkillCrit : _roundContext.IsEnemySkillCrit;
        BattleEntityController battleController = isPlayer ? playerBattleController : monsterBattleController;
        var isHit = isPlayer ? _roundContext.IsPlayerSkillHit : _roundContext.IsEnemySkillHit;

        // 暴击动画 + 日志 - 只有伤害技能(Category != 4)才能播放暴击动画
        if (isCrit && skillData.Category != 4)
        {
            var critAnim = isPlayer ? PlayerCritAnim : MonsterCritAnim;
            critAnim.GetComponent<OneShotAnim>().Play(1.2f, () => { });
            // 消除对方强化防御效果
            var targetBuffKey = skillData.Category == 1 ? 10001 : 10003;
            BattleEntityController removeTarget = isPlayer ? monsterBattleController : playerBattleController;
            if (removeTarget.ActiveBuffs.TryGetValue(targetBuffKey, out var buff))
            {
                // 存在该Buff且效果值>0时删除
                if (buff.EffectValue > 0)
                {
                    removeTarget.RemoveBuff(targetBuffKey);
                }
            }
        }
        AddBuffLog(battleController, isPlayer, skillData, isCrit);

        // 扣减PP值
        if (isPlayer)
            DeductPlayerSkillPP();
        else
            DeductEnemySkillPP();

        // 应用技能效果
        if (isHit)
        {
            BattleEntityController caster = battleController;
            BattleEntityController target = isPlayer ? monsterBattleController : playerBattleController;
            ApplySkillDamageAndEffects(caster, target, skillData, SkillEffectsTiming.OnAfterCastSkill);
            ApplySeEffectToCaster(caster, target, skillData, SkillEffectsTiming.OnAfterCastSkill);
        }

        // 播放动画
        if (isPlayer)
        {
            yield return (playerBattleController as PlayerBattleController)?.PlaySkillAnimationCoroutine(skillData, petId);
        }
        else
        {
            yield return (monsterBattleController as MonsterBattleController)?.PlaySkillAnimationCoroutine(skillData, petId);
        }
    }

    /// <summary>
    /// 播放玩家技能动画和效果
    /// </summary>
    private IEnumerator PlayPlayerSkillWithEffects(SkillData skillData, int petId)
        => PlaySkillWithEffects(true, skillData, petId);

    /// <summary>
    /// 播放敌方技能动画和效果
    /// </summary>
    private IEnumerator PlayMonsterSkillWithEffects(SkillData skillData, int petId)
        => PlaySkillWithEffects(false, skillData, petId);

    /// <summary>
    /// 播放受击动画
    /// </summary>
    private IEnumerator PlayHitedAnimation(bool isPlayer, int petId)
    {
        yield return isPlayer ?
            animationManager.PlayPlayerHitedAnimation(petId) :
            animationManager.PlayEnemyHitedAnimation(petId);
    }

    /// <summary>
    /// 播放玩家受击动画
    /// </summary>
    private IEnumerator PlayPlayerHitedAnimation(int petId) => PlayHitedAnimation(true, petId);

    /// <summary>
    /// 播放敌方受击动画
    /// </summary>
    private IEnumerator PlayEnemyHitedAnimation(int petId) => PlayHitedAnimation(false, petId);

    // =============== UI按钮控制 ===============

    /// <summary>
    /// 逃跑按钮点击事件
    /// </summary>
    private void OnEscapeButtonClick() => HandleButtonClick(BattleUIManager.ButtonType.Escape);

    /// <summary>
    /// 道具按钮点击事件
    /// </summary>
    private void OnItemButtonClick()
    {
        if (!_state.CanAcceptInput()) return;
        ResetSwitchPetState();
        uiManager.SetButtonSelected(BattleUIManager.ButtonType.Item);
        uiManager.ShowPanel(ItemlPanle);

        // 设置道具面板数据
        if (ItemPanelController != null)
        {
            // 判断当前敌方精灵是否第一形态，并且可捕捉
            MonsterData monsterData = petInfoSystem.GetMonsterById(monsterBattleController.GetCurrentMonsterData().Id);
            bool isCatchable = monsterData.EvolvesFrom == 0 && monsterData.CatchRate > 0;
            ItemPanelController.SetData(archiveModel.CurrentData, _state.CurrentPlayerPetIndex, isCatchable);
        }
    }

    /// <summary>
    /// 战斗按钮点击事件
    /// </summary>
    private void OnFightButtonClick()
    {
        if (!_state.CanAcceptInput()) return;
        ResetSwitchPetState();
        uiManager.SetButtonSelected(BattleUIManager.ButtonType.Fight);
        uiManager.ShowPanel(FightSkillPanle, FightPetIconPanle);
    }

    /// <summary>
    /// 切换精灵按钮点击事件
    /// </summary>
    private void OnSwitchPetsButtonClick()
    {
        if (_state.Phase == BattlePhase.Ended || _state.CurrentSwitchType == SwitchType.EnemyDefeat) return;
        if (_state.CurrentSwitchType != SwitchType.PlayerDefeat && !_state.IsPlayerTurn) return;

        if (_state.CurrentSwitchType != SwitchType.PlayerDefeat)
        {
            _state.CurrentSwitchType = SwitchType.PlayerActive;
        }

        uiManager.SetButtonSelected(BattleUIManager.ButtonType.SwitchPets);
        uiManager.ShowPanel(SwitchPetPanle);
        SwitchPetPanelController.SetData(archiveModel.CurrentData, _state.CurrentPlayerPetIndex);
    }

    /// <summary>
    /// 逃跑按钮点击处理
    /// </summary>
    private void HandleButtonClick(BattleUIManager.ButtonType buttonType)
    {
        if (!_state.CanAcceptInput()) return;
        uiManager.SetButtonSelected(buttonType);
        ResetSwitchPetState();
        Dialog2Manager.Instance.ShowPopupOnTransform(
           FightCanvas,
           "",
           "你确定要逃离这次战斗吗？",
           onConfirm: () =>
           {
               BattleLost();
           },
           onCancel: () =>
           {
               // 更新UI状态
               uiManager.SetButtonSelected(BattleUIManager.ButtonType.Fight);
               uiManager.ShowPanel(FightSkillPanle, FightPetIconPanle);
           }
       );
    }

    /// <summary>
    /// 重置切换精灵状态
    /// </summary>
    private void ResetSwitchPetState()
    {
        if (_state.CurrentSwitchType == SwitchType.PlayerActive)
            _state.CurrentSwitchType = SwitchType.None;
    }

    // =============== 辅助方法 ===============

    /// <summary>
    /// 切换精灵显示状态
    /// </summary>
    private void TogglePetVisibility(int petId, bool isPlayer, bool visible)
    {
        var actions = isPlayer ? animationManager.PlayerPetsAction : animationManager.EnemyPetsAction;
        if (actions.TryGetValue($"{petId}_idle", out GameObject pet))
            pet.SetActive(visible);
    }

    /// <summary>
    /// 播放道具效果
    /// </summary>
    private void PlayItemEffect(OneShotAnim anim, float duration)
    {
        anim.Play(duration, () =>
        {
            //_roundContext.Reset();
            SelectAndCalculateEnemySkill();
            _roundContext.Priority = TurnPriority.EnemyFirst;
            animationManager.EnqueueAnimation(ExecuteItemRound());
        });
    }

    /// <summary>
    /// 动画队列完成回调
    /// </summary>
    private void OnAnimationQueueComplete()
    {
        if (_state.Phase != BattlePhase.Ended && !_state.IsSwitching())
        {
            this.SendCommand(new RoundCompleteCommand());
        }
    }

    /// <summary>
    /// 判断概率触发
    /// </summary>
    private bool IsProbabilityTriggeredByPercent(int percent, int maxRange)
    {
        if (percent <= 0) return false;
        int randomValue = Random.Range(1, maxRange + 1);
        return randomValue <= percent;
    }

    /// <summary>
    /// 统一的击败检查和处理
    /// </summary>
    private IEnumerator CheckDefeatAndHandle(bool checkPlayer, System.Action<bool> onComplete)
    {
        bool isDefeated = checkPlayer ? IsPlayerDefeated() : IsEnemyDefeated();
        if (!isDefeated)
        {
            onComplete?.Invoke(false);
            yield break;
        }

        // 标记被技能击败
        _state.MarkDefeated(checkPlayer, DefeatType.BySkill);
        SeerGoLogger.Log($"<color=red>[CheckDefeatAndHandle] {(checkPlayer ? "玩家" : "敌方")}精灵被技能击败！HP<=0</color>");

        bool allDefeated = checkPlayer ? AreAllPlayerPetsDefeated() : AreAllMonstersDefeated();

        if (allDefeated)
        {
            SeerGoLogger.Log($"<color=red>[CheckDefeatAndHandle] {(checkPlayer ? "玩家" : "敌方")}全部精灵被击败，结束战斗</color>");
            EndBattle(!checkPlayer);
            onComplete?.Invoke(true);
            yield break;
        }

        // 标记切换需求
        SeerGoLogger.Log($"<color=yellow>[CheckDefeatAndHandle] {(checkPlayer ? "玩家" : "敌方")}精灵被技能击败，标记切换需求</color>");

        if (checkPlayer)
        {
            _state.WaitingForPlayerSwitch = true;
        }
        else
        {
            _state.WaitingForEnemySwitch = true;
        }

        onComplete?.Invoke(true);
    }

    /// <summary>
    /// 统一的攻击命中处理
    /// </summary>
    private void HandleAttackHit(bool isPlayerAttack)
    {
        if (_state.Phase == BattlePhase.Ended) return;

        BattleEntityController caster = isPlayerAttack ? playerBattleController : monsterBattleController;
        BattleEntityController target = isPlayerAttack ? monsterBattleController : playerBattleController;

        bool isSkillHit = isPlayerAttack ? _roundContext.IsPlayerSkillHit : _roundContext.IsEnemySkillHit;

        if (isPlayerAttack)
        {
            int enemyPetId = monsterBattleController.GetCurrentMonsterData().Id;
            StartCoroutine(PlayEnemyHitedAnimation(enemyPetId));
            // 判断技能是否命中
            if (!_roundContext.IsPlayerSkillHit)
            {
                ApplySkillDamageAndEffects(caster, target, _roundContext.PlayerSkill, SkillEffectsTiming.OnSkillNotHit);
                ApplySeEffectToCaster(caster, target, _roundContext.PlayerSkill, SkillEffectsTiming.OnSkillNotHit);

                // 显示伤害 - 未命中时伤害为-1
                _roundContext.PlayerDamage = -1;
                monsterBattleController.SetMonsterHp(_roundContext.PlayerDamage,
                monsterBattleController.GetCurrentMonsterData());
                return;
            }

            // 应用伤害结算前效果
            ApplySkillDamageAndEffects(caster, target, _roundContext.PlayerSkill, SkillEffectsTiming.OnBeforeApplyDamage);
            ApplySeEffectToCaster(caster, target, _roundContext.PlayerSkill, SkillEffectsTiming.OnBeforeApplyDamage);
            ApplySeEffectToTarget(target, caster, _roundContext.PlayerSkill, SkillEffectsTiming.OnBeforeApplyDamage);
            if (_roundContext.PlayerDamage != -2)
            {
                // Boss特有效果
                // 致死锁血
                if (NearDeathBuffEffect.IsNearDeath(target))
                    _roundContext.PlayerDamage = monsterBattleController.GetCurrentMonsterData().CurrentStatus.Hp - 1;
                // 免伤
                if (ShadowBarrierBuffEffect.IsImmuneToDamage(target))
                    _roundContext.PlayerDamage = 0;
                // 判断对方是否有百倍伤害 魔师迪露
                if (OpponentDamageBuffEffect.IsDamage100X(target) && _roundContext.PlayerSkill.ID.ToInt() != 10036)
                {
                    _roundContext.PlayerDamage *= 100;
                }
                // 技能循环 哈莫雷特
                if (LoopTypeSkillDamageBuffEffect.isContainLoopSkill(target))
                {
                    if (!LoopTypeSkillDamageBuffEffect.IsLoopSkill(target))
                        _roundContext.PlayerDamage = 1;
                }

                // 显示伤害
                monsterBattleController.SetMonsterHp(_roundContext.PlayerDamage,
                monsterBattleController.GetCurrentMonsterData());
            }

            // 应用伤害结算后效果
            ApplySkillDamageAndEffects(caster, target, _roundContext.PlayerSkill, SkillEffectsTiming.OnAfterApplyDamage);
            ApplySeEffectToCaster(caster, target, _roundContext.PlayerSkill, SkillEffectsTiming.OnAfterApplyDamage);
            ApplySeEffectToTarget(target, caster, _roundContext.PlayerSkill, SkillEffectsTiming.OnAfterApplyDamage);

            if (_roundContext.isMonsterFirstDoubleDamage)
                playerBattleController.TakeDamage(_roundContext.PlayerDamage * 2);

            LogAttackResult(_roundContext.IsPlayerSkillHit, "玩家");
        }
        else
        {
            int playerPetId = playerBattleController.GetCurrentPetData().Id;
            StartCoroutine(PlayPlayerHitedAnimation(playerPetId));
            // 魔师迪露低血秒杀
            if (LowHpInstantKillBuffEffect.IsLowHpInstantKill(caster))
            {
                _roundContext.EnemyDamage = playerBattleController.GetCurrentPetData().CurrentStatus.Hp;
                playerBattleController.SetPlayerHp(_roundContext.EnemyDamage,
                    playerBattleController.GetCurrentPetData());
                return;
            }
            // 判断技能是否命中
            if (!_roundContext.IsEnemySkillHit)
            {
                ApplySkillDamageAndEffects(caster, target, _roundContext.EnemySkill, SkillEffectsTiming.OnSkillNotHit);
                ApplySeEffectToCaster(caster, target, _roundContext.EnemySkill, SkillEffectsTiming.OnSkillNotHit);

                // 显示伤害 - 未命中时伤害为-1
                _roundContext.EnemyDamage = -1;
                playerBattleController.SetPlayerHp(_roundContext.EnemyDamage,
                    playerBattleController.GetCurrentPetData());

                return;
            }

            // 应用伤害结算前效果
            ApplySkillDamageAndEffects(caster, target, _roundContext.EnemySkill, SkillEffectsTiming.OnBeforeApplyDamage);
            ApplySeEffectToCaster(caster, target, _roundContext.EnemySkill, SkillEffectsTiming.OnBeforeApplyDamage);
            ApplySeEffectToTarget(target, caster, _roundContext.EnemySkill, SkillEffectsTiming.OnBeforeApplyDamage);
            if (_roundContext.EnemyDamage != -2)
            {
                // Boss特有效果
                // 致死锁血
                if (NearDeathBuffEffect.IsNearDeath(target))
                    _roundContext.EnemyDamage = playerBattleController.GetCurrentPetData().CurrentStatus.Hp - 1;
                // 免伤
                if (ShadowBarrierBuffEffect.IsImmuneToDamage(target))
                    _roundContext.EnemyDamage = 0;

                // 显示伤害
                playerBattleController.SetPlayerHp(_roundContext.EnemyDamage,
                    playerBattleController.GetCurrentPetData());
            }
            // 应用伤害结算后效果
            ApplySkillDamageAndEffects(caster, target, _roundContext.EnemySkill, SkillEffectsTiming.OnAfterApplyDamage);
            ApplySeEffectToCaster(caster, target, _roundContext.EnemySkill, SkillEffectsTiming.OnAfterApplyDamage);
            ApplySeEffectToTarget(target, caster, _roundContext.EnemySkill, SkillEffectsTiming.OnAfterApplyDamage);

            if (_roundContext.isPlayerFirstDoubleDamage)
                monsterBattleController.TakeDamage(_roundContext.EnemyDamage * 2);

            LogAttackResult(_roundContext.IsEnemySkillHit, "敌人");
        }

    }

    /// <summary>
    /// 攻击命中日志
    /// </summary>
    private void LogAttackResult(bool isHit, string attacker)
    {
        if (isHit)
            SeerGoLogger.Log($"<color=yellow>[攻击命中] {attacker}攻击命中</color>");
        else
            SeerGoLogger.Log($"<color=red>[{attacker}攻击未命中]</color>");
    }

    /// <summary>
    /// 扣减玩家技能PP值
    /// </summary>
    private void DeductPlayerSkillPP()
    {
        if (_roundContext.PlayerSkillIndex < 0) return;

        var currentPet = playerBattleController.GetCurrentPetData();
        if (_roundContext.PlayerSkillIndex >= currentPet.Skills.NormalSkill.Count) return;

        int currentPP = currentPet.Skills.NormalSkill[_roundContext.PlayerSkillIndex].currentPP;
        if (currentPP > 0)
        {
            currentPet.Skills.NormalSkill[_roundContext.PlayerSkillIndex].currentPP--;
            playerBattleController.RefreshSkillButtons();  // 刷新技能按钮状态
        }
    }

    /// <summary>
    /// 扣减敌方技能PP值
    /// </summary>
    private void DeductEnemySkillPP()
    {
        // 判断是否有无限PPbuff
        if (InfinitePpBuffEffect.IsInfinitePp(monsterBattleController))
        {
            return;
        }

        if (_roundContext.EnemySkillIndex < 0) return;

        var currentEnemy = monsterBattleController.GetCurrentMonsterData();
        if (currentEnemy == null || _roundContext.EnemySkillIndex >= currentEnemy.Skills.NormalSkill.Count) return;

        int currentPP = currentEnemy.Skills.NormalSkill[_roundContext.EnemySkillIndex].currentPP;
        if (currentPP > 0)
        {
            currentEnemy.Skills.NormalSkill[_roundContext.EnemySkillIndex].currentPP--;
        }
    }

    /// <summary>
    /// 添加Buff日志
    /// </summary>
    public void AddBuffLog(BattleEntityController controller, bool isPlayer, SkillData skillData, bool isCrit)
    {
        var sortedDictionary = controller.GetCurrentSortBuffDict();
        string stateChange = isCrit ? "打出了致命一击," : "";
        string desc = "正常";

        if (sortedDictionary?.Count > 0)
        {
            int remainingTurns = sortedDictionary.First().Value.RemainingTurns;
            if (remainingTurns != -1) desc = $"{sortedDictionary.First().Value.Config.name}:持续{remainingTurns}回合";
        }

        logTextManager.AddStateChangeSkillUseLog(
            controller.GetCurrentPetData().Basic.Name,
            skillData.Name,
            stateChange,
            desc,
            isPlayer
        );
    }

    // =============== 状态检查 ===============

    /// <summary>
    /// 检查玩家当前精灵是否被击败
    /// </summary>
    private bool IsPlayerDefeated()
    {
        var currentPet = playerBattleController.GetCurrentPetData();
        return currentPet.CurrentStatus.Hp <= 0;
    }

    /// <summary>
    /// 检查敌人当前精灵是否被击败
    /// </summary>
    private bool IsEnemyDefeated()
    {
        var currentEnemy = monsterBattleController.GetCurrentMonsterData();
        if (currentEnemy == null) return true;
        return currentEnemy.CurrentStatus.Hp <= 0;
    }

    /// <summary>
    /// 检查所有精灵是否全部被击败
    /// </summary>
    private bool AreAllPetsDefeated(bool checkPlayer)
    {
        if (checkPlayer)
        {
            foreach (var pet in archiveModel.CurrentData.PetBag)
            {
                if (pet.CurrentStatus.Hp > 0) return false;
            }
            return true;
        }
        else
        {
            var allMonsters = monsterBattleController.GetAllMonsterDataList();
            if (allMonsters == null || allMonsters.Count == 0) return true;

            foreach (var monster in allMonsters)
            {
                if (monster != null && monster.CurrentStatus.Hp > 0) return false;
            }
            return true;
        }
    }

    /// <summary>
    /// 检查所有玩家精灵是否全部被击败
    /// </summary>
    private bool AreAllPlayerPetsDefeated() => AreAllPetsDefeated(true);

    /// <summary>
    /// 检查所有敌方精灵是否全部被击败
    /// </summary>
    private bool AreAllMonstersDefeated() => AreAllPetsDefeated(false);

    // =============== 战斗结束 ===============

    /// <summary>
    /// 战斗结束
    /// type: 战斗类型,Normal:正常,Catch:捕获
    /// </summary>
    private void EndBattle(bool isVictory, string typeResult = "Normal")
    {
        if (_state.Phase == BattlePhase.Ended) return;

        _state.EndBattle();
        uiManager.SetMask(false);

        // 停止音乐，使用存档BattleBGMVolume设置音量
        AudioManager.Instance.StopMusic();
        string musicName = isVictory ? "Audio/Fight/FightMusicWin" : "Audio/Fight/FightMusicLose";
        int battleBGMVolume = archiveModel.CurrentData.SettingsData.BattleBGMVolume;
        AudioManager.Instance.PlayMusicWithSaveVolume(musicName, battleBGMVolume, false);

        if (isVictory)
        {
            if (BattleDataManager.CurrentBattleType is BattleDataManager.BattleType.BRAVETOWER or BattleDataManager.BattleType.TRIALTOWER)
            {
                AdvanceTowerFloor(BattleDataManager.CurrentBattleType);
            }
        }

        // 显示战斗结果弹窗
        DialogBattleManager.Instance.ShowPopupOnTransform(
            FightCanvas,
            isVictory ? "胜利" : "失败",
            isVictory
                ? "恭喜你!在这场战斗中获得胜利，继续加油!"
                : "很遗憾!这场较量你输了别气馁，下次一定能赢!",
            isVictory,
            () =>
            {
                // 停止音乐
                AudioManager.Instance.StopMusic();
                // 记录战斗结果
                BattleDataManager.EndBattle(isVictory);
                BattleDataManager.LastPetIndex = _state.CurrentPlayerPetIndex;
                BattleDataManager.CurrentBattleWonType = (typeResult == "Normal") ? BattleDataManager.BattleWonType.NORMAL : BattleDataManager.BattleWonType.CATCHPET;

                if (typeResult == "Catch")
                    BattleDataManager.CatchPetId = monsterBattleController.GetCurrentMonsterData().Id;

                // 存档
                this.SendCommand(new SaveArchiveDataCommand(archiveModel.CurrentData));
                ClearData();
                GameSceneManager.Instance.ChangeScene("Maps");
            }
        );
    }

    private void AdvanceTowerFloor(BattleDataManager.BattleType towerType)
    {
        var yiteData = archiveModel.CurrentData.YiteRogueData;
        int current = 0, history = 0, maxFloor = 0;
        string towerName = "";

        // 根据塔类型获取对应楼层和上限
        switch (towerType)
        {
            case BattleDataManager.BattleType.BRAVETOWER:
                var braveCfg = BraveTowerConfigLoader.LoadConfig();
                maxFloor = braveCfg.FloorCount > 0 ? braveCfg.FloorCount : 60;
                current = yiteData.CurrentBraveTowerFloor;
                history = yiteData.HistoryBraveTowerFloor;
                towerName = "勇者之塔";
                break;
            case BattleDataManager.BattleType.TRIALTOWER:
                var trialCfg = TrialTowerConfigLoader.LoadConfig();
                maxFloor = trialCfg.FloorCount > 0 ? trialCfg.FloorCount : 30;
                current = yiteData.CurrentTrialTowerFloor;
                history = yiteData.HistoryTrialTowerFloor;
                towerName = "试炼之塔";
                break;
            default: return;
        }

        // 已达到最高层，不再推进楼层，但需正确标记通关信息
        if (current >= maxFloor)
        {
            SeerGoLogger.Log($"[{towerName}] 已达到最高层 {maxFloor}，不再推进");
            // 记录实际通关层数，供奖励服务使用（因为current未+1，beatenFloor不能=current-1）
            BattleDataManager.TowerBeatenFloorOverride = maxFloor;
            // 历史+1，使下次进入时IsFirstClear=false，避免重复发放奖励
            if (towerType == BattleDataManager.BattleType.BRAVETOWER)
                yiteData.HistoryBraveTowerFloor = maxFloor + 1;
            else
                yiteData.HistoryTrialTowerFloor = maxFloor + 1;
            // 保存存档
            this.GetSystem<IArchiveSystem>().SaveGame("autosave0", archiveModel.CurrentData);
            return;
        }

        // 推进楼层
        current++;
        if (towerType == BattleDataManager.BattleType.BRAVETOWER)
            yiteData.CurrentBraveTowerFloor = current;
        else
            yiteData.CurrentTrialTowerFloor = current;

        // 更新历史最高层
        if (current > history)
        {
            if (towerType == BattleDataManager.BattleType.BRAVETOWER)
                yiteData.HistoryBraveTowerFloor = current;
            else
                yiteData.HistoryTrialTowerFloor = current;
        }

        // 保存+日志
        this.GetSystem<IArchiveSystem>().SaveGame("autosave0", archiveModel.CurrentData);
        SeerGoLogger.Log($"[{towerName}] 楼层推进: current={current}, history={history}");
    }

    private void ClearData()
    {
        // 销毁技能按钮
        foreach (var item in playerBattleController.GetPlyaerSkillCache().Values)
        {
            if (item != null) Destroy(item);
        }
        // 取消技能效果处理
        sideEffectManager.UnRegisterHandler();
        animationManager.ClearAllAnimationCaches();
    }

    /// <summary>
    /// 战斗胜利
    /// </summary>
    private void BattleWon() => EndBattle(true);
    // 捕捉成功结算
    private void BattleWonByCatch() => EndBattle(true, "Catch");

    /// <summary>
    /// 战斗失败
    /// </summary>
    private void BattleLost() => EndBattle(false);

    private void OnDestroy()
    {
        // 清理静态单例引用
        if (Instance == this)
        {
            Instance = null;
        }

        // 清理管理器
        if (sideEffectManager != null)
        {
            sideEffectManager.UnRegisterHandler();
            sideEffectManager = null;
        }

        if (animationManager != null)
        {
            animationManager.ClearAllAnimationCaches();
            animationManager = null;
        }

        if (uiManager != null)
        {
            uiManager = null;
        }

        // 清理特性管理器
        if (newSeEffectManager != null)
        {
            newSeEffectManager.UnRegisterHandlers();
            newSeEffectManager = null;
        }

        // 清理回合上下文（包括静态实例）
        if (_roundContext != null)
        {
            _roundContext.Reset();
            _roundContext = null;
        }
        // 清除静态实例，防止场景切换后数据残留
        RoundContext.ClearStaticInstance();

        // 清理状态
        if (_state != null)
        {
            _state = null;
        }

        // 清理其他引用
        archiveModel = null;
        skillSystem = null;
        buffSystem = null;
        battleCalcUtilsHelper = null;
        itemModel = null;
        petInfoSystem = null;
        itemEffectManager = null;
        playerBattleController = null;
        monsterBattleController = null;
        SwitchPetPanelController = null;
        ItemPanelController = null;
        logTextManager = null;

        // 反初始化架构
        BattleArchitecture.Interface.Deinit();
    }
}