using PixeLadder.SimpleTooltip;
using QFramework;
using SeerGoGame.Commands;
using SeerGoGame.Controller;
using SeerGoGame.Events;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

public class PetStroageController : BaseController
{
    [Header("左半UI")]
    [SerializeField] private GameObject PetAttrPanel; // 左侧属性面板
    [SerializeField] private Button AllButton, CloseButton, AttrPreButton, AttrNextButton, SingleAttrButton, DualAttrButton; // 左侧按钮
    [SerializeField] private Image SingleAttrBtnImg, DualAttrBtnImg; // 单/双属性按钮图标（用于颜色切换）
    [SerializeField] private TextMeshProUGUI AttrPageNum, PetCounts; // 左侧文本

    [Header("中间UI")]
    [SerializeField] private Button PetPreButton, PetNextButton; // 宠物分页按钮
    [SerializeField] private TextMeshProUGUI PetPageNum; // 宠物分页文本
    [SerializeField] private Button AddToBagButton, PetStroageButton, ReleaseStroageButton; // 功能按钮
    [SerializeField] private GameObject PetItemPanel; // 宠物项容器

    [Header("右半UI")]
    [SerializeField] private TextMeshProUGUI petId, petName, petLevel, petIV, petPersonality, PetFeature, petGetTime, petAtk, petSpAtk, petDef, petSpDef, petSpd, petHp; // 宠物信息文本
    [SerializeField] private Button ReleasePetButton, ShowButton; // 右侧功能按钮

    [Header("预制体")]
    [SerializeField] private GameObject PetItem, PetItemAttr; // 宠物项和属性项预制体

    // 分页设置
    private const int AttrPageSize = 16, PetPageSize = 9; // 每页显示数量
    private int CurrentAttrPage = 1, TotalAttrPages = 1, CurrentPetPage = 1, TotalPetPages = 1; // 当前页码和总页数

    // 颜色设置
    private readonly Color normalColor = Color.white, petNormalColor = Color.white, petSelectedColor = Color.yellow; // 默认和选中颜色

    // 系统接口
    private IAttributeSystem _attributeSystem; // 属性系统
    private IAttributeModel _attributeModel; // 属性模型（读取全量配置）
    private IArchiveModel _archiveModel; // 存档模型
    private IArchiveSystem _archiveSystem;
    private IPersonalitySystem _personalitySystem; // 性格系统

    // 属性数据
    private List<AttributeData> allSingleAttributes = new List<AttributeData>(); // 所有单属性
    private List<AttributeData> allDualAttributes = new List<AttributeData>(); // 所有双属性
    private List<AttributeData> currentDisplayAttributes = new List<AttributeData>(); // 当前显示的属性列表（单/双切换）
    private List<Image> attrItemImages = new List<Image>(); // 属性项Image组件列表
    private int selectedAttrId = -1; // 选中的属性真实ID
    private int selectedAttrLocalIndex = -1; // 选中的属性在当前页的本地索引
    private bool isDualMode = false; // 当前是否为双属性模式

    // 宠物数据
    private List<PetData> currentAllPetStroageData = new List<PetData>(); // 当前仓库所有宠物数据
    private List<(PetData pet, int storageIndex)> filteredPetData = new List<(PetData pet, int storageIndex)>(); // 筛选后的宠物数据（包含原始索引）
    private int selectedPetIndex = -1, selectedPetStorageIndex = -1; // 选中的宠物索引（筛选列表和原始列表）

    // 宠物项对象池
    private List<GameObject> petItemPool = new List<GameObject>(); // 宠物项对象池
    private List<Image> petItemImages = new List<Image>(); // 宠物项Image组件列表
    private List<PetStroagePetItemCell> petItemCells = new List<PetStroagePetItemCell>(); // 宠物项组件列表

