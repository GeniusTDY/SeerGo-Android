using PixeLadder.SimpleTooltip;
using QFramework;
using SeerGoGame.Controller;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogSkillItem : BaseController
{
    [Header("技能信息")]
    public TextMeshProUGUI skillPPInfo;
    public TextMeshProUGUI skillPowerInfo;
    public TextMeshProUGUI skillName;

    public Image skillTypeImg;
    public Image bg;

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

    public void SetDialogSkillItem(SkillData skillData, GameObject skillBg, Canvas uiCanvasCache)
    {
        skillPPInfo.text = $"{skillData.MaxPP.ToString()}/{ skillData.MaxPP.ToString()}";
        skillPowerInfo.text = skillData.Power.ToString();
        skillName.text = skillData.Name;
        string typeSpriteName = (skillData.Category == 1 || skillData.Category == 2) ?
        skillData.Type.ToString() : "0";
        skillTypeImg.sprite = Resources.Load<Sprite>(TYPE_SPRITE_PATH+ typeSpriteName);

        // 添加提示
        // 添加类型图标提示（支持单/双属性文本）
        int typeId = int.Parse(typeSpriteName);
        var (icon, icon2) = SpriteUtility.LoadAttributeTextSprites(attributeSystem, typeId, LoadSprite);
        TooltipTrigger.AddTooltip(skillTypeImg.gameObject, null, null,
            Color.yellow, icon, uiCanvasCache, icon2);
        // 设置技能背景提示
        SetSkillTooltip(skillData, skillBg, uiCanvasCache);
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

    public void SetDialogSkillBg(Sprite sprite)
    {
        bg.sprite = sprite;
    }
    private Sprite LoadSprite(string path)
    {
        return Resources.Load<Sprite>(path);
    }
}
