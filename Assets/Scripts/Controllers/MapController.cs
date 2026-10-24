using PixeLadder.SimpleTooltip;
using QFramework;
using SeerGoGame.Commands;
using SeerGoGame.Controller;
using SeerGoGame.Events;
using SeerGoGame.Interfaces;
using SeerGoGame.Managers;
using SeerGoGame.Models;
using SeerGoGame.Services;  // 添加服务命名空间
using SeerGoGame.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Controllers
{
    /// <summary>
    /// 地图控制器
    /// 负责管理地图加载、显示、NPC交互和战斗后处理
    /// </summary>
    public class MapController : BaseController
    {

        public PlayerController playerController;
        // 系统依赖
        private IMapLoadSystem mapSystem;
        private IUISystem UISystem;
        private IArchiveModel archiveModel;
        private IBattleRewardService battleRewardService; // 战斗奖励服务
        private IItemModel itemModel;

        // Unity组件
        private SpriteRenderer spriteRenderer;
        [field: SerializeField]
        public Transform EffectsParent { get; private set; }

        // 对象管理
        private List<GameObject> spritesList;
        private bool isInitialized = false;

        // 缓存频繁使用的变量
        private Transform cachedTransform;
        private GameObject uiCanvasObject;
        private Canvas uiCanvas;
        private LoadProgressController progressController;

        // 常量定义
        private const string UICANVAS_NAME = "UICanvas";

        /// <summary>
        /// Awake生命周期方法
        /// 初始化组件和获取系统引用
        /// </summary>
        private void Awake()
        {
            // 初始化精灵列表
            spritesList = new List<GameObject>(20);
            cachedTransform = transform;

            // 获取系统引用
            UISystem = this.GetSystem<IUISystem>();
            archiveModel = this.GetModel<IArchiveModel>();
            itemModel = this.GetModel<IItemModel>();

            #if UNITY_ANDROID
                Application.targetFrameRate = 60; // 可选的帧率限制
            #endif

        }

        /// <summary>
        /// 初始化战斗奖励服务
        /// 注入所需的依赖系统
        /// </summary>
        private void InitializeBattleRewardService()
        {
            var petInfoSystem = this.GetSystem<IPetInfoSystem>();
            var expConfigSystem = this.GetSystem<IExpConfigSystem>();
            var petAttributeCalcSystem = this.GetSystem<IPetAttributeCalcSystem>();
            var skillSystem = this.GetSystem<ISkillSystem>();
            var archiveSystem = this.GetSystem<IArchiveSystem>();

            // 创建战斗奖励服务实例
            battleRewardService = new BattleRewardService(
                petInfoSystem,
                expConfigSystem,
                petAttributeCalcSystem,
                archiveModel,
                skillSystem,
                archiveSystem,
                itemModel,
                uiCanvas?.transform
            );

        }

        /// <summary>
        /// 初始化控制器逻辑
        /// </summary>
        private void Start()
        {
            InitializeController();
        }

        /// <summary>
        /// 设置系统引用、注册事件、加载地图
        /// </summary>
        private void InitializeController()
        {
            if (isInitialized) return;

            // 获取系统引用
            mapSystem = this.GetSystem<IMapLoadSystem>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            // 预先查找并缓存重要引用
            CacheImportantReferences();

            // 默认关闭某些UI画布
            UISystem.SetUICanvasGroup("PetBagCanvas", false);
            UISystem.SetUICanvasGroup("NoNoFuncCanvas", false);

            // 获取加载进度控制器实例
            progressController = LoadProgressController.Instance;

            // 加载精灵信息，性格
            progressController.RegisterQueue("spriteInfo", 9);
            this.SendCommand<InitPetInfoDataCommand>();

            // 注册事件处理器
            RegisterEventHandlers();

            // 发送登录后设置命令
            this.SendCommand<SetupAfterLoginCommand>();

            // 加载地图（如果当前有地图ID则使用，否则使用默认地图）
            LoadMap(BattleDataManager.CurrentMapId != 0 ? BattleDataManager.CurrentMapId : -50001);

            isInitialized = true;

            if (FirstLoginHelper.GetStatus())
            {
                OnFirstLoginDialog();
            }
        }

        private ArchiveData CollectCurrentData()
        {
            ArchiveData archiveData = new ArchiveData
            {
                Nickname = "小赛尔",
                PetTotalExp = 999999,
                ItemStorage =
                {
                   new ItemData { Id = 1, Num = 5000 },
                   new ItemData { Id = 2, Num = 5000 }
                }
            };
            archiveModel.CurrentData = archiveData;
            archiveModel.CurrentData.PetTotalExp = 10000;
            archiveData.YiteRogueData.CurrentBraveTowerFloor = 1;
            archiveData.YiteRogueData.HistoryBraveTowerFloor = 1;
            archiveData.YiteRogueData.CurrentTrialTowerFloor = 1;
            archiveData.YiteRogueData.HistoryTrialTowerFloor = 1;
            battleRewardService.ProcessCatchOrGetPetBattleReward(1, true, false);
            battleRewardService.ProcessCatchOrGetPetBattleReward(4, true, false);
            battleRewardService.ProcessCatchOrGetPetBattleReward(7, true, false);
            return archiveData;
        }

        /// <summary>
        /// 缓存重要的场景引用
        /// 减少运行时查找开销
        /// </summary>
        private void CacheImportantReferences()
        {
            uiCanvasObject = GameObject.Find(UICANVAS_NAME);
            if (uiCanvasObject != null)
            {
                uiCanvas = uiCanvasObject.GetComponent<Canvas>();
            }
        }

        /// <summary>
        /// 注册事件处理器
        /// 集中管理所有事件订阅
        /// </summary>
        private void RegisterEventHandlers()
        {
            this.RegisterEvent<MapLoadedEvent>(OnMapLoaded)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<LoadGameDataSuccessEvent>(OnGameDataLoaded)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<NpcClickResultEvent>(OnNpcClickResult)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<AchievementUnlockedEvent>(OnAchievementUnlocked)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<MapEffectsSwitchedEvent>(OnMapEffectsSwitched)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<PetLoadedEvent>(OnPetLoaded).UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnPetLoaded(PetLoadedEvent @event)
        {

            // 判断当前地图是否是勇者之塔
            if (this.GetModel<IMapModel>().CurrentMapId.Value == -50500)
            {
                OnNpcClicked("-50500", "BraveTower", "", cachedTransform, spritesList);
            }
            // 试炼之塔
            if (this.GetModel<IMapModel>().CurrentMapId.Value == -50600)
            {
                OnNpcClicked("-50600", "TrialTower", "", cachedTransform, spritesList);
            }
        }

        private void OnFirstLoginDialog()
        {
            DialogAnnouncementManager.Instance.ShowPopupOnTransform(uiCanvas.transform, "声明",
               "    本单机版《赛尔号》为个人复刻作品，谨向《赛尔号》官方开发及运营团队（淘米公司）致以诚挚敬意！" +
               "复刻初衷源于对经典游戏的热爱，仅用于个人及同好学习交流、技术探讨，无任何商业盈利意图，不替代官方正版。" +
               "在此呼吁所有用户支持官方正版，尊重开发者劳动成果。\n" +
               "1.  本复刻版相关知识产权（含游戏名称、精灵形象、玩法等）均归淘米公司所有，本人未获得官方授权，仅享有非商业学习使用权。\n" +
               "2.  严禁将本复刻版用于出售、引流等商业及违法违规用途，违者自行承担法律责任，本人不承担连带责任。\n" +
               "3.  本复刻版未经过专业测试，可能存在运行漏洞，使用过程中产生的设备故障、数据丢失等损失，本人不承担责任。\n" +
               "4.  若官方提出异议，本人将立即停止复刻版相关分享、使用行为，删除全部相关素材，不承担用户后续损失。\n"
               + "  联系方式:QQ:1145478054");
            FirstLoginHelper.SetStatus(false);
        }

        /// <summary>
        /// 游戏数据加载成功事件处理器
        /// 处理存档加载和战斗结果结算
        /// </summary>
        /// <param name="event">游戏数据加载成功事件</param>
        private void OnGameDataLoaded(LoadGameDataSuccessEvent @event)
        {
            HandleBattleResult();
        }

        /// <summary>
        /// 处理战斗结果
        /// 包括加载存档和战斗胜利结算
        /// </summary>
        private void HandleBattleResult()
        {
            // 初始化战斗奖励服务
            InitializeBattleRewardService();

            // 加载对应的存档
            LoadLatestSave();

            // 处理战斗胜利结算
            if (BattleDataManager.BattleResult &&
                BattleDataManager.CurrentBattleType == BattleDataManager.BattleType.NORMAL)
            {
                SeerGoLogger.Log($"战斗胜利！事件{BattleDataManager.SelectedNpc.battle.branch.winHandler.button.ToString()}");
                ProcessNormalBattleReward();
            }
            else if (BattleDataManager.BattleResult &&
                BattleDataManager.CurrentBattleType == BattleDataManager.BattleType.BOSS)
            {
                // 处理BOSS战斗结果
                SeerGoLogger.Log($"Boss战斗胜利！事件{BattleDataManager.SelectedNpc.battle.branch.winHandler.button.ToString()}");
                ProcessNormalBattleReward();
            }
            else if (BattleDataManager.BattleResult &&
                BattleDataManager.CurrentBattleType == BattleDataManager.BattleType.BRAVETOWER)
            {
                // 处理勇者之塔战斗胜利奖励
                SeerGoLogger.Log("[勇者之塔] 战斗胜利！发放奖励");
                ProcessBraveTowerBattleReward();
            }
            else if (BattleDataManager.BattleResult &&
                BattleDataManager.CurrentBattleType == BattleDataManager.BattleType.TRIALTOWER)
            {
                // 处理试炼之塔战斗胜利奖励
                SeerGoLogger.Log("[试炼之塔] 战斗胜利！发放奖励");
                ProcessTrialTowerBattleReward();
            }

            // 重置战斗数据
            BattleDataManager.ResetBattleData();
        }

        /// <summary>
        /// 加载最新的存档
        /// </summary>
        private void LoadLatestSave()
        {
            var archiveSystem = this.GetSystem<IArchiveSystem>();
            string[] saves = archiveSystem.GetAllSaves();
            // 加载最新的存档
            if (saves.Length > 0)
            {
                this.SendCommand(new LoadGameCommand(saves[0]));
            }
            else
            {
                this.GetSystem<IArchiveSystem>().SaveGame("autosave0", CollectCurrentData());
                this.SendCommand(new LoadGameCommand(archiveSystem.GetAllSaves()[0]));
            }

        }

        /// <summary>
        /// 处理普通PVE战斗奖励
        /// 使用战斗奖励服务进行计算和显示
        /// </summary>
        private void ProcessNormalBattleReward()
        {
            // 参数检查
            bool canProceed = CheckBattleResultCondition();
            if (!canProceed)
            {
                return;
            }

            if (BattleDataManager.CurrentBattleWonType == BattleDataManager.BattleWonType.CATCHPET)
            {
                // 处理抓宠战斗奖励
                battleRewardService.ProcessCatchOrGetPetBattleReward(BattleDataManager.CatchPetId, true);
            }

            // 获取敌方宠物列表和我方宠物数据
            List<Pet> enemyPets = BattleDataManager.SelectedNpc.battle.branch.enemy.pet;
            PetData petData = archiveModel.CurrentData.PetBag[BattleDataManager.LastPetIndex];

            // 使用战斗奖励服务处理奖励
            battleRewardService.ProcessNormalBattleReward(
                petData,
                BattleDataManager.LastPetIndex,
                enemyPets
            );
        }

        /// <summary>
        /// 处理勇者之塔战斗胜利奖励
        /// 发放经验/EV + 楼层奖励 + 推进楼层
        /// </summary>
        private void ProcessBraveTowerBattleReward()
        {
            // 1. 处理战斗基础奖励（经验、EV）
            if (CheckBattleResultCondition())
            {
                List<Pet> enemyPets = BattleDataManager.SelectedNpc.battle.branch.enemy.pet;
                PetData petData = archiveModel.CurrentData.PetBag[BattleDataManager.LastPetIndex];
                battleRewardService.ProcessNormalBattleReward(
                    petData,
                    BattleDataManager.LastPetIndex,
                    enemyPets
                );
            }

            // 2. 发放勇者之塔楼层奖励（赛尔豆、道具、金豆、精灵）
            battleRewardService.ProcessBraveTowerReward();

        }

        private void ProcessTrialTowerBattleReward()
        {
            // 1. 处理战斗基础奖励（经验、EV）
            // 1. 处理战斗基础奖励（经验、EV）
            if (CheckBattleResultCondition())
            {
                List<Pet> enemyPets = BattleDataManager.SelectedNpc.battle.branch.enemy.pet;
                PetData petData = archiveModel.CurrentData.PetBag[BattleDataManager.LastPetIndex];
                battleRewardService.ProcessNormalBattleReward(
                    petData,
                    BattleDataManager.LastPetIndex,
                    enemyPets
                );
            }
            // 2. 发放试炼之塔楼层奖励（赛尔豆、道具、金豆、精灵,经验池）
            battleRewardService.ProcessTrialTowerReward();
        }

        private bool CheckBattleResultCondition()
        {
            if (BattleDataManager.SelectedNpc?.battle?.branch?.enemy?.pet == null ||
                BattleDataManager.SelectedNpc.battle.branch.enemy.pet.Count == 0)
            {
                SeerGoLogger.LogWarning("无法处理战斗奖励：敌方宠物数据为空");
                return false;
            }

            if (BattleDataManager.LastPetIndex < 0 ||
                BattleDataManager.LastPetIndex >= archiveModel.CurrentData.PetBag.Count)
            {
                SeerGoLogger.LogWarning($"无效的宠物索引：{BattleDataManager.LastPetIndex}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 地图特效切换完成事件处理器
        /// </summary>
        /// <param name="e">地图特效切换事件</param>
        private void OnMapEffectsSwitched(MapEffectsSwitchedEvent e)
        {
            // 地图动画切换完成后的逻辑
            // 可以在这里添加额外的特效处理逻辑
        }

        /// <summary>
        /// 加载地图
        /// </summary>
        /// <param name="mapId">地图ID</param>
        public void LoadMap(int mapId)
        {
            if (mapSystem == null)
            {
                SeerGoLogger.LogError("MapSystem is not initialized!");
                return;
            }

            // 切地图前立即强制隐藏所有tooltip，防止遗留显示
            if (TooltipManager.Instance != null)
            {
                TooltipManager.Instance.ForceHideAllImmediate();
            }

            mapSystem.RequestMapLoad(mapId);
        }

        /// <summary>
        /// 销毁所有精灵对象
        /// 清理内存，避免资源泄漏
        /// </summary>
        private void DestroyAllSprites()
        {
            if (spritesList == null || spritesList.Count == 0)
                return;

            // 使用倒序循环避免列表修改问题
            for (int i = spritesList.Count - 1; i >= 0; i--)
            {
                var sprite = spritesList[i];
                if (sprite != null)
                {
                    Destroy(sprite);
                }
            }
            spritesList.Clear();
        }

        /// <summary>
        /// NPC点击事件处理
        /// </summary>
        /// <param name="npcId">NPC ID</param>
        /// <param name="action">动作类型</param>
        /// <param name="parameters">参数</param>
        public void OnNpcClicked(string npcId, string action, string parameters, Transform parent = null, List<GameObject> resultList = null)
        {
            if (string.IsNullOrEmpty(npcId) || mapSystem == null)
                return;

            mapSystem.HandleNpcClick(npcId, action, parameters, parent, resultList);
        }

        /// <summary>
        /// 地图加载完成事件处理器
        /// </summary>
        /// <param name="e">地图加载事件</param>
        private void OnMapLoaded(MapLoadedEvent e)
        {
            if (e.Success)
            {
                MapData currentMap = this.GetModel<IMapModel>().CurrentMap;
                // 停止音乐，使用存档BGMVolume设置音量
                AudioManager.Instance.StopMusic();
                int bgmVolume = archiveModel.CurrentData.SettingsData.BGMVolume;
                AudioManager.Instance.PlayMusicWithSaveVolume($"Audio/Maps/{currentMap.music.bgm}", bgmVolume);

                UpdateMapDisplay(e);

                // 设置加载队列计数
                progressController.RegisterQueue("mapnpc", 2);

                // 加载地图特效
                this.SendCommand(new SwitchMapEffectCommand(e.MapId, progressController, EffectsParent));

                // 切换地图精灵
                this.SendCommand(new ChangeMapSpritesCommand(spritesList, progressController, cachedTransform));

                // 初始化玩家位置
                this.SendCommand(new InitPlayerPositionCommand());

                // Path遮罩图片需要可读纹理（像素级采样），使用 readable: true
                Sprite sprite = ExternalResourcesManager.Instance.LoadSprite($"Maps/Path/{e.MapId}", readable: true);
                playerController.SetMaskSprite(sprite);


            }
            else
            {
                ShowError(e.ErrorMessage);
            }
        }

        /// <summary>
        /// 更新地图显示
        /// 加载并显示新的地图背景（从外部 Resources/ 目录加载）
        /// </summary>
        /// <param name="e">地图加载事件</param>
        private void UpdateMapDisplay(MapLoadedEvent e)
        {
            // 清理旧精灵
            DestroyAllSprites();

            // 加载新地图背景（外部资源）
            string mapPath = ResourcePathBuilder.GetMapBackgroundPath(e.MapId);
            var newSprite = ExternalResourcesManager.Instance.LoadSprite(mapPath);

            if (newSprite != null)
            {
                spriteRenderer.sprite = newSprite;
                // Tiled模式需要纹理wrapMode=Repeat才能平铺
                newSprite.texture.wrapMode = TextureWrapMode.Repeat;
                spriteRenderer.drawMode = SpriteDrawMode.Tiled;
                spriteRenderer.size = new Vector2(18f, 10f);
            }
            else
            {
                // 外部文件加载失败时，保留预制体默认Sprite不变
                SeerGoLogger.LogWarning($"Failed to load sprite at path: {mapPath}, 保留默认背景");
            }

            // 隐藏工具提示
            HideTooltip();
        }

        /// <summary>
        /// NPC点击结果事件处理器
        /// </summary>
        /// <param name="e">NPC点击结果事件</param>
        private void OnNpcClickResult(NpcClickResultEvent e)
        {
            if (e.Success)
            {
                ShowDialogue(e.Message);
            }
            else
            {
                ShowError(e.ErrorMessage);
            }
        }

        /// <summary>
        /// 成就解锁事件处理器
        /// </summary>
        /// <param name="e">成就解锁事件</param>
        private void OnAchievementUnlocked(AchievementUnlockedEvent e)
        {
            ShowAchievement(e.AchievementName);
        }

        /// <summary>
        /// 隐藏工具提示
        /// 使用立即强制隐藏，避免切地图时动画协程失败
        /// </summary>
        private void HideTooltip()
        {
            if (TooltipManager.Instance == null)
            {
                return;
            }

            TooltipManager.Instance.ForceHideAllImmediate();
        }

        /// <summary>
        /// 显示对话
        /// </summary>
        /// <param name="message">对话消息</param>
        private void ShowDialogue(string message)
        {
            // 对话显示逻辑
            // TODO: 实现对话显示
        }

        /// <summary>
        /// 显示错误信息
        /// </summary>
        /// <param name="error">错误消息</param>
        private void ShowError(string error)
        {
            SeerGoLogger.LogError($"显示错误: {error}");
            // TODO: 可以在这里添加UI错误提示
        }

        /// <summary>
        /// 显示成就
        /// </summary>
        /// <param name="achievement">成就名称</param>
        private void ShowAchievement(string achievement)
        {
            // 成就显示逻辑
            // TODO: 实现成就显示
        }

        /// <summary>
        /// Update生命周期方法
        /// 处理调试输入
        /// </summary>
    /*    private void Update()
        {
            HandleDebugInput();
        }*/

        /// <summary>
        /// 处理调试输入
        /// 开发测试用快捷键
        /// </summary>
        private void HandleDebugInput()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                battleRewardService.ProcessCatchOrGetPetBattleReward(-1, false);

            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                LoadMap(-50036);
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                LoadMap(-50033);
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                OnNpcClicked("-5000101", "teleport", "-50314");
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                LoadMap(-50001);
            }
            else if (Input.GetKeyDown(KeyCode.T))
            {
                GameSceneManager.Instance.ChangeScene("Login");
            }
        }

        /// <summary>
        /// OnDestroy生命周期方法
        /// 清理资源和事件
        /// </summary>
        private void OnDestroy()
        {
            // 清理精灵列表
            if (spritesList != null)
            {
                DestroyAllSprites();
                spritesList = null;
            }

            // 取消架构初始化
            SeerGoArchitecture.Interface.Deinit();
        }
    }
}