    void Start()
    {
        // 注册宠物仓库打开事件
        this.RegisterEvent<PetStroageOpenEvent>(OnPetStroageOpen).UnRegisterWhenGameObjectDestroyed(gameObject);

        // 获取系统接口
        _attributeSystem = this.GetSystem<IAttributeSystem>();
        _attributeModel = this.GetModel<IAttributeModel>();
        _archiveModel = this.GetModel<IArchiveModel>();
        _personalitySystem = this.GetSystem<IPersonalitySystem>();
        _archiveSystem = this.GetSystem<IArchiveSystem>();

        // 尝试获取按钮Image组件（若未在Inspector中拖入则自动获取）
        if (SingleAttrBtnImg == null && SingleAttrButton != null)
            SingleAttrBtnImg = SingleAttrButton.GetComponent<Image>();
        if (DualAttrBtnImg == null && DualAttrButton != null)
            DualAttrBtnImg = DualAttrButton.GetComponent<Image>();

        // 绑定按钮事件
        PetPreButton.onClick.AddListener(OnPetPrePage);
        PetNextButton.onClick.AddListener(OnPetNextPage);
        AttrPreButton.onClick.AddListener(OnAttrPrePage);
        AttrNextButton.onClick.AddListener(OnAttrNextPage);
        CloseButton.onClick.AddListener(() => FunctionPanelLoader.Instance.ClosePanel("PetStroage/PetStroagePanel"));
        AllButton.onClick.AddListener(OnAllButtonClick);
        AddToBagButton.onClick.AddListener(OnAddToBagClick);
        ReleasePetButton.onClick.AddListener(OnReleasePetClick);
        SingleAttrButton.onClick.AddListener(OnSingleAttrClick);
        DualAttrButton.onClick.AddListener(OnDualAttrClick);

        // 初始化数据
        GetAllAttributes();
        InitializePetItemPool();
    }

    /// <summary>
    /// 从属性配置读取全部属性，分离单属性和双属性
    /// </summary>
    private void GetAllAttributes()
    {
        allSingleAttributes.Clear();
        allDualAttributes.Clear();

        foreach (var kvp in _attributeModel.AttributeDict)
        {
            if (kvp.Value.is_dou == 1)
                allDualAttributes.Add(kvp.Value);
            else
                allSingleAttributes.Add(kvp.Value);
        }

        // 按ID排序
        allSingleAttributes.Sort((a, b) => a.id.CompareTo(b.id));
        allDualAttributes.Sort((a, b) => a.id.CompareTo(b.id));

        // 默认显示单属性
        currentDisplayAttributes = allSingleAttributes;
    }

    /// <summary>
    /// 切换到单属性显示模式
    /// </summary>
    private void OnSingleAttrClick()
    {
        if (!isDualMode) return; // 已是单属性模式
        isDualMode = false;
        currentDisplayAttributes = allSingleAttributes;
        InitData();
    }

    /// <summary>
    /// 切换到双属性显示模式
    /// </summary>
    private void OnDualAttrClick()
    {
        if (isDualMode) return; // 已是双属性模式
        isDualMode = true;
        currentDisplayAttributes = allDualAttributes;
        InitData();
    }

    /// <summary>
    /// 更新单/双属性按钮颜色：选中变黄，未选中恢复白色
    /// </summary>
    private void UpdateModeButtonColors()
    {
        if (SingleAttrBtnImg != null)
            SingleAttrBtnImg.color = isDualMode ? normalColor : Color.yellow;
        if (DualAttrBtnImg != null)
            DualAttrBtnImg.color = isDualMode ? Color.yellow : normalColor;
    }

    /// <summary>
    /// 初始化宠物项对象池
    /// </summary>
    private void InitializePetItemPool()
    {
        for (int i = 0; i < PetPageSize; i++) // 创建一页最大数量的宠物项
        {
            GameObject petItem = Instantiate(PetItem, PetItemPanel.transform);
            Image itemImage = petItem.GetComponent<Image>() ?? petItem.AddComponent<Image>();
            PetStroagePetItemCell itemCell = petItem.GetComponent<PetStroagePetItemCell>();

            petItemPool.Add(petItem);
            petItemImages.Add(itemImage);
            petItemCells.Add(itemCell);
            petItem.SetActive(false); // 初始不激活
        }
    }

