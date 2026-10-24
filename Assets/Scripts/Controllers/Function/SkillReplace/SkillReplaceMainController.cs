using QFramework;
using SeerGoGame.Commands;
using SeerGoGame.Controller;
using SeerGoGame.Events;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

public class SkillReplaceMainController : BaseController
{
    [Header("左半UI")]
    [SerializeField] private GameObject PetSkillPanel; // 左侧已装备技能面板
    [SerializeField] private Button ConfirmButton; // 确认按钮
    [SerializeField] private Button CloseButton; // 关闭按钮
    [SerializeField] private Button CancelButton; // 取消按钮
    [SerializeField] private Image petIcon; // 宠物头像
    [SerializeField] private TextMeshProUGUI petId, petName, petLevel, petPersonality, petAtk, petSpAtk, petDef, petSpDef, petSpd, petHp; // 宠物信息文本

    [Header("右半UI")]
    [SerializeField] private GameObject ReplaceSkillPanel; // 右侧可替换技能面板
    [SerializeField] private Button PreButton, NextButton; // 分页按钮
    [SerializeField] private TextMeshProUGUI PageNum; // 分页信息文本

    [Header("颜色设置")]
    [SerializeField] private Color normalColor = Color.white; // 默认颜色
    [SerializeField] private Color selectedColor = Color.yellow; // 选中颜色

    [Header("预制体")]
    [SerializeField] private GameObject ItemPrefab; // 技能项预制体

    // 系统接口
    private ISkillSystem skillSystem;
    private IPersonalitySystem personalitySystem;
    private IArchiveModel archiveModel;
    private IArchiveSystem archiveSystem;

    // 当前选中的宠物索引
    private int currentPetIndex = -1;
    // 左侧选中的技能索引
    private int leftSelectedSkillIndex = -1;
    // 右侧选中的技能数据
    private SkillData rightSelectedSkillData;
    // 左侧技能项Image组件列表
    private List<Image> skillItemImages = new List<Image>();
    // 右侧技能项Image组件列表
    private List<Image> replaceSkillItemImages = new List<Image>();
    // 可替换技能列表
    private List<SkillData> replaceSkillList = new List<SkillData>();

    // 分页设置
    private const int PageSize = 4;
    private int CurrentPage = 1;
    private int TotalPages = 1;

    void Start()
    {
        // 注册事件
        this.RegisterEvent<SkillReplaceMainOpenEvent>(OnSkillReplaceMainOpen)
          .UnRegisterWhenGameObjectDestroyed(gameObject);

        // 获取系统
        skillSystem = this.GetSystem<ISkillSystem>();
        personalitySystem = this.GetSystem<IPersonalitySystem>();
        archiveModel = this.GetModel<IArchiveModel>();
        archiveSystem = this.GetSystem<IArchiveSystem>();

        // 绑定按钮事件
        CancelButton.onClick.AddListener(() => ClosePanel());
        CloseButton.onClick.AddListener(() => ClosePanel());
        PreButton.onClick.AddListener(OnPrePage);
        NextButton.onClick.AddListener(OnNextPage);
        ConfirmButton.onClick.AddListener(OnConfirmButtonClick);
    }

    /// <summary>
    /// 确认按钮点击事件
    /// </summary>
    private void OnConfirmButtonClick()
    {
        // 检查是否选择了左右两边的技能
        if (leftSelectedSkillIndex == -1 || rightSelectedSkillData == null)
        {
            Dialog1Manager.Instance.ShowPopupOnTransform(FunctionPanelLoader.Instance.transform, "", "点击选择两边的技能才能进行技能唤醒。");
            return;
        }
        if (currentPetIndex == -1) return;

        // 获取当前宠物数据
        PetData petData = archiveModel.CurrentData.PetBag[currentPetIndex];
        // 创建新的已装备技能
        EquippedSkill equippedSkill = new EquippedSkill(
            rightSelectedSkillData.ID.ToInt(),
            rightSelectedSkillData.MaxPP,
            rightSelectedSkillData.MaxPP);
        // 替换技能
        petData.Skills.NormalSkill[leftSelectedSkillIndex] = equippedSkill;
        // 保存数据
        archiveSystem.SaveGame("autosave0", archiveModel.CurrentData);

        // 显示成功提示
        Dialog1Manager.Instance.ShowPopupOnTransform(
            FunctionPanelLoader.Instance.transform,
            "",
            $"你花费了100赛尔豆使<color=red>{petData.Basic.Name}</color>技能唤醒成功。",
            () =>
            {
                // 关闭面板并刷新背包
                ClosePanel();
                this.SendCommand<ChangePetBagCommand>();
            }
        );
    }

