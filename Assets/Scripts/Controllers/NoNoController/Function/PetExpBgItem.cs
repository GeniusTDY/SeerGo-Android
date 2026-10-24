using SeerGoGame.Utilities;
using UnityEngine;
using UnityEngine.UI;

public class PetExpBgItem : MonoBehaviour
{
    public Image petGridImg;
    public Image petGridBg;
   
    public void SetPetExpBgItem(PetData petData)
    {
        string resPath = $"Sprites/PetImage/Pet_{petData.Id}/1";
        Sprite sprite = ResourcesManager.Instance.Load<Sprite>(resPath);
        petGridImg.sprite = sprite;
        
        string bgPath = "Sprites/NoNo/Exp/bg";
        Sprite bgSprite = ResourcesManager.Instance.Load<Sprite>(bgPath);
        petGridBg.sprite = bgSprite;

    }

}