    /// <summary>
    /// 处理宠物仓库打开事件
    /// </summary>
    private void OnPetStroageOpen(PetStroageOpenEvent @event) => InitData();

    /// <summary>
    /// 初始化数据（每次打开仓库或切换单/双属性时调用）
    /// </summary>
    private void InitData()
    {
        // 清空现有数据
        ClearAttrItems();
        HideAllPetItems();
        ClearSelectedPet();

        // 获取当前仓库数据
        currentAllPetStroageData = _archiveModel.CurrentData.PetStorage;

        // 重置选中状态
        selectedAttrId = -1;
        selectedAttrLocalIndex = -1;

        // 计算属性总页数
        TotalAttrPages = Mathf.Max(1, Mathf.CeilToInt(currentDisplayAttributes.Count / (float)AttrPageSize));
        CurrentAttrPage = 1;
        UpdateAttrPageInfo();

        // 更新按钮颜色
        UpdateModeButtonColors();

        // 加载第一页属性和全部宠物
        LoadCurrentAttrPage();
        OnAllButtonClick(); // 初始加载所有宠物
    }

    /// <summary>
    /// 隐藏所有宠物项
    /// </summary>
    private void HideAllPetItems()
    {
        foreach (var petItem in petItemPool)
        {
            if (petItem != null)
            {
                petItem.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 加载当前属性页
    /// </summary>
    private void LoadCurrentAttrPage()
    {
        ClearAttrItems();

        int startIndex = (CurrentAttrPage - 1) * AttrPageSize;
        int endIndex = Mathf.Min(startIndex + AttrPageSize, currentDisplayAttributes.Count);

        for (int i = startIndex; i < endIndex; i++)
        {
            CreateAttrItem(currentDisplayAttributes[i], i - startIndex);
        }
    }

    /// <summary>
    /// 创建属性项
    /// </summary>
    private void CreateAttrItem(AttributeData attributeData, int localIndex)
    {
        GameObject attrItem = Instantiate(PetItemAttr, PetAttrPanel.transform);
        Image itemImage = attrItem.GetComponent<Image>() ?? attrItem.AddComponent<Image>();
        attrItemImages.Add(itemImage);
        itemImage.color = normalColor; // 设置默认颜色

        // 初始化属性项组件
        attrItem.GetComponent<PetStroageAttrItemCell>().Initialize(attributeData);

        // 添加点击事件（闭包捕获真实attribute ID）
        int attrId = attributeData.id;
        Button itemButton = attrItem.GetComponent<Button>() ?? attrItem.AddComponent<Button>();
        itemButton.onClick.RemoveAllListeners();
        itemButton.onClick.AddListener(() => OnAttrSelected(attrId, localIndex));

        // 添加提示
        TooltipTrigger.AddTooltip(attrItem, null, attributeData.cn, Color.yellow, null,
            FunctionPanelLoader.Instance.gameObject.GetComponent<Canvas>());
    }

    /// <summary>
    /// 处理属性项点击（使用真实属性ID筛选）
    /// </summary>
    private void OnAttrSelected(int attributeId, int localIndex)
    {
        // 重置所有属性项颜色
        ResetColors(attrItemImages, normalColor);
        if (localIndex >= 0 && localIndex < attrItemImages.Count && attrItemImages[localIndex] != null)
            attrItemImages[localIndex].color = Color.yellow; // 设置选中颜色

        selectedAttrId = attributeId;
        selectedAttrLocalIndex = localIndex;
        filteredPetData.Clear();

        // 筛选对应属性的宠物（按真实属性ID匹配）
        for (int i = 0; i < currentAllPetStroageData.Count; i++)
        {
            if (currentAllPetStroageData[i].Basic.Attributes == attributeId)
                filteredPetData.Add((currentAllPetStroageData[i], i));
        }

        ClearSelectedPet();
        UpdatePetCounts();
        LoadPetsData();
    }

    /// <summary>
    /// 处理"全部"按钮点击
    /// </summary>
    private void OnAllButtonClick()
    {
        // 重置所有属性项颜色
        ResetColors(attrItemImages, normalColor);
        selectedAttrId = -1;
        selectedAttrLocalIndex = -1;
        filteredPetData.Clear();

        // 添加所有宠物及其原始索引
        for (int i = 0; i < currentAllPetStroageData.Count; i++)
            filteredPetData.Add((currentAllPetStroageData[i], i));

        ClearSelectedPet();
        UpdatePetCounts();
        LoadPetsData();
    }

    /// <summary>
    /// 加载宠物分页数据
    /// </summary>
    private void LoadPetsData()
    {
        // 计算宠物总页数
        TotalPetPages = Mathf.Max(1, Mathf.CeilToInt(filteredPetData.Count / (float)PetPageSize));

        // 确保当前页码不超出范围
        if (CurrentPetPage > TotalPetPages)
        {
            CurrentPetPage = Mathf.Max(1, TotalPetPages);
        }

        UpdatePetPageInfo();

        // 加载当前页宠物
        LoadCurrentPetPage();
    }

    /// <summary>
    /// 加载当前页宠物
    /// </summary>
    private void LoadCurrentPetPage()
    {
        // 首先隐藏所有宠物项
        HideAllPetItems();

        // 重置所有宠物项颜色
        ResetColors(petItemImages, petNormalColor);

        int startIndex = (CurrentPetPage - 1) * PetPageSize;
        int endIndex = Mathf.Min(startIndex + PetPageSize, filteredPetData.Count);

        // 激活并设置当前页的宠物项
        for (int i = 0; i < PetPageSize; i++)
        {
            int itemIndex = i;

            // 检查是否还有数据
            if (startIndex + i >= endIndex)
            {
                // 超出数据范围，保持隐藏状态
                continue;
            }

            int filteredIndex = startIndex + i;
            var petInfo = filteredPetData[filteredIndex];

            // 激活并设置宠物项
            SetupPetItem(itemIndex, petInfo, filteredIndex);
        }
    }

    /// <summary>
    /// 设置宠物项
    /// </summary>
    private void SetupPetItem(int itemIndex, (PetData pet, int storageIndex) petInfo, int filteredIndex)
    {
        if (itemIndex < 0 || itemIndex >= petItemPool.Count) return;

        GameObject petItem = petItemPool[itemIndex];
        if (petItem == null) return;

        // 激活宠物项
        petItem.SetActive(true);

        // 设置颜色
        if (itemIndex < petItemImages.Count && petItemImages[itemIndex] != null)
        {
            petItemImages[itemIndex].color = petNormalColor;
        }

        // 初始化宠物项显示
        if (itemIndex < petItemCells.Count && petItemCells[itemIndex] != null)
        {
            petItemCells[itemIndex].Initialize(petInfo.pet.Id);
        }

        // 绑定点击事件
        Button petButton = petItem.GetComponent<Button>() ?? petItem.AddComponent<Button>();
        petButton.onClick.RemoveAllListeners();
        petButton.onClick.AddListener(() => OnPetSelected(itemIndex, filteredIndex, petInfo.storageIndex));
    }

    /// <summary>
    /// 处理宠物项点击
    /// </summary>
    private void OnPetSelected(int itemIndex, int filteredIndex, int storageIndex)
    {
        // 重置所有宠物项颜色
        ResetColors(petItemImages, petNormalColor);

        // 设置选中项颜色
        if (itemIndex >= 0 && itemIndex < petItemImages.Count && petItemImages[itemIndex] != null)
            petItemImages[itemIndex].color = petSelectedColor; // 设置选中颜色

        // 记录选中索引
        selectedPetIndex = filteredIndex;
        selectedPetStorageIndex = storageIndex;

        // 更新右侧宠物信息
        if (selectedPetStorageIndex >= 0 && selectedPetStorageIndex < currentAllPetStroageData.Count)
            UpdatePetInfo(currentAllPetStroageData[selectedPetStorageIndex]);
    }

    /// <summary>
    /// 更新右侧宠物信息面板
    /// </summary>
    private void UpdatePetInfo(PetData petData)
    {
        petId.text = "序号:" + petData.Id;
        petName.text = "名字:" + petData.Basic.Name;
        petLevel.text = "等级:" + petData.Exp.Level;
        petPersonality.text = "性格:" + _personalitySystem.GetPersonalityById(petData.Basic.Personality).name;
        petIV.text = "个体值:" + petData.Talent.IV;
        petGetTime.text = "获得时间:" + petData.Basic.GetPetDate;
        petAtk.text = "攻击:" + petData.CurrentStatus.Atk;
        petSpAtk.text = "特攻:" + petData.CurrentStatus.SpAtk;
        petDef.text = "防御:" + petData.CurrentStatus.Def;
        petSpDef.text = "特防:" + petData.CurrentStatus.SpDef;
        petSpd.text = "速度:" + petData.CurrentStatus.Spd;
        petHp.text = "体力:" + petData.CurrentStatus.Hp;

        // 特性信息
        int featureId = petData.Feature.FeatureId;
        if (featureId > 0)
        {
            var seConfig = NewSeManager.Instance.GetConfigById(featureId);
            string desc = seConfig != null ? seConfig.Desc : "未知特性";
            PetFeature.text = "特性:" + desc;
            PetFeature.gameObject.SetActive(true);
        }
        else
        {
            PetFeature.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 处理添加到背包按钮点击
    /// </summary>
    private void OnAddToBagClick()
    {
        if (selectedPetStorageIndex == -1)
        {
            Dialog1Manager.Instance.ShowPopupOnTransform(FunctionPanelLoader.Instance.transform, "", "请先选择要添加到背包的宠物");
            return;
        }

        if (_archiveModel.CurrentData.PetBag.Count < 6)
        {
            if (selectedPetStorageIndex >= 0 && selectedPetStorageIndex < currentAllPetStroageData.Count)
            {
                var selectedPet = currentAllPetStroageData[selectedPetStorageIndex];

                _archiveModel.CurrentData.PetBag.Add(selectedPet);
                // 从原始数据列表中移除
                currentAllPetStroageData.RemoveAt(selectedPetStorageIndex);

                _archiveSystem.SaveGame("autosave0", _archiveModel.CurrentData);
                Dialog1GetPetManager.Instance.ShowPopupOnTransform(FunctionPanelLoader.Instance.transform,
                    $"<color=red>{selectedPet.Basic.Name}</color>已经放入你的精灵背包",
                    selectedPet,
                    false,
                    () =>
                    {
                        // 刷新数据
                        OnAllButtonClick();
                        this.SendCommand<ChangePetBagCommand>();
                    });
            }
        }
        else
        {
            // 加载背包面板
            FunctionPanelLoader.Instance.LoadPanel("PetStroage/ReplacePetPanel", (panel) =>
            {
                if (panel != null)
                {
                    this.SendCommand(new ChangeReplacePetCommand(selectedPetStorageIndex));
                }
            });
        }
    }

    /// <summary>
    /// 处理放生按钮点击
    /// </summary>
    private void OnReleasePetClick()
    {
        if (selectedPetStorageIndex == -1)
        {
            Dialog1Manager.Instance.ShowPopupOnTransform(FunctionPanelLoader.Instance.transform, "", "请先选择要放生的宠物");
            return;
        }

        Dialog2Manager.Instance.ShowPopupOnTransform(FunctionPanelLoader.Instance.transform, 
            "", "该操作会永久放生精灵，请谨慎选择！",
            () =>
                {
                    currentAllPetStroageData.RemoveAt(selectedPetStorageIndex);
                    // 刷新数据
                    OnAllButtonClick();
                    _archiveSystem.SaveGame("autosave0", _archiveModel.CurrentData);
                    Dialog1Manager.Instance.ShowPopupOnTransform(FunctionPanelLoader.Instance.transform, "", "放生成功!");
                }
            );

      
    }

    /// <summary>
    /// 刷新数据
    /// </summary>
    private void RefreshData()
    {
        // 重新获取仓库数据
        currentAllPetStroageData = _archiveModel.CurrentData.PetStorage;

        // 根据当前筛选条件重新加载（使用真实属性ID）
        if (selectedAttrId >= 0) OnAttrSelected(selectedAttrId, selectedAttrLocalIndex);
        else OnAllButtonClick();
    }

    /// <summary>
    /// 清空选中的宠物状态
    /// </summary>
    private void ClearSelectedPet()
    {
        selectedPetIndex = selectedPetStorageIndex = -1;
        ClearPetInfo();
    }

    /// <summary>
    /// 清空右侧宠物信息
    /// </summary>
    private void ClearPetInfo()
    {
        petId.text = "序号:"; petName.text = "名字:"; petLevel.text = "等级:";
        petPersonality.text = "性格:"; petIV.text = "个体值:"; petGetTime.text = "获得时间:";
        petAtk.text = "攻击:"; petSpAtk.text = "特攻:"; petDef.text = "防御:";
        petSpDef.text = "特防:"; petSpd.text = "速度:"; petHp.text = "体力:";
        PetFeature.gameObject.SetActive(false);
    }

    /// <summary>
    /// 清空属性项
    /// </summary>
    private void ClearAttrItems()
    {
        foreach (Transform child in PetAttrPanel.transform) Destroy(child.gameObject);
        attrItemImages.Clear();
        selectedAttrId = -1;
        selectedAttrLocalIndex = -1;
    }

    /// <summary>
    /// 重置指定列表的颜色
    /// </summary>
    private void ResetColors(List<Image> images, Color color)
    {
        for (int i = 0; i < images.Count; i++)
        {
            if (images[i] != null) images[i].color = color;
        }
    }

    /// <summary>
    /// 更新宠物数量显示
    /// </summary>
    private void UpdatePetCounts() => PetCounts.text = $"{filteredPetData.Count}";

    /// <summary>
    /// 更新属性分页信息
    /// </summary>
    private void UpdateAttrPageInfo() => UpdatePageInfo(AttrPageNum, CurrentAttrPage, TotalAttrPages, AttrPreButton, AttrNextButton);

    /// <summary>
    /// 更新宠物分页信息
    /// </summary>
    private void UpdatePetPageInfo() => UpdatePageInfo(PetPageNum, CurrentPetPage, TotalPetPages, PetPreButton, PetNextButton);

    /// <summary>
    /// 通用分页信息更新
    /// </summary>
    private void UpdatePageInfo(TextMeshProUGUI text, int current, int total, Button prev, Button next)
    {
        text.text = $"{current}/{total}";
        prev.interactable = current > 1;
        next.interactable = current < total;
    }

    /// <summary>
    /// 属性上一页按钮点击事件
    /// </summary>
    public void OnAttrPrePage()
    {
        if (CurrentAttrPage > 1)
        {
            CurrentAttrPage--;
            UpdateAttrPageInfo();
            LoadCurrentAttrPage();
            OnAllButtonClick();
        }
    }

    /// <summary>
    /// 属性下一页按钮点击事件
    /// </summary>
    public void OnAttrNextPage()
    {
        if (CurrentAttrPage < TotalAttrPages)
        {
            CurrentAttrPage++;
            UpdateAttrPageInfo();
            LoadCurrentAttrPage();
            OnAllButtonClick();
        }
    }

    /// <summary>
    /// 宠物上一页按钮点击事件
    /// </summary>
    public void OnPetPrePage()
    {
        if (CurrentPetPage > 1)
        {
            CurrentPetPage--;
            LoadCurrentPetPage();
            UpdatePetPageInfo();
        }
    }

    /// <summary>
    /// 宠物下一页按钮点击事件
    /// </summary>
    public void OnPetNextPage()
    {
        if (CurrentPetPage < TotalPetPages)
        {
            CurrentPetPage++;
            LoadCurrentPetPage();
            UpdatePetPageInfo();
        }
    }

    /// <summary>
    /// 销毁时清理对象池
    /// </summary>
    private void OnDestroy()
    {
        foreach (var petItem in petItemPool)
        {
            if (petItem != null) Destroy(petItem);
        }
        petItemPool.Clear();
        petItemImages.Clear();
        petItemCells.Clear();
    }
}