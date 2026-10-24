using QFramework;
using SeerGoGame.Commands;
using SeerGoGame.Controller;
using SeerGoGame.Interfaces;
using SeerGoGame.Managers;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

/**
 * NPC点击交互事件
 */
public class NpcCilckController : BaseController, IPointerClickHandler
{
    private MapNpc _mapNpcEntity;
    private MapTeleport _mapTeleportEntity;
    // 定义状态，方便管理
    private enum NPCState { Inactive, Activated }
    private NPCState currentState = NPCState.Inactive;

    // 定义图层名称，在Unity的Layer设置中提前创建好
    private string defaultLayer = "ClickableNPC"; // 默认层：不可交互
    private string activeLayer = "Default";       // 激活层：可交互

    private delegate void TagHandler();
    private Dictionary<string, TagHandler> tagHandlerMap;

    private IMapModel _mapModel;

    // 在Awake或Start中初始化映射关系
    private void InitializeTagHandlers()
    {
        tagHandlerMap = new Dictionary<string, TagHandler>
        {
            { "NPC", HandleNpcInteraction },
            { "teleport", () => StartCoroutine(HandleTeleportWithDelay(0.2f)) },
            { "battle", HandleBattleInteraction },
            { "Item", HandleItemInteraction },
            { "openDialog", HandleDialogInteraction },
            { "openPanel", HandlePanelInteraction }
        };
    }

    // 外部调用的方法：当精灵被玩家点击时激活
    public void ActivateTrigger()
    {
        if (currentState == NPCState.Inactive)
        {
            currentState = NPCState.Activated;
            // 切换到激活图层，使其能与玩家触发交互
            gameObject.layer = LayerMask.NameToLayer(activeLayer);
            //SeerGoLogger.Log("精灵已激活，图层可交互。");
        }
    }

    // 外部调用的方法：当需要取消激活时（如点击其他地方）
    public void DeactivateTrigger()
    {
        if (currentState == NPCState.Activated)
        {
            currentState = NPCState.Inactive;
            // 切回初始图层，避免误触
            gameObject.layer = LayerMask.NameToLayer(defaultLayer);
            //SeerGoLogger.Log("精灵已取消激活，恢复原图层。");
        }
    }

    private void Start()
    {
        _mapModel = this.GetModel<IMapModel>();
        // 设置图层以便区分
        gameObject.layer = LayerMask.NameToLayer("ClickableNPC");
        InitializeTagHandlers();
    }

    public void Initialize(MapNpc mapNpcEntity)
    {
        _mapNpcEntity = mapNpcEntity;
    }
    public void Initialize(MapTeleport mapTeleportEntity)
    {
        _mapTeleportEntity = mapTeleportEntity;
    }

    // 人物移动
    public void OnPointerClick(PointerEventData eventData)
    {
        // 获取坐标
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0; // 确保z坐标为0
        this.SendCommand(new ChangePlayerPositionCommand(mouseWorldPos));

        switch (gameObject.tag)
        {
            case "unopened":
                //SeerGoLogger.Log("点击了未开放");
                break;
            case "teleport":
                // SeerGoLogger.Log("直接处理teleport点击");
                break;
            case "battle":
                //   SeerGoLogger.Log("直接处理battle点击");
                break;
        }
    }

    // 靠近触发
    public void OnCollisionEnter2D(Collision2D collision)
    {
        // SeerGoLogger.Log("OnCollisionEnter2D被调用，碰撞对象: " + (collision.gameObject != null ? collision.gameObject.name : "NULL"));
        if (_mapNpcEntity == null && _mapTeleportEntity == null)
            return;

        string currentTag = gameObject?.tag;

        // 使用字典查找并执行对应的处理逻辑
        if (tagHandlerMap.TryGetValue(currentTag, out TagHandler handler))
        {
            handler.Invoke();
        }
        else
        {
            SeerGoLogger.LogWarning($"未知的标签: {currentTag}");
        }

    }

    // 具体的处理函数
    private void HandleNpcInteraction()
    {
        // 处理NPC交互逻辑
    }

