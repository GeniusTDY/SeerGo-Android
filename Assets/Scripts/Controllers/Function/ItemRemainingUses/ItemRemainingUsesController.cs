using PixeLadder.SimpleTooltip;
using QFramework;
using SeerGoGame.Controller;
using SeerGoGame.Events;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

public class ItemRemainingUsesController : BaseController
{
    [SerializeField] private Image DropRateUses, LearnUses, SeerBeanUses;
    [SerializeField] private TextMeshProUGUI DRNums, LearnNums, SeerBeanNums;

    [Header("中间UI")]
    [SerializeField] private Button CloseButton;

    void Start()
    {
        this.RegisterEvent<RemainingUsesChangeEventOpenEvent>(OnRemainingUsesOpenEvent)
          .UnRegisterWhenGameObjectDestroyed(gameObject);

          CloseButton.onClick.AddListener(OnCloseButtonClick);
    }

    private void OnCloseButtonClick()
    {
        Debug.Log("OnCloseButtonClick");
       FunctionPanelLoader.Instance.ClosePanel("ItemRemainingUses/ItemRemainingUsesPanel");
    }

    private void OnRemainingUsesOpenEvent(RemainingUsesChangeEventOpenEvent @event)
    {
        var archiveModel = this.GetModel<IArchiveModel>();
        var itemModel = this.GetModel<IItemModel>();

        RefreshSingleBuff(
            archiveModel.CurrentData.RewardBuffs,
            itemModel,
            "exp",
            LearnUses.gameObject,
            LearnNums
        );

        RefreshSingleBuff(
            archiveModel.CurrentData.RewardBuffs,
            itemModel,
            "seerBean",
            SeerBeanUses.gameObject,
            SeerBeanNums
        );

        RefreshSingleBuff(
            archiveModel.CurrentData.RewardBuffs,
            itemModel,
            "dropRate",
            DropRateUses.gameObject,
            DRNums
        );
    }

    private void RefreshSingleBuff(
        List<RewardBuffData> rewardBuffs,
        IItemModel itemModel,
        string buffType,
        GameObject targetObj,
        TextMeshProUGUI countText)
    {
        var buff = rewardBuffs.FirstOrDefault(b => b.BuffType == buffType && b.RemainCount > 0);

        if (buff != null)
        {
            var itemConfig = itemModel.GetById(buff.BuffId);
            string itemName = itemConfig != null ? itemConfig.name : $"道具{buff.BuffId}";
            string tooltipContent = $"{itemName}\n剩余 <color=#FFD700>{buff.RemainCount}</color> 次";

            TooltipTrigger.AddTooltip(targetObj, tooltipContent, itemName, Color.yellow, null, FunctionPanelLoader.Instance.transform.GetComponent<Canvas>());
            countText.text = $"×{buff.RemainCount}";
        }
        else
        {
            countText.text = "×0";
        }
    }

}
