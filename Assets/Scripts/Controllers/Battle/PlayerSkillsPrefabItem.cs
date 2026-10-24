using QFramework;
using PixeLadder.SimpleTooltip;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSkillsPrefabItem : BaseBattleController
{
    [Header("UI组件引用")]
    [SerializeField] private TextMeshProUGUI skillNameText;  // SkillName字段
    [SerializeField] private Image typeImage;        // Type字段（图片）
    [SerializeField] private TextMeshProUGUI countsText;    // 使用次数

    private ISkillSystem skillSystem;
    private IAttributeSystem attributeSystem;
    // 缓存常用路径和类型映射
    private const string TYPE_SPRITE_PATH = "Sprites/Pets/PetType/";
    private const string TYPE_TEXT_PATH = "Sprites/Pets/PetTypeText/";
    private readonly string[] categoryNames = { "属性攻击", "物理攻击", "特殊攻击" };

    private void Awake()
    {
        skillSystem = this.GetSystem<ISkillSystem>();
        attributeSystem = this.GetSystem<IAttributeSystem>();
    }

    public void SetSkillData(SkillData skillData, PetData petData, Canvas uiCanvasCache)
    {
        skillNameText.text = skillData.Name;
        // 遍历技能id下标
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

        // 添加类型图标提示（支持单/双属性文本）
        int typeId = int.Parse(typeSpriteName);
        var (icon, icon2) = SpriteUtility.LoadAttributeTextSprites(attributeSystem, typeId,
            path => ResourcesManager.Instance.Load<Sprite>(path));
        TooltipTrigger.AddTooltip(typeImage.gameObject, null, null,
            Color.yellow, icon, uiCanvasCache, icon2);

        // 设置技能背景提示
        SetSkillTooltip(skillData, gameObject, uiCanvasCache);
    }

    private void SetSkillTooltip(SkillData skillData, GameObject skillBg, Canvas uiCanvasCache)
    {
        string typeName = categoryNames[skillData.Category == 4 ? 0 : skillData.Category];
        var effectInfo = skillSystem.GetSkillEffectInfoById(skillData.ID);

        // 使用StringBuilder优化字符串拼接
        System.Text.StringBuilder descBuilder = new System.Text.StringBuilder();

        if (skillData.Power > 0)
        {
            descBuilder.Append($"威力：{skillData.Power}\n命中率：{skillData.Accuracy}%\n\n");
        }
        else
        {
            descBuilder.Append($"命中率：{skillData.Accuracy}%\n\n");
        }
        if (effectInfo != null)
            descBuilder.Append(effectInfo.help.Replace("；", "；\n"));

        TooltipTrigger.AddTooltip(skillBg, descBuilder.ToString(),
            $"{skillData.Name}   ({typeName})", Color.yellow, null, uiCanvasCache);
    }
}
