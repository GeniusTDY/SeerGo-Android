using SeerGoGame.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSwitchPetSkillItem : MonoBehaviour
{
    [Header("UI组件引用")]
    [SerializeField] private TextMeshProUGUI skillNameText;  // SkillName字段
    [SerializeField] private Image typeImage;        // Type字段（图片）
    [SerializeField] private TextMeshProUGUI countsText;    // 使用次数
    private const string TYPE_SPRITE_PATH = "Sprites/Pets/PetType/";
    
    public void SetSkillData(SkillData skillData, PetData petData)
    {
        skillNameText.text = skillData.Name;
        for (int i = 0; i < petData.Skills.NormalSkill.Count; i++)
        {
            if (petData.Skills.NormalSkill[i].id.ToString() == skillData.ID)
            {
                countsText.text = $"使用次数：{petData.Skills.NormalSkill[i].currentPP}/{skillData.MaxPP}";
                break;
            }
        }
        string typeSpriteName = (skillData.Category == 1 || skillData.Category == 2) ?
        skillData.Type.ToString() : "0";
        typeImage.sprite = ResourcesManager.Instance.Load<Sprite>(TYPE_SPRITE_PATH + typeSpriteName);
    }
}
