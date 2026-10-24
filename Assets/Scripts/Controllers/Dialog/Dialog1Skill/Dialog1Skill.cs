using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SeerGoGame.Models;

public class Dialog1Skill : Dialog1Base
{
    [Header("UI组件")]
    [SerializeField] private TextMeshProUGUI petName;
    [SerializeField] private Image petImg;
    [SerializeField] private TextMeshProUGUI skillName;
    [SerializeField] private TextMeshProUGUI power;
    [SerializeField] private TextMeshProUGUI pp;
    [SerializeField] private Image skillAttr;

    private const string TYPE_SPRITE_PATH = "Sprites/Pets/PetType/";

    /// <summary>
    /// 初始化弹窗内容
    /// </summary>
    public void Initialize(SkillData skillData, PetData petData)
    {
        petName.text = petData.Basic.Name;
        skillName.text = skillData.Name;
        power.text = "威力:" + skillData.Power;
        pp.text = $"PP:{skillData.MaxPP}/{skillData.MaxPP}";

        string petPath = $"Sprites/PetImage/Pet_{petData.Id}/1";
        petImg.sprite = ResourcesManager.Instance.Load<Sprite>(petPath);

        string typeSpriteName = (skillData.Category == 1 || skillData.Category == 2) ?
            skillData.Type.ToString() : "0";
        skillAttr.sprite = ResourcesManager.Instance.Load<Sprite>(TYPE_SPRITE_PATH + typeSpriteName);

        isClosing = false;
    }
}