using SeerGoGame.Controller;
using UnityEngine;
using UnityEngine.UI;

public class PetStroagePetItemCell : BaseController
{
    [Header("UI References")]
    [SerializeField] private Image ItemIcon;

    public void Initialize(int id)
    {
        Sprite sprite = ResourcesManager.Instance.Load<Sprite>($"Sprites/PetImage/Pet_{id}/1");

        if (ItemIcon != null)
            ItemIcon.sprite = sprite;
    }

}