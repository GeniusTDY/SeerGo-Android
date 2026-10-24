using QFramework;
using SeerGoGame.Models;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 战斗道具面板控制器
/// </summary>
public class ItemPanelController : BaseBattleController
{
    [Header("UI组件引用")]
    public ItemScrollViewManager itemScrollViewManager;

    private IItemModel itemModel;
    private ArchiveData _archiveData;

    private int _selectedPetIndex = -1;

    private void Start()
    {
        itemModel = this.GetModel<IItemModel>();
    }

    /// <summary>
    /// 设置数据并显示道具列表
    /// </summary>
    public void SetData(ArchiveData archiveData, int currentPetIndex, bool isCatchable)
    {
        _archiveData = archiveData;
        _selectedPetIndex = currentPetIndex;

        CreateItemList(isCatchable);
    }

    /// <summary>
    /// 创建道具列表
    /// </summary>
    private void CreateItemList(bool isCatchable)
    {
        var itemStorage = _archiveData.ItemStorage;
        List<ItemData> itemList = new List<ItemData>();
        foreach (var item in itemStorage)
        {
            // 只显示可以在战斗中使用的道具
            if (itemModel.ItemConfigs.TryGetValue(item.Id, out var config))
            {
                if (itemModel.IsContainScope(config,"INBATTLE") && item.Num > 0)
                {
                    // 判断胶囊道具是否可用
                    if (config.type == "capsule" && (!BattleDataManager.IsNormalBattle || !isCatchable))
                    {
                        continue;
                    }
                    itemList.Add(item);
                }
            }
        }

        if (itemList.Count == 0)
        {
            SeerGoLogger.LogWarning("<color=yellow>背包中没有可用的战斗道具</color>");
        }
        else
        {
            itemScrollViewManager.ReloadData(itemList, _selectedPetIndex);
        }
    }

}
