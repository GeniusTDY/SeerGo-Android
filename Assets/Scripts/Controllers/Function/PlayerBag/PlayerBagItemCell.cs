using SeerGoGame.Controller;
using SeerGoGame.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBagItemCell : BaseController
{
    [Header("UI References")]
    [SerializeField] private Image ItemIcon;
    [SerializeField] private TextMeshProUGUI ItemNum;

    private ItemData itemData;

    public void Initialize(ItemData data, IArchiveModel _archiveModel, Sprite sprite)
    {
        itemData = data;
        UpdateUI(sprite);
    }

    // 重置方法，用于对象池回收时调用
    public void ResetCell()
    {
        itemData = null;
        if (ItemIcon != null) ItemIcon.sprite = null;
        if (ItemNum != null) ItemNum.text = "";
    }

    private void UpdateUI(Sprite sprite)
    {
        if (itemData == null) return;

        if (ItemIcon != null)
            ItemIcon.sprite = sprite;

        if (ItemNum != null)
            ItemNum.text = itemData.Num.ToString();
    }
}