    /// <summary>
    /// 技能替换面板打开事件
    /// </summary>
    /// <param name="e">事件参数</param>
    private void OnSkillReplaceMainOpen(SkillReplaceMainOpenEvent e)
    {
        InitData(e.petData, e.petIndex);
    }

    /// <summary>
    /// 初始化面板数据
    /// </summary>
    /// <param name="data">宠物数据</param>
    /// <param name="petIndex">宠物索引</param>
    public void InitData(PetData data, int petIndex = -1)
    {
        // 清空所有项
        ClearAllItems();
        currentPetIndex = petIndex;

        // 更新宠物信息
        UpdatePetInfo(data);
        // 创建已装备技能项
        CreateEquippedSkills(data);
        // 创建可替换技能列表
        CreateReplaceSkills(data);
        // 加载第一页
        LoadCurrentPage();
    }

    /// <summary>
    /// 更新宠物信息
    /// </summary>
    /// <param name="data">宠物数据</param>
    private void UpdatePetInfo(PetData data)
    {
        // 设置宠物基本信息文本
        petId.text = "序号:" + data.Id.ToString();
        petName.text = "名字:" + data.Basic.Name;
        petLevel.text = "等级:" + data.Exp.Level.ToString();
        petPersonality.text = "性格:" + personalitySystem.GetPersonalityById(data.Basic.Personality).name;
        petAtk.text = "攻击:" + data.CurrentStatus.Atk.ToString();
        petSpAtk.text = "特攻:" + data.CurrentStatus.SpAtk.ToString();
        petDef.text = "防御:" + data.CurrentStatus.Def.ToString();
        petSpDef.text = "特防:" + data.CurrentStatus.SpDef.ToString();
        petSpd.text = "速度:" + data.CurrentStatus.Spd.ToString();
        petHp.text = "体力:" + data.CurrentStatus.Hp.ToString();
        // 加载宠物头像
        petIcon.sprite = ResourcesManager.Instance.Load<Sprite>($"Sprites/PetImage/Pet_{data.Id}/1");
    }

    /// <summary>
    /// 创建已装备技能项
    /// </summary>
    /// <param name="data">宠物数据</param>
    private void CreateEquippedSkills(PetData data)
    {
        // 遍历已装备技能
        for (int i = 0; i < data.Skills.NormalSkill.Count; i++)
        {
            var skill = data.Skills.NormalSkill[i];
            var skillData = skillSystem.GetSkillById(skill.id.ToString());
            // 创建技能项
            CreateSkillItem(skillData, i, PetSkillPanel.transform, skillItemImages, OnSkillSelected);
        }
    }

    /// <summary>
    /// 创建可替换技能列表
    /// </summary>
    /// <param name="data">宠物数据</param>
    private void CreateReplaceSkills(PetData data)
    {
        // 清空列表
        replaceSkillList.Clear();
        // 获取未装备的技能ID
        List<int> ownSkillId = data.Skills.OwnSkillId
            .Where(id => !data.Skills.NormalSkill.Any(skill => skill.id == id))
            .ToList();

        // 将技能ID转换为技能数据
        foreach (var id in ownSkillId)
        {
            var skill = skillSystem.GetSkillById(id.ToString());
            if (skill != null) replaceSkillList.Add(skill);
        }

        // 计算总页数
        TotalPages = Mathf.Max(1, Mathf.CeilToInt(replaceSkillList.Count / (float)PageSize));
        CurrentPage = 1;
        UpdatePageInfo();
    }

