using UnityEngine;
using TMPro;
using UnityEngine.UI;
using SeerGoGame.Utilities;

public class Dialog1GetPet : Dialog1Base
{
    [Header("UI组件")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Image petImg;
    [SerializeField] private Image iconImg;

    /// <summary>
    /// 初始化弹窗内容
    /// </summary>
    public void Initialize(string message, PetData petData, bool isBagFull)
    {
        if (messageText != null)
            messageText.text = message ?? "";

        string resPath = $"Sprites/PetImage/Pet_{petData.Id}/1";
        Sprite sprite = ResourcesManager.Instance.Load<Sprite>(resPath);
        petImg.sprite = sprite;

        string iconPath = isBagFull ? "Sprites/PetBag/intoStroage" : "Sprites/Maps/UI/2669";
        Sprite iconSprite = ResourcesManager.Instance.Load<Sprite>(iconPath);
        iconImg.sprite = iconSprite;

        isClosing = false;
    }
}