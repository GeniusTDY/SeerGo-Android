using PixeLadder.SimpleTooltip;
using QFramework;
using SeerGoGame.Commands;
using SeerGoGame.Controller;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

public class ShopItemCell : BaseController, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] private Image ItemIcon;
    [SerializeField] private Image MoneyIcon;
    [SerializeField] private TextMeshProUGUI ItemName;
    [SerializeField] private TextMeshProUGUI ItemPrice;
    [SerializeField] private Button BuyButton;

    private ItemConfig itemData;
    private System.Action<ItemConfig> onClickCallback;
    private Canvas _uiCanvasCache;
    private bool isInitialized = false;

    private const string BasePath = "Sprites/Item/";
    private const string BaseMoneyPath = "Sprites/Shop/";

    public void Initialize(ItemConfig data, System.Action<ItemConfig> onClick, IArchiveModel _archiveModel, Canvas uiCanvasCache)
    {
        if (data.type == "money")
            return;

        itemData = data;
        onClickCallback = onClick;

        UpdateUI(uiCanvasCache);

        // 只在第一次初始化时注册事件
        if (!isInitialized && BuyButton != null)
        {
            BuyButton.onClick.RemoveListener(() => OnBuyButtonClicked(_archiveModel));

            BuyButton.onClick.AddListener(() => OnBuyButtonClicked(_archiveModel));
            isInitialized = true;
        }
    }

    // 重置方法，用于对象池回收时调用
    public void ResetCell()
    {
        itemData = null;
        onClickCallback = null;

        // 清理UI状态
        if (ItemIcon != null) ItemIcon.sprite = null;
        if (MoneyIcon != null) MoneyIcon.sprite = null;
        if (ItemName != null) ItemName.text = "";
        if (ItemPrice != null) ItemPrice.text = "";
        if (_uiCanvasCache != null) _uiCanvasCache = null;

    }

    private void UpdateUI(Canvas uiCanvasCache)
    {
        if (itemData == null) return;
        _uiCanvasCache = uiCanvasCache;

        Sprite sprite = ResourcesManager.Instance.Load<Sprite>(BasePath + itemData.id.ToString());

        if (ItemIcon != null)
            ItemIcon.sprite = sprite;

        if (ItemName != null)
            ItemName.text = itemData.name;

        if (ItemPrice != null)
            ItemPrice.text = itemData.price.ToString();

        string moneyName = itemData.purchaseMethod == "SEER_BEAN" ? "seerBean" : "goldBean";
        Sprite moneySprite = ResourcesManager.Instance.Load<Sprite>(BaseMoneyPath + moneyName);

        if (MoneyIcon != null)
            MoneyIcon.sprite = moneySprite;

        // 添加tooltip
        if (sprite != null)
        {
            TooltipTrigger.AddTooltip(gameObject, itemData.description, itemData.name,
                Color.yellow, sprite, _uiCanvasCache);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClickCallback?.Invoke(itemData);
    }

    private void OnBuyButtonClicked(IArchiveModel _archiveModel)
    {
        // 1. 获取核心数据
        int currencyId = GetCurrencyIdByPurchaseMethod(itemData.purchaseMethod);
        if (currencyId == -1)
        {
            SeerGoLogger.LogError($"<color=red>[UseItemCommand] 不支持的购买方式：{itemData.purchaseMethod}</color>");
            return;
        }

        List<ItemData> itemStorage = _archiveModel.CurrentData.ItemStorage;
        ItemData currencyItem = itemStorage.FirstOrDefault(item => item.Id == currencyId);
        if (currencyItem == null)
        {
            SeerGoLogger.LogError($"<color=red>[UseItemCommand] 未找到货币ID：{currencyId}</color>");
            ShowInsufficientCurrencyTip(currencyId);
            return;
        }

        // 2. 构建弹窗
        string coinType = GetCurrencyName(currencyId);
        string content = $"1个{itemData.name}需花费<color=red>{itemData.price}</color>个<color=red>{coinType}</color>，你现在拥有<color=red>{currencyItem.Num}</color>个<color=red>{coinType}</color>，要确认购买吗?";

        // 3. 显示购买确认弹窗
        DialogBuyItemManager.Instance.ShowBuyItemPopupOnTransform(
            _uiCanvasCache.transform,
            "",
            content,
            itemData.id,
            1,
            (int count) =>
            {
                // 直接执行购买
                ExecutePurchase(_archiveModel, currencyId, count);
            },
            () => { }); // 取消逻辑为空
    }

    /// <summary>
    /// 合并校验+执行购买的核心逻辑
    /// </summary>
    /// <param name="_archiveModel">存档模型</param>
    /// <param name="currencyId">货币ID</param>
    /// <param name="count">购买数量</param>
    private void ExecutePurchase(IArchiveModel _archiveModel, int currencyId, int count)
    {
        // 获取物品存储列表
        List<ItemData> itemStorage = _archiveModel.CurrentData.ItemStorage;
        ItemData currencyItem = itemStorage.FirstOrDefault(item => item.Id == currencyId);

        // 1. 检查货币是否足够（总价=单价*数量）
        int totalPrice = itemData.price * count;
        if (currencyItem == null || currencyItem.Num < totalPrice)
        {
            SeerGoLogger.LogError($"<color=red>[UseItemCommand] {GetCurrencyName(currencyId)}不足，需要{totalPrice}，当前{currencyItem?.Num ?? 0}</color>");
            ShowInsufficientCurrencyTip(currencyId);
            return;
        }

        // 2. 扣减货币
        currencyItem.Num -= totalPrice;

        // 3. 更新物品数量（存在则累加，不存在则新增）
        ItemData existingItem = itemStorage.FirstOrDefault(item => item.Id == itemData.id);
        if (existingItem != null)
        {
            existingItem.Num += count;
        }
        else
        {
            itemStorage.Add(new ItemData { Id = itemData.id, Num = count });
        }

        // 4. 保存存档
        this.SendCommand(new SaveArchiveDataCommand(_archiveModel.CurrentData));
        Dialog1Manager.Instance.ShowPopupOnTransform(
            _uiCanvasCache.transform,
            "提示",
            $"<color=red>{itemData.name}</color>购买成功！");
    }

    /// <summary>
    /// 根据购买方式获取对应货币ID
    /// </summary>
    private int GetCurrencyIdByPurchaseMethod(string purchaseMethod)
    {
        return purchaseMethod switch
        {
            "SEER_BEAN" => 1,
            "GOLD_BEAN" => 2,
            _ => -1
        };
    }

    /// <summary>
    /// 获取货币名称
    /// </summary>
    private string GetCurrencyName(int currencyId)
    {
        return currencyId switch
        {
            1 => "赛尔豆",
            2 => "金豆",
            _ => "货币"
        };
    }

    /// <summary>
    /// 显示货币不足提示
    /// </summary>
    private void ShowInsufficientCurrencyTip(int currencyId)
    {
        Dialog1Manager.Instance.ShowPopupOnTransform(
            _uiCanvasCache.transform,
            "提示",
            $"您的{GetCurrencyName(currencyId)}不足！");
    }


    public ItemConfig GetItemData()
    {
        return itemData;
    }

    private void OnDestroy()
    {
        if (BuyButton != null)
        {
            BuyButton.onClick.RemoveListener(() => OnBuyButtonClicked(null));
        }
    }
}
