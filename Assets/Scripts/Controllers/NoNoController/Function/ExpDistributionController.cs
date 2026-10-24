using QFramework;
using SeerGoGame.Commands;
using SeerGoGame.Controller;
using SeerGoGame.Events;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

public class ExpDistributionController : BaseController
{
    [Header("经验分配器按钮")]
    public Button close; //关闭
    public Button confirm; //确认

    [Header("left")]
    public TextMeshProUGUI leftPetName;
    public Image leftPetImg;

    [Header("center")]
    public RectTransform petExpPanel;

    [Header("bottom")]
    public TextMeshProUGUI inputField;

    [Header("right")]
    public TextMeshProUGUI rightId;
    public TextMeshProUGUI rightName;
    public TextMeshProUGUI rightLevel;
    public TextMeshProUGUI rightNature;
    public TextMeshProUGUI rightExpInfo;
    public TextMeshProUGUI rightTotalExp;

    [Header("预制体")]
    public GameObject item;
    public Transform canvasTransform;

    private IArchiveModel _archiveModel;
    private IPersonalitySystem _personalitySystem;
    private IUISystem _UISystem;
    private IExpConfigSystem _expConfigSystem;
    private IPetAttributeCalcSystem _petAttributeCalcSystem;

    private List<PetExpBgItem> expBagList;
    private int totalExp;
    private int _currentSelectedIndex; // 当前选择的精灵下标

    private const string SELECT_BG_PATH = "Sprites/NoNo/Exp/selectBg";
    private const string NORMAL_BG_PATH = "Sprites/NoNo/Exp/bg";

    void Start()
    {
        // 初始化系统
        _archiveModel = this.GetModel<IArchiveModel>();
        _UISystem = this.GetSystem<IUISystem>();
        _personalitySystem = this.GetSystem<IPersonalitySystem>();
        _expConfigSystem = this.GetSystem<IExpConfigSystem>();
        _petAttributeCalcSystem = this.GetSystem<IPetAttributeCalcSystem>();

        // 初始化列表
        expBagList = new List<PetExpBgItem>();

        // 添加按钮监听
        confirm.onClick.AddListener(() => ConfirmButtonClick());
        close.onClick.AddListener(() => CloseButtonClick());

        this.RegisterEvent<PetExpOpenEvent>(OnPetExpOpenEvent)
           .UnRegisterWhenGameObjectDestroyed(gameObject);
    }

    private void OnPetExpOpenEvent(PetExpOpenEvent @event)
    {
        LoadResources();
    }

    // 关闭按钮点击事件
    private void CloseButtonClick()
    {
        _UISystem.SetUICanvasGroup("NoNoFuncCanvas",false);
    }

    // 确认按钮点击事件
    private void ConfirmButtonClick()
    {
        // 查询当前精灵信息
        List<PetData> petBag = _archiveModel.CurrentData.PetBag;

        // 弹窗提示是否分配经验

        // 获取当前精灵信息
        PetData petData = petBag[_currentSelectedIndex];
        _expConfigSystem.InitializeData();

        int exp = SafeStringToInt(inputField.text);
        if (exp <= 0) return;
        if (petData.Exp.Level == 100) return;

        if(_archiveModel.CurrentData.PetTotalExp < exp)
        {
            Dialog1Manager.Instance.ShowPopupImmediatelyOnTransform(
            canvasTransform,
            "提示",
            "可分配经验不足！！！"
        );
            return;
        }

        // 弹窗提示确认分配
        Dialog2Manager.Instance.ShowPopupImmediatelyOnTransform(
            canvasTransform,
            petData.Basic.Name,
            "你确定要这样分配经验吗?",
            onConfirm: () => ExpDistributionInfo(petData, exp) // 确认，继续弹窗
        );
    }