    /// <summary>
    /// 创建技能项
    /// </summary>
    /// <param name="skillData">技能数据</param>
    /// <param name="index">索引</param>
    /// <param name="parent">父节点</param>
    /// <param name="imageList">Image组件列表</param>
    /// <param name="onClick">点击回调</param>
    private void CreateSkillItem(SkillData skillData, int index, Transform parent, List<Image> imageList, Action<SkillData, int> onClick)
    {
        // 实例化预制体
        GameObject itemObject = Instantiate(ItemPrefab, parent);
        // 获取或添加Image组件
        Image itemImage = itemObject.GetComponent<Image>() ?? itemObject.AddComponent<Image>();
        imageList.Add(itemImage);
        itemImage.color = normalColor;

        // 设置技能UI
        DialogSkillItem skillUI = itemObject.GetComponent<DialogSkillItem>();
        if (skillUI != null)
        {
            skillUI.SetDialogSkillItem(skillData, itemObject, FunctionPanelLoader.Instance.transform.GetComponent<Canvas>());
        }

        // 添加点击事件
        Button itemButton = itemObject.GetComponent<Button>() ?? itemObject.AddComponent<Button>();
        itemButton.onClick.RemoveAllListeners();
        itemButton.onClick.AddListener(() => onClick?.Invoke(skillData, index));
    }

    /// <summary>
    /// 左侧技能项选中事件
    /// </summary>
    /// <param name="skillData">技能数据</param>
    /// <param name="index">索引</param>
    private void OnSkillSelected(SkillData skillData, int index)
    {
        // 重置所有左侧技能项颜色
        ResetColors(skillItemImages);
        // 设置选中项颜色
        if (index >= 0 && index < skillItemImages.Count) skillItemImages[index].color = selectedColor;
        leftSelectedSkillIndex = index;
    }

    /// <summary>
    /// 右侧技能项选中事件
    /// </summary>
    /// <param name="skillData">技能数据</param>
    /// <param name="index">索引</param>
    private void OnReplaceSkillSelected(SkillData skillData, int index)
    {
        // 重置所有右侧技能项颜色
        ResetColors(replaceSkillItemImages);
        // 设置选中项颜色
        if (index >= 0 && index < replaceSkillItemImages.Count) replaceSkillItemImages[index].color = selectedColor;
        rightSelectedSkillData = skillData;
    }

    /// <summary>
    /// 加载当前页的可替换技能
    /// </summary>
    private void LoadCurrentPage()
    {
        // 清空右侧面板
        ClearTransform(ReplaceSkillPanel.transform);
        replaceSkillItemImages.Clear();

        // 计算起始和结束索引
        int startIndex = (CurrentPage - 1) * PageSize;
        int endIndex = Mathf.Min(startIndex + PageSize, replaceSkillList.Count);

        // 创建当前页的技能项
        for (int i = startIndex; i < endIndex; i++)
        {
            CreateSkillItem(replaceSkillList[i], i - startIndex, ReplaceSkillPanel.transform, replaceSkillItemImages, OnReplaceSkillSelected);
        }
    }

    /// <summary>
    /// 更新分页信息
    /// </summary>
    private void UpdatePageInfo()
    {
        PageNum.text = $"{CurrentPage}/{TotalPages}";
        PreButton.interactable = CurrentPage > 1;
        NextButton.interactable = CurrentPage < TotalPages;
    }

    /// <summary>
    /// 上一页按钮点击事件
    /// </summary>
    private void OnPrePage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            UpdatePageInfo();
            LoadCurrentPage();
        }
    }

    /// <summary>
    /// 下一页按钮点击事件
    /// </summary>
    private void OnNextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            UpdatePageInfo();
            LoadCurrentPage();
        }
    }

    /// <summary>
    /// 重置指定Image列表的颜色
    /// </summary>
    /// <param name="images">Image列表</param>
    private void ResetColors(List<Image> images)
    {
        foreach (var img in images)
        {
            if (img != null) img.color = normalColor;
        }
    }

    /// <summary>
    /// 清空指定Transform的所有子对象
    /// </summary>
    /// <param name="t">Transform</param>
    private void ClearTransform(Transform t)
    {
        foreach (Transform child in t) Destroy(child.gameObject);
    }

    /// <summary>
    /// 清空所有技能项
    /// </summary>
    private void ClearAllItems()
    {
        ClearTransform(PetSkillPanel.transform);
        ClearTransform(ReplaceSkillPanel.transform);

        // 清空列表
        skillItemImages.Clear();
        replaceSkillItemImages.Clear();

        // 重置选中状态
        leftSelectedSkillIndex = -1;
        rightSelectedSkillData = null;
        currentPetIndex = -1;
    }

    /// <summary>
    /// 关闭面板
    /// </summary>
    private void ClosePanel()
    {
        FunctionPanelLoader.Instance.ClosePanel("SkillRelace/SkillReplaceMainPanel");
    }
}