    private void HandleBattleInteraction()
    {
        var archiveModel = this.GetModel<IArchiveModel>();
        // 无存活则返回
        if (archiveModel.CurrentData.PetBag.Count(pet => pet.CurrentStatus.Hp > 0) <= 0)
        {
            Dialog1Manager.Instance.ShowPopupOnTransform(
                Dialog2Manager.Instance.GetMainCanvas(),
                "",
                "请先恢复宠物",
                onConfirm: () =>
                {

                }
            );
            return;
        }

        // 尼尔/尼奥随机遭遇：直接替换NPC对战数据
        TryApplyNeilNioEncounter();

        // 1. 保存NPC数据到BattleDataManager（内部深拷贝）
        BattleDataManager.SelectedNpc = _mapNpcEntity;

        // 2. 保存当前存档数据到BattleDataManager（用于场景切换）
        BattleDataManager.CachedArchiveData = archiveModel.CurrentData;
        // 3.设置当前战斗类型
        if (_mapNpcEntity.battle.branch.enemy.Type == "BOSS")
        {
            BattleDataManager.CurrentBattleType = BattleDataManager.BattleType.BOSS;
        }
        else if (_mapNpcEntity.battle.branch.enemy.Type == "BRAVETOWER")
        {
            BattleDataManager.CurrentBattleType = BattleDataManager.BattleType.BRAVETOWER;
            // 记录是否首次通关：当前楼层==历史最高时，视为首次挑战
            var yite = archiveModel.CurrentData.YiteRogueData;
            BattleDataManager.BraveTowerIsFirstClear = yite.CurrentBraveTowerFloor == yite.HistoryBraveTowerFloor;
            SeerGoLogger.Log($"[勇者之塔] 战斗发起: floor={yite.CurrentBraveTowerFloor}, history={yite.HistoryBraveTowerFloor}, isFirstClear={BattleDataManager.BraveTowerIsFirstClear}");
        }
        else if (_mapNpcEntity.battle.branch.enemy.Type == "TRIALTOWER")
        {
            BattleDataManager.CurrentBattleType = BattleDataManager.BattleType.TRIALTOWER;
            // 记录是否首次通关：当前楼层==历史最高时，视为首次挑战
            var yite = archiveModel.CurrentData.YiteRogueData;
            BattleDataManager.TrialTowerIsFirstClear = yite.CurrentTrialTowerFloor == yite.HistoryTrialTowerFloor;
            SeerGoLogger.Log($"[试炼之塔] 战斗发起: floor={yite.CurrentTrialTowerFloor}, history={yite.HistoryTrialTowerFloor}, isFirstClear={BattleDataManager.TrialTowerIsFirstClear}");
        }
        else
        {
            BattleDataManager.CurrentBattleType = BattleDataManager.BattleType.NORMAL;
        }
        // 4. 设置当前地图ID
        BattleDataManager.CurrentMapId = _mapModel.CurrentMapId.Value;
        // 停止音乐
        AudioManager.Instance.StopMusic();
        // 5. 切换到对战场景
        GameSceneManager.Instance.ChangeScene("Fight");
    }

    /// <summary>
    /// 尼尔/尼奥随机遭遇。仅NORMAL类型且Rate=100的NPC触发。
    /// 尼尔10%，尼奥5%，同时触发优先尼奥。
    /// 直接修改NPC宠物数据，BattleDataManager内部深拷贝所以无需恢复。
    /// </summary>
    private void TryApplyNeilNioEncounter()
    {
        if (_mapNpcEntity == null) return;
        if (_mapNpcEntity.Rate != 100) return;
        if (_mapNpcEntity.battle?.branch?.enemy == null) return;
        if (_mapNpcEntity.battle.branch.enemy.Type != "NORMAL") return;

        int roll = Random.Range(0, 100);

        if (roll < 5)
        {
            // 尼奥 5%
            _mapNpcEntity.battle.branch.enemy.pet = new List<Pet>
            {
                new Pet { Id = "416", Level = 17, LoopSkill = "20097,10194,10001,20096" }
            };
            SeerGoLogger.Log($"[尼尔/尼奥] 触发稀有遭遇: 尼奥(416), roll={roll}");
        }
        else if (roll < 15)
        {
            // 尼尔 10%
            _mapNpcEntity.battle.branch.enemy.pet = new List<Pet>
            {
                new Pet { Id = "77", Level = 16, LoopSkill = "20097,20096,10193" }
            };
            SeerGoLogger.Log($"[尼尔/尼奥] 触发稀有遭遇: 尼尔(77), roll={roll}");
        }
    }

    private void HandleItemInteraction()
    {
        // 处理物品交互逻辑
    }
    // 功能页面
    private void HandlePanelInteraction()
    {
        if (_mapNpcEntity.eventHandler.button.action == "openPanel")
        {
            string panelName = _mapNpcEntity.eventHandler.button.param;
            FunctionPanelLoader.Instance.LoadPanel(panelName, (panel) =>
            {
                if (panel == null) return;

                switch (panelName)
                {
                    case "Shop/ShopPanel":
                        this.SendCommand(new ChangeShopCommand());
                        break;
                    case "SkillRelace/SkillReplacePanel":
                        this.SendCommand(new ChangeSkillReplaceCommand());
                        break;
                    case "Fusion/FusionPanel":
                        this.SendCommand(new ChangeFusionCommand());
                        break;
                    case "BraveTower/BraveTowerPanel":
                        this.SendCommand(new ChangeBraveTowerCommand());
                        break;
                    case "TrialTower/TrialTowerPanel":
                        this.SendCommand(new ChangeTrialTowerCommand());
                        break;
                }
            });
        }
        else if (_mapNpcEntity.eventHandler.button.action == "openPetBag")
        {
            this.GetSystem<IUISystem>().SetUICanvasGroup("PetBagCanvas", true);
            this.SendCommand<ChangePetBagCommand>();
        }
    }
    // 弹窗
    private void HandleDialogInteraction()
    {
        string content = _mapNpcEntity.dialog.content;
        Dialog2Manager.Instance.ShowPopupOnTransform(
           Dialog2Manager.Instance.GetMainCanvas(),
           "",
           content,
           onConfirm: () =>
           {
               // 判断点击事件
               _mapNpcEntity.dialog.functionHandler.func.ForEach((func) =>
               {
                   if (func.action == "battle")
                   {
                       HandleBattleInteraction();
                   }
               });
           },
           onCancel: () =>
           {

           }
       );
    }

    private IEnumerator HandleTeleportWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 发送命令前再次检查实体是否仍然有效
        if (_mapNpcEntity != null)
        {
            this.SendCommand(new NpcClickCommand(_mapNpcEntity));
        }
        else if (_mapTeleportEntity != null)
        {
            this.SendCommand(new NpcClickCommand(_mapTeleportEntity));
        }
    }

}
