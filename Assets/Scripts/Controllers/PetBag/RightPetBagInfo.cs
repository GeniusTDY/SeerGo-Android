using PixeLadder.SimpleTooltip;
using QFramework;
using SeerGoGame.Controller;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RightPetBagInfo : BaseController
{
    [Header("右半背包信息")]
    [Header("基础信息")]
    public Image petImg, petTypeImg, petGenderImg;
    public TextMeshProUGUI petId, petName, petLevel, petGeTiZhi, petExp, peCharacter, peGetTime, PetFeature, PetFeatureInfo;

    [Header("学习力")]
    public TextMeshProUGUI Atk, AtkLearn, Def, DefLearn, SpAtk, SpAtkLearn,
                          SpDef, SpDefLearn, Spd, SpdLearn, Hp, HpLearn;

    [Header("技能父容器")] public GameObject[] skillBgs;
    [Header("技能预制体")] public GameObject petSkillItem;

    private ISkillSystem skillSystem;
    private IPersonalitySystem personalitySystem;
    private IAttributeSystem attributeSystem;

    // 常量定义
    private const string PET_IMAGE_PATH = "Sprites/PetImage/Pet_{0}/1";
    private const string GENDER_PATH = "Sprites/Pets/Gender/{0}";
    private const string TYPE_PATH = "Sprites/Pets/PetType/{0}";
    private const string TYPE_TEXT_PATH = "Sprites/Pets/PetTypeText/{0}";

    private readonly string[] genderDescriptions = { "无性别", "雄", "雌" };

    void Start()
    {
        skillSystem = this.GetSystem<ISkillSystem>();
        personalitySystem = this.GetSystem<IPersonalitySystem>();
        attributeSystem = this.GetSystem<IAttributeSystem>();
    }

    public void SetRightBagInfo(PetData petData, int index, Canvas uiCanvasCache)
    {
        // 设置宠物图片
        SetPetImage(petData);

        // 设置基础信息
        SetBasicInfo(petData, uiCanvasCache);

        // 设置属性信息
        SetAttributeInfo(petData, uiCanvasCache);

        // 设置种族值和学习力
        SetStatusValues(petData);

        // 设置技能
        SetSkills(petData, uiCanvasCache);
    }

    private void SetPetImage(PetData petData)
    {
        petImg.sprite = LoadSprite(string.Format(PET_IMAGE_PATH, petData.Id));
    }

    private void SetBasicInfo(PetData petData, Canvas uiCanvasCache)
    {
        petId.text = petData.Id.ToString();
        petName.text = petData.Basic.Name;
        petLevel.text = petData.Exp.Level.ToString();
        petGeTiZhi.text = petData.Talent.IV.ToString();
        petExp.text = petData.Exp.CurrentExp.ToString();
        peGetTime.text = petData.Basic.GetPetDate;

        // 性格信息
        PersonalityData personalityData = personalitySystem.GetPersonalityById(petData.Basic.Personality);
        peCharacter.text = personalityData.name;
        TooltipTrigger.AddTooltip(peCharacter.gameObject, null, personalityData.desc, Color.yellow, null, uiCanvasCache);

        // 特性信息
        SetFeatureInfo(petData, uiCanvasCache);

        // 性别信息
        SetGenderInfo(petData, uiCanvasCache);
    }

    /// <summary>
    /// 设置特性信息
    /// </summary>
    private void SetFeatureInfo(PetData petData, Canvas uiCanvasCache)
    {
        int featureId = petData.Feature.FeatureId;
        
        if (featureId > 0)
        {
            // 显示特性
            PetFeature.gameObject.SetActive(true);
            PetFeatureInfo.gameObject.SetActive(true);

            // 根据FeatureId查询new_se配置获取特性描述
            var seConfig = NewSeManager.Instance.GetConfigById(featureId);
            string desc = seConfig != null ? seConfig.Desc : "未知特性";

            // 设置特性名称文本
            PetFeatureInfo.text = desc;

            // 添加特性提示
            string fullDesc = seConfig != null ? $"{seConfig.Desc}\n{seConfig.Intro}" : "未知特性";
            TooltipTrigger.AddTooltip(PetFeatureInfo.gameObject, null, fullDesc, Color.yellow, null, uiCanvasCache);
        }
        else
        {
            // 隐藏特性
            PetFeature.gameObject.SetActive(false);
            PetFeatureInfo.gameObject.SetActive(false);
        }
    }

    private void SetGenderInfo(PetData petData, Canvas uiCanvasCache)
    {
        petGenderImg.sprite = LoadSprite(string.Format(GENDER_PATH, petData.Basic.Gender));
        string genderDesc = genderDescriptions[petData.Basic.Gender < genderDescriptions.Length ? petData.Basic.Gender : 2];
        TooltipTrigger.AddTooltip(petGenderImg.gameObject, null, genderDesc, Color.yellow, null, uiCanvasCache);
    }

    private void SetAttributeInfo(PetData petData, Canvas uiCanvasCache)
    {
        petTypeImg.sprite = LoadSprite(string.Format(TYPE_PATH, petData.Basic.Attributes));
        // 使用SpriteUtility自动处理单/双属性文本图片加载
        var (icon, icon2) = SpriteUtility.LoadAttributeTextSprites(attributeSystem, petData.Basic.Attributes, LoadSprite);
        TooltipTrigger.AddTooltip(petTypeImg.gameObject, null, null, Color.yellow, icon, uiCanvasCache, icon2);
    }

    private void SetStatusValues(PetData petData)
    {
        // 种族值赋值
        var status = petData.CurrentStatus;
        Hp.text = status.MaxHp.ToString();
        Atk.text = status.Atk.ToString();
        Def.text = status.Def.ToString();
        SpAtk.text = status.SpAtk.ToString();
        SpDef.text = status.SpDef.ToString();
        Spd.text = status.Spd.ToString();

        // 学习力赋值
        var ev = petData.Talent.EV;
        HpLearn.text = ev.Hp.ToString();
        AtkLearn.text = ev.Atk.ToString();
        DefLearn.text = ev.Def.ToString();
        SpAtkLearn.text = ev.SpAtk.ToString();
        SpDefLearn.text = ev.SpDef.ToString();
        SpdLearn.text = ev.Spd.ToString();
    }

    private void SetSkills(PetData petData, Canvas uiCanvasCache)
    {
        var normalSkillId = petData.Skills.NormalSkill;
        int skillCount = Mathf.Min(normalSkillId.Count, skillBgs.Length);

        for (int i = 0; i < skillCount; i++)
        {
            var skillData = skillSystem.GetSkillById(normalSkillId[i].id.ToString());
            var itemObj = Instantiate(petSkillItem, skillBgs[i].transform);
            var itemUI = itemObj.GetComponent<PetSkillItem>();
            itemUI.SetSkillItem(skillData, uiCanvasCache, skillBgs[i], petData);
        }
    }

    private Sprite LoadSprite(string path)
    {
        // 使用 ResourcesManager（自动路由外部资源到 ExternalResourcesManager）
        return ResourcesManager.Instance.Load<Sprite>(path);
    }
}