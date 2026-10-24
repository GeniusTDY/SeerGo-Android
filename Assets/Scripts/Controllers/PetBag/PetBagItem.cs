using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PetBagItem : MonoBehaviour
{
    [Header("×ó°ë±³°üItem")]
    public Image petImg;
    public Image petBgImg;
    public Image petHpImg;
    public Button petBgButton;
    public TextMeshProUGUI petName;
    public TextMeshProUGUI petLevel;
    public TextMeshProUGUI petHpText;

    public void SetPetBagItem(PetData petData)
    {
        string resPath ="Sprites/PetBag/1";
        Sprite sprite = ResourcesManager.Instance.Load<Sprite>(resPath);
        petBgImg.sprite = sprite;

        Sprite petImgSprite = ResourcesManager.Instance.Load<Sprite>($"Sprites/PetImage/Pet_{petData.Id}/1");
        petImg.sprite = petImgSprite;

        petName.text = petData.Basic.Name;
        petLevel.text = "Lv:"+petData.Exp.Level;
        petHpText.text = $"{petData.CurrentStatus.Hp}/{ petData.CurrentStatus.MaxHp}";
        petHpImg.fillAmount = (float)petData.CurrentStatus.Hp/ (float)petData.CurrentStatus.MaxHp;

    }
}
