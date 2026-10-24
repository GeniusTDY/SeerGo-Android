using PixeLadder.SimpleTooltip;
using SeerGoGame.Models;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using QFramework;

public class ItemCell : BaseBattleController, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI Num;

    private ItemData itemData;
    private System.Action<ItemData> onClickCallback;

    private IItemModel itemModel;

    private const string BasePath = "Sprites/Item/";

    private void Awake()
    {
        itemModel = this.GetModel<IItemModel>();
    }


    public void Initialize(ItemData data, System.Action<ItemData> onClick, Canvas uiCanvasCache)
    {
        itemData = data;
        onClickCallback = onClick;

        UpdateUI(uiCanvasCache);
    }

    private void UpdateUI(Canvas uiCanvasCache)
    {
        Sprite sprite = ResourcesManager.Instance.Load<Sprite>(BasePath + itemData.Id.ToString());
        if (iconImage != null)
            iconImage.sprite = sprite;

        if (Num != null)
            Num.text = itemData.Num.ToString();

        ItemConfig itemConfig = itemModel.GetById(itemData.Id);

        TooltipTrigger.AddTooltip(gameObject, itemConfig.description, itemConfig.name,
         Color.yellow, sprite, uiCanvasCache);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClickCallback?.Invoke(itemData);
    }

    public ItemData GetItemData()
    {
        return itemData;
    }
}