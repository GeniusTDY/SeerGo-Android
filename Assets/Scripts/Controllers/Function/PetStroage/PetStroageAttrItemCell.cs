using SeerGoGame.Controller;
using SeerGoGame.Models;
using UnityEngine;
using UnityEngine.UI;

public class PetStroageAttrItemCell : BaseController
{
    [Header("UI References")]
    [SerializeField] private Image ItemIcon;

    private AttributeData itemData;
    private const string BasePath = "Sprites/Pets/PetType/";

    public void Initialize(AttributeData data)
    {
        itemData = data;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (itemData == null) return;

        Sprite sprite = ResourcesManager.Instance.Load<Sprite>(BasePath + itemData.id);

        if (ItemIcon != null)
            ItemIcon.sprite = sprite;

    }
}