    // 经验分配信息显示
    private void ExpDistributionInfo(PetData petData, int expGained)
    {
        string finalText = string.Format("你的<color=#FF0000>{0}</color>获得了<color=#0066FF>{1}</color>点经验，更喜欢你了呢！",
            petData.Basic.Name, expGained);

        Dialog1Manager.Instance.ShowPopupOnTransform(
            canvasTransform,
            null,
            finalText,
            onConfirm: () =>
            {
                // 记录初始等级
                int originLevel = petData.Exp.Level;

                // 升级经验计算
                this.GetSystem<IExpConfigSystem>().CalcExp(petData, expGained, canvasTransform);
                // 种族值计算 
                List<int> addNums = _petAttributeCalcSystem.CalculateAllStats(petData);
                int newLevel = petData.Exp.Level - originLevel;
                if (newLevel > 0)
                {
                    // 种族值弹窗
                    addNums.Add(petData.Exp.Level - originLevel);
                    Dialog1LevelUpManager.Instance.ShowPopupImmediatelyOnTransform(
                          canvasTransform,
                          addNums,
                          petData,
                          expGained
                        );
                }
                // 扣除经验
                _archiveModel.CurrentData.PetTotalExp -= expGained;

                this.SendCommand(new SaveArchiveDataCommand(_archiveModel.CurrentData));
                LoadResources();
            } // 计算升级
        );
    }


    // 安全字符串转整数
    public static int SafeStringToInt(string input, int defaultValue = 0)
    {
        if (string.IsNullOrEmpty(input))
            return defaultValue;

        // 1. 去除首尾空白
        string cleanText = input.Trim();

        // 2. 安全转换
        if (int.TryParse(cleanText, out int result))
        {
            return result;
        }

        // 3. 如果仍然失败，尝试更彻底的清理
        cleanText = System.Text.RegularExpressions.Regex.Replace(cleanText, @"[^\d-]", "");
        if (int.TryParse(cleanText, out result))
        {
            return result;
        }

        SeerGoLogger.LogWarning($"无法将字符串 '{input}' 转换为整数");
        return defaultValue;
    }

    // 加载资源
    private void LoadResources()
    {
        // 清空现有项
        foreach (Transform child in petExpPanel)
        {
            Destroy(child.gameObject);
        }
        expBagList.Clear();

        totalExp = _archiveModel.CurrentData.PetTotalExp;

        // 创建宠物经验背包项
        List<PetData> petBag = _archiveModel.CurrentData.PetBag;
        for (int i = 0; i < petBag.Count; i++)
        {
            CreatePetExpBagItem(petBag[i], i);
        }

        // 默认选择第一个，并且展示左右信息
        if (petBag.Count > 0)
        {
            SelectPet(petBag[0], 0);
        }
    }

    // 创建宠物经验背包项
    private void CreatePetExpBagItem(PetData petData, int index)
    {
        GameObject petExpObject = Instantiate(item, petExpPanel);
        PetExpBgItem petExpBgItem = petExpObject.GetComponent<PetExpBgItem>();
        Button button = petExpBgItem.GetComponent<Button>();

        button.onClick.AddListener(() => OnSelected(petData, index));
        petExpBgItem.SetPetExpBgItem(petData);

        expBagList.Add(petExpBgItem);
    }

    // 选择宠物回调
    private void OnSelected(PetData petData, int index)
    {
        SelectPet(petData, index);
    }

    // 选择宠物并更新UI
    private void SelectPet(PetData petData, int index)
    {
        // 更新左侧宠物信息
        string resPath = $"Sprites/PetImage/Pet_{petData.Id}/1";
        Sprite sprite = ResourcesManager.Instance.Load<Sprite>(resPath);
        leftPetImg.sprite = sprite;
        leftPetName.text = petData.Basic.Name;

        // 更新右侧宠物详细信息
        rightId.text = "序号:" + petData.Id;
        rightName.text = "名字:" + petData.Basic.Name;
        rightLevel.text = "等级:" + petData.Exp.Level;

        PersonalityData personalityData = _personalitySystem.GetPersonalityById(petData.Basic.Personality);
        rightNature.text = "性格:" + personalityData.name;

        rightExpInfo.text = petData.Exp.CurrentExp.ToString();
        rightTotalExp.text = totalExp.ToString();

        _currentSelectedIndex = index;

        // 更新选中状态背景
        for (int i = 0; i < expBagList.Count; i++)
        {
            if (i == index)
            {
                expBagList[i].petGridBg.sprite = ResourcesManager.Instance.Load<Sprite>(SELECT_BG_PATH);
            }
            else
            {
                expBagList[i].petGridBg.sprite = ResourcesManager.Instance.Load<Sprite>(NORMAL_BG_PATH);
            }
        }
    }
}