using PixeLadder.SimpleTooltip;
using QFramework;
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
using Image = UnityEngine.UI.Image;

public class FusionController : BaseController
{
    [SerializeField] private Button PreButton, NextButton, FusionButton, ConfirmButton, FusionCloseButton, BagCloseButton;
    [SerializeField] private Transform PetPanel;
    [SerializeField] private GameObject PetItem;
    [SerializeField] private GameObject SelectPetPanel;

    [SerializeField] private GameObject LeftFusionSpriteAnim;
    [SerializeField] private GameObject RightFusionSpriteAnim;
    [SerializeField] private GameObject FusionAnim;

    // ================= Left 区域 =================
    [SerializeField] private Transform Left;
    [SerializeField] private Button LeftItem1;       // 对应 Item1 (Button)
    [SerializeField] private Image LeftItem1Image;  // 对应 ItemImage1 (Image)
    [SerializeField] private Button LeftItem2;       // 对应 Item2 (Button)
    [SerializeField] private Image LeftItem2Image;  // 对应 ItemImage2 (Image)
    [SerializeField] private Button LeftItem3;       // 对应 Item3 (Button)
    [SerializeField] private Image LeftItem3Image;  // 对应 ItemImage3 (Image)
    [SerializeField] private Button LeftItem4;       // 对应 Item4 (Button)
    [SerializeField] private Image LeftItem4Image;  // 对应 ItemImage4 (Image)

    // 新增 Left Sprite 相关
    [SerializeField] private Button LeftSpriteButton;    // 对应 LeftSpriteButton
    [SerializeField] private Image LeftSpriteImage;     // 对应 LeftSpriteImage
    [SerializeField] private TextMeshProUGUI LeftPetName;
    [SerializeField] private TextMeshProUGUI RightPetName;

    // ================= Right 区域 =================
    [SerializeField] private Transform Right;
    [SerializeField] private Button RightItem5;      // 对应 Item5 (Button)
    [SerializeField] private Image RightItem5Image; // 对应 ItemImage5 (Image)
    [SerializeField] private Button RightItem6;      // 对应 Item6 (Button)
    [SerializeField] private Image RightItem6Image; // 对应 ItemImage6 (Image)
    [SerializeField] private Button RightItem7;      // 对应 Item7 (Button)
    [SerializeField] private Image RightItem7Image; // 对应 ItemImage7 (Image)
    [SerializeField] private Button RightItem8;      // 对应 Item8 (Button)
    [SerializeField] private Image RightItem8Image; // 对应 ItemImage8 (Image)

    // 新增 Right Sprite 相关
    [SerializeField] private Button RightSpriteButton;   // 对应 RightSpriteButton
    [SerializeField] private Image RightSpriteImage;    // 对应 RightSpriteImage

    // ================= Center 区域 =================
    [SerializeField] private Transform Center;
    [SerializeField] private Button CenterUseItem1;      // 对应 UseItem1 (Button)
    [SerializeField] private Image CenterUseItem1Image; // 对应 UseItemImage1 (Image)
    [SerializeField] private Image CenterUseItem1Bg;   // CenterUseItem1 的背景图片
    [SerializeField] private Button CenterUseItem2;      // 对应 UseItem2 (Button)
    [SerializeField] private Image CenterUseItem2Image; // 对应 UseItemImage2 (Image)
    [SerializeField] private Image CenterUseItem2Bg;   // CenterUseItem2 的背景图片
    [SerializeField] private Button CenterUseItem3;      // 对应 UseItem3 (Button)
    [SerializeField] private Image CenterUseItem3Image; // 对应 UseItemImage3 (Image)
    [SerializeField] private Image CenterUseItem3Bg;   // CenterUseItem3 的背景图片
    [SerializeField] private Button CenterUseItem4;      // 对应 UseItem4 (Button)
    [SerializeField] private Image CenterUseItem4Image; // 对应 UseItemImage4 (Image)
    [SerializeField] private Image CenterUseItem4Bg;   // CenterUseItem4 的背景图片

    // ================= 材料选择相关 =================
    private const int CENTER_SLOT_COUNT = 4; // Center区域有4个材料槽
    private int[] centerSlotItemIds = new int[CENTER_SLOT_COUNT]; // 每个槽位对应的物品ID，0表示空
    private int selectedCenterSlot = -1; // 当前选中的Center槽位索引，-1表示未选中
    private Color defaultSlotColor = Color.white; // 默认图片颜色
    private Color selectedSlotColor = new Color(1f, 1f, 0.5f, 1f); // 选中时淡黄色
    private Color[] centerSlotOriginalColors; // 保存每个槽位图片的原始颜色

    // Center槽位数组
    private Image[] centerSlotImages;
    private Image[] centerSlotBgs;
    private Button[] centerSlotButtons;
    private Sprite defaultItemSlotSprite;

    // ================= 矿物材料相关 =================
    private IItemModel itemModel;
    private IArchiveModel archiveModel;
    private List<int> mineralItemIds = new List<int>(); // 玩家拥有的矿物材料ID列表

    // 分页相关
    private const int ITEMS_PER_PAGE = 8; // 每页最多显示8个
    private int currentPage = 1;
    private int totalPages = 1;

    // 矿物材料槽位数组（Left 4个 + Right 4个）
    private Image[] itemSlotImages;

    // ================= 精灵选择相关 =================
    private List<PetBagItem> petBagItems = new List<PetBagItem>(); // 已创建的精灵项
    private List<PetData> allPets = new List<PetData>(); // 所有宠物数据
    private List<PetData> filteredPets = new List<PetData>(); // 过滤后的宠物数据（根据融合条件）
    private PetData selectedLeftPet;  // 左边选择的精灵
    private PetData selectedRightPet; // 右边选择的精灵
    private PetData currentSelectingPet; // 当前在SelectPetPanel中选中的精灵
    private bool isSelectingLeft = true; // 当前是否在为左边选择
    private int selectedPetIndex = -1; // SelectPetPanel中当前选中的宠物索引
    private IPetInfoSystem petInfoSystem; // 精灵信息查询系统
    private IArchiveSystem archiveSystem;

    // 精灵选择相关颜色
    private Color normalPetBgColor = Color.white;
    private Color selectedPetBgColor = new Color(1f, 1f, 0.5f, 1f); // 淡黄色
    private Color disabledPetBgColor = new Color(0.5f, 0.5f, 0.5f, 0.5f); // 灰色半透明
    private Color buttonHighlightColor = new Color(0.8f, 0.6f, 0.2f, 1f); // 深黄色
    private Sprite defaultSpriteButtonSprite; // 按钮默认图片
    private Sprite defaultSpriteImageSprite; // 精灵图片默认图片

    void Start()
    {
        this.RegisterEvent<FusionChangeEventOpenEvent>(OnFusionOpenEvent)
    .UnRegisterWhenGameObjectDestroyed(gameObject);

        FusionCloseButton.onClick.AddListener(() => OnCloseClick());
        BagCloseButton.onClick.AddListener(() => OnBagCloseClick());
        PreButton.onClick.AddListener(() => ChangePage(-1));
        NextButton.onClick.AddListener(() => ChangePage(1));

        // 添加精灵选择按钮点击事件
        if (LeftSpriteButton != null)
            LeftSpriteButton.onClick.AddListener(() => OnSpriteButtonClick(true));
        if (RightSpriteButton != null)
            RightSpriteButton.onClick.AddListener(() => OnSpriteButtonClick(false));

        itemModel = this.GetModel<IItemModel>();
        archiveModel = this.GetModel<IArchiveModel>();
        petInfoSystem = this.GetSystem<IPetInfoSystem>();
        archiveSystem = this.GetSystem<IArchiveSystem>();

        // 初始化槽位数组
        itemSlotImages = new Image[]
        {
            LeftItem1Image, LeftItem2Image, LeftItem3Image, LeftItem4Image,   // Left区域 4个
            RightItem5Image, RightItem6Image, RightItem7Image, RightItem8Image // Right区域 4个
        };

        // 矿物槽位按钮数组
        Button[] mineralSlotButtons = new Button[]
        {
            LeftItem1, LeftItem2, LeftItem3, LeftItem4,   // Left区域 4个
            RightItem5, RightItem6, RightItem7, RightItem8 // Right区域 4个
        };

        // 初始化Center槽位数组
        centerSlotImages = new Image[]
        {
            CenterUseItem1Image, CenterUseItem2Image, CenterUseItem3Image, CenterUseItem4Image
        };
        centerSlotBgs = new Image[]
        {
            CenterUseItem1Bg, CenterUseItem2Bg, CenterUseItem3Bg, CenterUseItem4Bg
        };
        centerSlotButtons = new Button[]
        {
            CenterUseItem1, CenterUseItem2, CenterUseItem3, CenterUseItem4
        };

        // 加载默认的空槽位图片
        defaultItemSlotSprite = ResourcesManager.Instance.Load<Sprite>($"Sprites/Function/Fusion/Item");

        // 加载精灵选择默认图片
        defaultSpriteButtonSprite = ResourcesManager.Instance.Load<Sprite>("Sprites/Function/Fusion/DefaultSpriteBg");
        defaultSpriteImageSprite = ResourcesManager.Instance.Load<Sprite>("Sprites/Function/Fusion/1");

        // 初始化Center槽位物品ID数组
        for (int i = 0; i < CENTER_SLOT_COUNT; i++)
        {
            centerSlotItemIds[i] = 0;
        }

        // 初始化Center槽位图片的原始颜色
        centerSlotOriginalColors = new Color[CENTER_SLOT_COUNT];
        for (int i = 0; i < CENTER_SLOT_COUNT; i++)
        {
            centerSlotOriginalColors[i] = centerSlotImages[i] != null ? centerSlotImages[i].color : Color.white;
        }

        // 添加Center槽位点击事件
        for (int i = 0; i < CENTER_SLOT_COUNT; i++)
        {
            int slotIndex = i;
            centerSlotButtons[i].onClick.AddListener(() => OnCenterSlotClick(slotIndex));
        }

        // 添加左右矿物槽位点击事件
        for (int i = 0; i < ITEMS_PER_PAGE; i++)
        {
            int slotIndex = i;
            mineralSlotButtons[i].onClick.AddListener(() => OnMineralSlotClick(slotIndex));
        }

        // 初始化Center槽位显示默认图片
        ResetCenterSlots();

        // 加载融合配置
        LoadFusionConfig();

        // 添加融合按钮点击事件
        FusionButton.onClick.AddListener(() => OnFusionClick());
    }

    #region 融合配置数据
    // ================= 融合配置数据 =================
    // 融合配置缓存
    private List<FusionRecipe> fusionRecipes = new List<FusionRecipe>();
    private List<FusionFormula> fusionFormulas = new List<FusionFormula>();
    private bool fusionConfigLoaded = false;

    /// <summary>
    /// 加载融合配置文件
    /// </summary>
    private void LoadFusionConfig()
    {
        if (fusionConfigLoaded) return;

        // 加载融合配方
        LoadFusionRecipes();

        // 加载融合公式
        LoadFusionFormulas();

        fusionConfigLoaded = true;
        SeerGoLogger.Log($"[融合] 加载配置完成，配方案例: {fusionRecipes.Count}, 公式: {fusionFormulas.Count}");
    }

    /// <summary>
    /// 加载融合配方 FusionRecipes.xml
    /// </summary>
    private void LoadFusionRecipes()
    {
        TextAsset configFile = Resources.Load<TextAsset>("Config/FusionRecipes");
        if (configFile == null)
        {
            SeerGoLogger.LogError("[融合] FusionRecipes.xml 未找到");
            return;
        }

        try
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(configFile.text);

            System.Xml.XmlNodeList groupNodes = doc.SelectNodes("//group");
            foreach (System.Xml.XmlNode groupNode in groupNodes)
            {
                FusionRecipe recipe = new FusionRecipe();

                // 解析 mainId
                System.Xml.XmlNode mainIdNode = groupNode.SelectSingleNode("mainId");
                if (mainIdNode != null)
                    int.TryParse(mainIdNode.InnerText, out recipe.MainId);

                // 解析所有 subId（可能有多个）
                System.Xml.XmlNodeList subIdNodes = groupNode.SelectNodes("subId");
                foreach (System.Xml.XmlNode subIdNode in subIdNodes)
                {
                    if (int.TryParse(subIdNode.InnerText, out int subId))
                    {
                        recipe.SubIds.Add(subId);
                    }
                }

                // 解析 results
                System.Xml.XmlNode resultsNode = groupNode.SelectSingleNode("results");
                if (resultsNode != null)
                {
                    System.Xml.XmlNodeList resultNodes = resultsNode.SelectNodes("result");
                    foreach (System.Xml.XmlNode resultNode in resultNodes)
                    {
                        FusionRecipeResult result = new FusionRecipeResult();
                        if (resultNode.Attributes["id"] != null)
                            int.TryParse(resultNode.Attributes["id"].Value, out result.Id);
                        if (resultNode.Attributes["rate"] != null)
                            int.TryParse(resultNode.Attributes["rate"].Value, out result.Rate);
                        if (resultNode.Attributes["beadid"] != null)
                            int.TryParse(resultNode.Attributes["beadid"].Value, out result.BeadId);
                        recipe.Results.Add(result);
                    }
                }

                fusionRecipes.Add(recipe);
            }

            SeerGoLogger.Log($"[融合] 加载 {fusionRecipes.Count} 个融合配方");
        }
        catch (System.Exception e)
        {
            SeerGoLogger.LogError($"[融合] 加载融合配方失败: {e.Message}");
        }
    }

    /// <summary>
    /// 加载融合公式 FusionFormula.xml
    /// </summary>
    private void LoadFusionFormulas()
    {
        TextAsset configFile = Resources.Load<TextAsset>("Config/FusionFormula");
        if (configFile == null)
        {
            SeerGoLogger.LogError("[融合] FusionFormula.xml 未找到");
            return;
        }

        try
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(configFile.text);

            System.Xml.XmlNodeList groupNodes = doc.SelectNodes("//group");
            foreach (System.Xml.XmlNode groupNode in groupNodes)
            {
                FusionFormula formula = new FusionFormula();

                // 解析 order 属性
                System.Xml.XmlAttribute orderAttr = groupNode.Attributes["order"];
                if (orderAttr != null)
                {
                    string[] orderStrs = orderAttr.Value.Split(',');
                    foreach (string str in orderStrs)
                    {
                        if (int.TryParse(str.Trim(), out int id))
                            formula.Order.Add(id);
                    }
                }

                // 解析 ids
                System.Xml.XmlNode idsNode = groupNode.SelectSingleNode("ids");
                if (idsNode != null)
                {
                    string[] idStrs = idsNode.InnerText.Split(',');
                    foreach (string str in idStrs)
                    {
                        if (int.TryParse(str.Trim(), out int id))
                            formula.Ids.Add(id);
                    }
                }

                fusionFormulas.Add(formula);
            }

            SeerGoLogger.Log($"[融合] 加载 {fusionFormulas.Count} 个融合公式");
        }
        catch (System.Exception e)
        {
            SeerGoLogger.LogError($"[融合] 加载融合公式失败: {e.Message}");
        }
    }

    /// <summary>
    /// 检查主宠和副宠是否可以融合（不能颠倒判断）
    /// </summary>
    private FusionRecipe CheckRecipeMatch(int mainId, int subId)
    {
        SeerGoLogger.Log($"[融合] 检查配方: 主宠={mainId}, 副宠={subId}, 可用配方案例: {fusionRecipes.Count}");

        foreach (var recipe in fusionRecipes)
        {
            // 主宠必须匹配，副宠必须在副宠列表中
            if (recipe.MainId == mainId && recipe.ContainsSubId(subId))
            {
                SeerGoLogger.Log($"[融合] 匹配到配方: 主宠={recipe.MainId}, 副宠列表=[{string.Join(",", recipe.SubIds)}], 结果ID={recipe.Results[0].Id}");
                return recipe;
            }
        }

        SeerGoLogger.LogWarning($"[融合] 主宠({mainId})和副宠({subId})无法融合 - 未在配方中找到");
        return null;
    }

    /// <summary>
    /// 根据材料顺序检查匹配的融合公式
    /// </summary>
    private FusionFormula CheckFormulaMatch(int[] itemOrder)
    {
        string currentOrder = string.Join(",", itemOrder);
        SeerGoLogger.Log($"[融合] 当前材料顺序: [{currentOrder}], 可用公式数: {fusionFormulas.Count}");

        foreach (var formula in fusionFormulas)
        {
            if (formula.Order.Count != 4)
                continue;

            string formulaOrder = string.Join(",", formula.Order);
            bool match = true;
            for (int i = 0; i < 4; i++)
            {
                // 只检查有物品的槽位
                if (itemOrder[i] > 0 && itemOrder[i] != formula.Order[i])
                {
                    match = false;
                    break;
                }
            }

            if (match)
            {
                SeerGoLogger.Log($"[融合] 匹配到公式: [{formulaOrder}]");
                return formula;
            }
        }

        SeerGoLogger.LogWarning("[融合] 未匹配到任何公式");
        return null;
    }

    /// <summary>
    /// 从特性列表中随机获取一个
    /// </summary>
    private int GetRandomSeId(List<int> seIds)
    {
        if (seIds == null || seIds.Count == 0)
            return 0;
        return seIds[UnityEngine.Random.Range(0, seIds.Count)];
    }

    /// <summary>
    /// 从全部特性配置中随机获取一个
    /// </summary>
    private int GetRandomSeIdFromAll()
    {
        // 使用 NewSeManager 获取全部特性
        int count = NewSeManager.Instance.GetConfigCount();
        SeerGoLogger.Log($"[融合] NewSeManager配置数量: {count}");
        
        if (count > 0)
        {
            var allSeIds = NewSeManager.Instance.GetAllSeIds();
            SeerGoLogger.Log($"[融合] 获取到的特性ID列表: {string.Join(",", allSeIds)}");
            
            if (allSeIds != null && allSeIds.Count > 0)
            {
                return GetRandomSeId(allSeIds);
            }
        }
        SeerGoLogger.LogError("[融合] 未能获取到任何特性配置，无法随机特性");
        return 0;
    }

    /// <summary>
    /// 验证融合条件
    /// </summary>
    private bool ValidateFusion()
    {
        // 1. 检查主宠和副宠是否已选择
        if (selectedLeftPet == null || selectedRightPet == null)
        {
            SeerGoLogger.LogWarning("[融合] 请先选择主宠和副宠");
            return false;
        }

        // 2. 检查主宠和副宠是否可以融合
        int mainId = selectedLeftPet.Id;
        int subId = selectedRightPet.Id;
        FusionRecipe recipe = CheckRecipeMatch(mainId, subId);
        if (recipe == null)
        {
            SeerGoLogger.LogWarning($"[融合] 主宠({mainId})和副宠({subId})无法融合");
            return false;
        }

        // 3. 检查Center槽位是否正好有4个材料
        int materialCount = 0;
        foreach (int itemId in centerSlotItemIds)
        {
            if (itemId > 0)
                materialCount++;
        }
        if (materialCount != 4)
        {
            SeerGoLogger.LogWarning($"[融合] 材料数量必须为4个，当前只有 {materialCount} 个");
            return false;
        }

        // 4. 检查4个物品的数量是否充足（需要统计每个物品需要消耗的数量）
        var requiredItems = new Dictionary<int, int>();
        foreach (int itemId in centerSlotItemIds)
        {
            if (itemId > 0)
            {
                if (!requiredItems.ContainsKey(itemId))
                    requiredItems[itemId] = 0;
                requiredItems[itemId]++;
            }
        }

        foreach (var kvp in requiredItems)
        {
            int itemId = kvp.Key;
            int needCount = kvp.Value;
            int playerCount = GetItemCount(itemId);
            if (playerCount < needCount)
            {
                SeerGoLogger.LogWarning($"[融合] 物品 {itemId} 数量不足，需要 {needCount} 个，玩家只有 {playerCount} 个");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 获取玩家物品数量
    /// </summary>
    private int GetItemCount(int itemId)
    {
        if (archiveModel?.CurrentData?.ItemStorage != null)
        {
            foreach (var item in archiveModel.CurrentData.ItemStorage)
            {
                if (item.Id == itemId)
                    return item.Num;
            }
        }
        return 0;
    }

    /// <summary>
    /// 消耗物品
    /// </summary>
    private void ConsumeItems()
    {
        foreach (int itemId in centerSlotItemIds)
        {
            if (itemId > 0)
            {
                RemoveItem(itemId, 1);
            }
        }
    }

    /// <summary>
    /// 移除物品
    /// </summary>
    private void RemoveItem(int itemId, int count)
    {
        if (archiveModel?.CurrentData?.ItemStorage == null)
            return;

        for (int i = archiveModel.CurrentData.ItemStorage.Count - 1; i >= 0; i--)
        {
            var item = archiveModel.CurrentData.ItemStorage[i];
            if (item.Id == itemId)
            {
                item.Num -= count;
                if (item.Num <= 0)
                {
                    archiveModel.CurrentData.ItemStorage.RemoveAt(i);
                }
                break;
            }
        }
    }

    /// <summary>
    /// 更新融合按钮状态
    /// </summary>
    private void UpdateFusionButtonState()
    {
        if (FusionButton == null) return;

        bool canFuse = ValidateFusionButton();
        FusionButton.interactable = canFuse;

        if (!canFuse)
        {
            // 设置为灰色
            ColorBlock colors = FusionButton.colors;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            FusionButton.colors = colors;
        }
    }

    /// <summary>
    /// 验证融合按钮是否可点击（不打印日志）
    /// </summary>
    private bool ValidateFusionButton()
    {
        // 检查主宠和副宠是否已选择
        if (selectedLeftPet == null || selectedRightPet == null)
            return false;

        // 检查主宠和副宠是否可以融合
        int mainId = selectedLeftPet.Id;
        int subId = selectedRightPet.Id;
        if (CheckRecipeMatch(mainId, subId) == null)
            return false;

        // 检查Center槽位是否正好有4个材料
        int materialCount = 0;
        foreach (int itemId in centerSlotItemIds)
        {
            if (itemId > 0)
                materialCount++;
        }
        if (materialCount != 4)
            return false;

        // 检查4个物品的数量是否充足（需要统计每个物品需要消耗的数量）
        var requiredItems = new Dictionary<int, int>();
        foreach (int itemId in centerSlotItemIds)
        {
            if (itemId > 0)
            {
                if (!requiredItems.ContainsKey(itemId))
                    requiredItems[itemId] = 0;
                requiredItems[itemId]++;
            }
        }

        foreach (var kvp in requiredItems)
        {
            if (GetItemCount(kvp.Key) < kvp.Value)
                return false;
        }

        return true;
    }

    /// <summary>
    /// 点击融合按钮
    /// </summary>
    private void OnFusionClick()
    {
        SeerGoLogger.Log("[融合] 点击融合按钮");

        // 验证融合条件
        if (!ValidateFusion())
        {
            return;
        }

        // 获取配方和公式
        int mainId = selectedLeftPet.Id;
        int subId = selectedRightPet.Id;
        FusionRecipe recipe = CheckRecipeMatch(mainId, subId);
        FusionFormula formula = CheckFormulaMatch(centerSlotItemIds);

        // 1. 消耗物品
        ConsumeItems();

        // 2. 从配方结果中根据概率随机获取结果精灵
        int resultPetId = 0;
        int beadId = 0;
        if (recipe.Results.Count > 0)
        {
            int totalRate = 0;
            foreach (var r in recipe.Results)
                totalRate += r.Rate;

            int randomValue = UnityEngine.Random.Range(0, totalRate);
            int currentRate = 0;
            foreach (var r in recipe.Results)
            {
                currentRate += r.Rate;
                if (randomValue < currentRate)
                {
                    resultPetId = r.Id;
                    beadId = r.BeadId;
                    break;
                }
            }
        }

        // 3. 从公式中随机获取特性ID（如果公式有结果），否则从全部特性随机
        int resultSeId = 0;
        if (formula != null && formula.Ids != null && formula.Ids.Count > 0)
        {
            resultSeId = GetRandomSeId(formula.Ids);
        }
        if (resultSeId == 0)
        {
            SeerGoLogger.Log("[融合] 未匹配到公式，从全部特性中随机获取");
            resultSeId = GetRandomSeIdFromAll();
        }

        SeerGoLogger.Log($"[融合] 融合成功！结果精灵: {resultPetId}, 珠子: {beadId}, 特性: {resultSeId}");

        // 检查 ItemStorage 中是否已存在相同的物品
        var itemStorage = archiveModel.CurrentData.ItemStorage;
        var existingItem = itemStorage.Find(item =>
            item.Id == beadId &&
            item.FusionPetId == resultPetId &&
            item.FusionFeatureId == resultSeId);

        if (existingItem != null)
        {
            // 已存在，Num+1
            existingItem.Num++;
            SeerGoLogger.Log($"[融合] 叠加到已有物品，当前数量: {existingItem.Num}");
        }
        else
        {
            // 不存在，创建新物品
            ItemData newItemData = new ItemData();
            newItemData.Id = beadId;
            newItemData.Num = 1;
            newItemData.FusionPetId = resultPetId;
            newItemData.FusionFeatureId = resultSeId;
            itemStorage.Add(newItemData);
            SeerGoLogger.Log($"[融合] 创建新物品到背包");
        }
        // 保存存档
        archiveSystem.SaveGame("autosave0", archiveModel.CurrentData);
        Left.gameObject.SetActive(false);
        Right.gameObject.SetActive(false);
        Center.gameObject.SetActive(false);

        FusionAnim.GetComponent<OneShotAnim>().Play(2.7f, () => {
            // 弹窗提示
            Dialog1Manager.Instance.ShowPopupOnTransform(
                FunctionPanelLoader.Instance.GetCanvas().transform,
                "提示",
                $"恭喜你获得了<color=red>1个</color>元神珠",
                onConfirm: () =>
                {
                    Left.gameObject.SetActive(true);
                    Right.gameObject.SetActive(true);
                    Center.gameObject.SetActive(true);
                    OnCloseClick();
                }
            );
        });
    }
    #endregion

    private void OnFusionOpenEvent(FusionChangeEventOpenEvent @event)
    {
        // 打开时重置所有状态
        ResetPetSelection();
        ClearPetBagItems();
        LoadMineralItems();
        ResetCenterSlots();
        UpdateFusionButtonState();
    }

    /// <summary>
    /// 点击精灵选择按钮（LeftSpriteButton 或 RightSpriteButton）
    /// </summary>
    private void OnSpriteButtonClick(bool isLeft)
    {
        isSelectingLeft = isLeft;
        SeerGoLogger.Log($"[融合] 点击精灵选择按钮 (Left: {isLeft})");

        // 设置按钮高亮
        if (isLeft)
            LeftSpriteButton.GetComponent<Image>().color = buttonHighlightColor;
        else
            RightSpriteButton.GetComponent<Image>().color = buttonHighlightColor;

        // 清空当前选择状态
        selectedPetIndex = -1;
        currentSelectingPet = null;

        // 加载宠物列表并显示
        LoadPetBag();

        // 更新已选择精灵的灰色状态
        UpdatePetItemGrayState();

        SelectPetPanel.SetActive(true);
    }

    private void OnCloseClick()
    {
        // 关闭前重置状态
        ResetPetSelection();
        ClearPetBagItems();
        OnBagCloseClick();
        FunctionPanelLoader.Instance.ClosePanel("Fusion/FusionPanel");
    }

    /// <summary>
    /// 点击关闭按钮隐藏SelectPetPanel
    /// </summary>
    private void OnBagCloseClick()
    {
        SelectPetPanel.SetActive(false);
        // 恢复按钮背景色
        ResetSpriteButtonHighlight();
        SeerGoLogger.Log("[融合] 关闭精灵选择面板");
    }

    /// <summary>
    /// 恢复按钮高亮状态
    /// </summary>
    private void ResetSpriteButtonHighlight()
    {
        if (selectedLeftPet == null && LeftSpriteButton != null)
            LeftSpriteButton.GetComponent<Image>().color = normalPetBgColor;
        if (selectedRightPet == null && RightSpriteButton != null)
            RightSpriteButton.GetComponent<Image>().color = normalPetBgColor;
    }

    /// <summary>
    /// 更新宠物项的灰色状态（已被选择的精灵显示为灰色）
    /// </summary>
    private void UpdatePetItemGrayState()
    {
        for (int i = 0; i < petBagItems.Count; i++)
        {
            PetData petData = allPets[i];
            PetBagItem item = petBagItems[i];
            bool isGray = false;

            // 如果是左边选择模式，检查是否被右边选中
            if (isSelectingLeft && selectedRightPet != null && petData.Id == selectedRightPet.Id)
                isGray = true;

            // 如果是右边选择模式，检查是否被左边选中
            if (!isSelectingLeft && selectedLeftPet != null && petData.Id == selectedLeftPet.Id)
                isGray = true;

            // 应用灰色状态
            if (item.petBgImg != null)
            {
                item.petBgImg.color = isGray ? disabledPetBgColor : normalPetBgColor;
                // 同时禁用按钮
                if (item.petBgButton != null)
                    item.petBgButton.interactable = !isGray;
            }

            // 清除当前选中高亮
            if (i != selectedPetIndex)
            {
                item.petBgImg.color = isGray ? disabledPetBgColor : normalPetBgColor;
            }
        }
    }

    /// <summary>
    /// 点击Center槽位（选择材料槽）
    /// </summary>
    private void OnCenterSlotClick(int slotIndex)
    {
        // 如果该槽位已有物品，再次点击则移除
        if (centerSlotItemIds[slotIndex] > 0)
        {
            // 移除物品，恢复默认图片
            centerSlotItemIds[slotIndex] = 0;
            centerSlotImages[slotIndex].sprite = defaultItemSlotSprite;

            // 取消选中高亮
            ClearSlotHighlight(slotIndex);

            SeerGoLogger.Log($"[融合] 移除Center槽位 {slotIndex + 1} 的物品");
            UpdateFusionButtonState();
            return;
        }

        // 如果当前没有选中的槽位，则选中这个槽位
        if (selectedCenterSlot == -1)
        {
            selectedCenterSlot = slotIndex;
            HighlightSlot(slotIndex);
            SeerGoLogger.Log($"[融合] 选中Center槽位 {slotIndex + 1}，请选择矿物材料");
        }
        else
        {
            // 如果点击的是已选中的槽位，取消选中
            if (selectedCenterSlot == slotIndex)
            {
                ClearSlotHighlight(slotIndex);
                selectedCenterSlot = -1;
                SeerGoLogger.Log($"[融合] 取消选中Center槽位 {slotIndex + 1}");
            }
            else
            {
                // 切换选中槽位
                ClearSlotHighlight(selectedCenterSlot);
                selectedCenterSlot = slotIndex;
                HighlightSlot(slotIndex);
                SeerGoLogger.Log($"[融合] 切换选中Center槽位为 {slotIndex + 1}");
            }
        }
    }

    /// <summary>
    /// 点击矿物材料槽位（从材料列表选择）
    /// </summary>
    private void OnMineralSlotClick(int slotIndex)
    {
        // 计算实际矿物ID
        int mineralIndex = (currentPage - 1) * ITEMS_PER_PAGE + slotIndex;
        if (mineralIndex >= mineralItemIds.Count)
        {
            SeerGoLogger.LogWarning($"[融合] 矿物槽位 {slotIndex} 无效");
            return;
        }

        int itemId = mineralItemIds[mineralIndex];

        // 如果有选中的Center槽位，则放置物品
        if (selectedCenterSlot != -1)
        {
            // 检查物品数量是否足够
            int currentCount = 0;
            foreach (int id in centerSlotItemIds)
            {
                if (id == itemId) currentCount++;
            }
            int needCount = currentCount + 1; // 放置后需要消耗的数量
            int playerCount = GetItemCount(itemId);
            
            if (playerCount < needCount)
            {
                var itemConfig = itemModel.GetById(itemId);
                string itemName = itemConfig != null ? itemConfig.name : $"物品{itemId}";
                Dialog1Manager.Instance.ShowPopupOnTransform(
                    FunctionPanelLoader.Instance.GetCanvas().transform,
                    "提示",
                    $"融合材料不足，{itemName}背包只有{playerCount}个"
                );
                return;
            }

            // 放置物品到选中的Center槽位（可重复放置同一矿物）
            centerSlotItemIds[selectedCenterSlot] = itemId;
            Sprite sprite = ResourcesManager.Instance.Load<Sprite>($"Sprites/Item/{itemId}");
            if (sprite != null)
            {
                centerSlotImages[selectedCenterSlot].sprite = sprite;
            }

            // 取消选中高亮
            ClearSlotHighlight(selectedCenterSlot);
            selectedCenterSlot = -1;

            var itemConfig2 = itemModel.GetById(itemId);
            string itemName2 = itemConfig2 != null ? itemConfig2.name : $"物品{itemId}";
            SeerGoLogger.Log($"[融合] 将矿物 {itemName2} (ID:{itemId}) 放置到Center槽位");

            // 更新融合按钮状态
            UpdateFusionButtonState();
        }
        else
        {
            SeerGoLogger.Log($"[融合] 请先点击Center槽位选择要放置的位置");
        }
    }

    /// <summary>
    /// 高亮显示选中的Center槽位（CenterUseItemImage变淡黄色）
    /// </summary>
    private void HighlightSlot(int slotIndex)
    {
        if (centerSlotImages[slotIndex] != null)
        {
            centerSlotImages[slotIndex].color = selectedSlotColor; // 淡黄色
        }
    }

    /// <summary>
    /// 清除槽位高亮（恢复原始颜色）
    /// </summary>
    private void ClearSlotHighlight(int slotIndex)
    {
        if (centerSlotImages[slotIndex] != null)
        {
            centerSlotImages[slotIndex].color = centerSlotOriginalColors[slotIndex]; // 恢复原始颜色
        }
    }

    /// <summary>
    /// 重置所有Center槽位
    /// </summary>
    private void ResetCenterSlots()
    {
        for (int i = 0; i < CENTER_SLOT_COUNT; i++)
        {
            centerSlotItemIds[i] = 0;
            centerSlotImages[i].sprite = defaultItemSlotSprite;
            ClearSlotHighlight(i);
        }
        selectedCenterSlot = -1;
    }

    /// <summary>
    /// 获取Center槽位中放置的所有物品ID
    /// </summary>
    public List<int> GetSelectedItemIds()
    {
        List<int> result = new List<int>();
        for (int i = 0; i < CENTER_SLOT_COUNT; i++)
        {
            if (centerSlotItemIds[i] > 0)
            {
                result.Add(centerSlotItemIds[i]);
            }
        }
        return result;
    }

    /// <summary>
    /// 切换分页
    /// </summary>
    private void ChangePage(int direction)
    {
        int newPage = currentPage + direction;
        if (newPage >= 1 && newPage <= totalPages)
        {
            currentPage = newPage;
            UpdatePage();
        }
    }

    /// <summary>
    /// 更新当前页的矿物材料显示
    /// </summary>
    private void UpdatePage()
    {
        // 更新分页按钮状态
        PreButton.interactable = currentPage > 1;
        NextButton.interactable = currentPage < totalPages;

        // 计算当前页显示的起始索引
        int startIndex = (currentPage - 1) * ITEMS_PER_PAGE;

        // 清空所有槽位
        foreach (var slotImage in itemSlotImages)
        {
            slotImage.sprite = null;
            slotImage.enabled = false;
        }

        // 显示当前页的矿物材料
        for (int i = 0; i < ITEMS_PER_PAGE; i++)
        {
            int itemIndex = startIndex + i;
            if (itemIndex >= mineralItemIds.Count)
                break;

            int itemId = mineralItemIds[itemIndex];
            var slotImage = itemSlotImages[i];

            // 加载物品图片
            Sprite sprite = ResourcesManager.Instance.Load<Sprite>($"Sprites/Item/{itemId}");
            if (sprite != null)
            {
                slotImage.sprite = sprite;
                slotImage.enabled = true;

                // 获取物品名称
                var itemConfig = itemModel.GetById(itemId);
                string itemName = itemConfig != null ? itemConfig.name : $"物品{itemId}";

                // 添加Tooltip提示
                TooltipTrigger.AddTooltip(
                    slotImage.gameObject,
                    null,
                    itemName,
                    Color.yellow,
                    null,
                    FunctionPanelLoader.Instance.GetCanvas()
                );
            }
            else
            {
                SeerGoLogger.LogWarning($"[融合] 无法加载矿物图片: Sprites/Item/{itemId}");
            }
        }

        SeerGoLogger.Log($"<color=cyan>[融合] 显示第 {currentPage}/{totalPages} 页</color>");
    }

    /// <summary>
    /// 从存档的itemStorage中获取玩家拥有的矿物材料（type="mineral"）
    /// </summary>
    private void LoadMineralItems()
    {
        mineralItemIds.Clear();

        // 获取存档中的物品存储
        var itemStorage = archiveModel.CurrentData.ItemStorage;

        // 获取所有矿物类型的道具配置
        var mineralConfigs = itemModel.GetByType("mineral");

        foreach (var config in mineralConfigs)
        {
            // 检查玩家是否拥有该矿物材料
            var itemData = itemStorage.Find(item => item.Id == config.id);
            if (itemData != null && itemData.Num > 0)
            {
                mineralItemIds.Add(config.id);
                SeerGoLogger.Log($"<color=yellow>[融合] 发现矿物材料: {config.name} (ID:{config.id}) 数量:{itemData.Num}</color>");
            }
        }

        // 计算分页
        totalPages = Mathf.Max(1, Mathf.CeilToInt((float)mineralItemIds.Count / ITEMS_PER_PAGE));
        currentPage = 1;

        SeerGoLogger.Log($"<color=cyan>[融合] 共找到 {mineralItemIds.Count} 种矿物材料，分 {totalPages} 页</color>");

        // 显示第一页
        UpdatePage();
    }

    /// <summary>
    /// 获取玩家拥有的所有矿物材料ID
    /// </summary>
    public List<int> GetMineralItemIds()
    {
        return mineralItemIds;
    }

    /// <summary>
    /// 根据矿物ID获取物品数量
    /// </summary>
    public int GetMineralItemCount(int itemId)
    {
        var itemData = archiveModel.CurrentData.ItemStorage.Find(item => item.Id == itemId);
        return itemData?.Num ?? 0;
    }

    /// <summary>
    /// 刷新矿物材料列表（当物品变更后调用）
    /// </summary>
    public void RefreshMineralItems()
    {
        LoadMineralItems();
    }

    /// <summary>
    /// 从存档petBag加载精灵列表到PetPanel（根据当前位置过滤可融合的精灵）
    /// </summary>
    private void LoadPetBag()
    {
        // 获取存档中的宠物列表
        List<PetData> petBag = archiveModel.CurrentData.PetBag;
        if (petBag == null || petBag.Count == 0)
        {
            SeerGoLogger.Log("[融合] 宠物列表为空");
            return;
        }

        // 清空之前的列表
        ClearPetBagItems();
        allPets.Clear();
        filteredPets.Clear();

        // 根据当前位置过滤精灵
        // LeftSpriteButton 是主宠位，需要 FuseMaster = 1
        // RightSpriteButton 是副宠位，需要 FuseSub = 1
        string fuseTypeName = isSelectingLeft ? "主宠" : "副宠";

        for (int i = 0; i < petBag.Count; i++)
        {
            PetData petData = petBag[i];
            MonsterData monsterData = petInfoSystem?.GetMonsterById(petData.Id);

            if (monsterData != null)
            {
                bool canFuse = false;
                if (isSelectingLeft && monsterData.FuseMaster == 1)
                {
                    canFuse = true;
                }
                else if (!isSelectingLeft && monsterData.FuseSub == 1)
                {
                    canFuse = true;
                }

                if (canFuse)
                {
                    filteredPets.Add(petData);
                }
            }
        }

        SeerGoLogger.Log($"[融合] {fuseTypeName}选择，共找到 {filteredPets.Count} 只可融合精灵");

        // 如果没有可融合的精灵，显示提示并返回
        if (filteredPets.Count == 0)
        {
            SeerGoLogger.LogWarning($"[融合] 没有可作为{fuseTypeName}的精灵");
            return;
        }

        // 设置 allPets 为过滤后的列表
        allPets.AddRange(filteredPets);

        // 遍历过滤后的宠物列表，创建PetItem并设置数据
        for (int i = 0; i < filteredPets.Count; i++)
        {
            PetData petData = filteredPets[i];
            int index = i;

            // 实例化PetItem到PetPanel下
            GameObject petItemObj = Instantiate(PetItem, PetPanel);
            PetBagItem petBagItem = petItemObj.GetComponent<PetBagItem>();
            if (petBagItem != null)
            {
                petBagItem.SetPetBagItem(petData);
                petBagItems.Add(petBagItem);

                // 添加点击事件
                if (petBagItem.petBgButton != null)
                {
                    petBagItem.petBgButton.onClick.RemoveAllListeners();
                    petBagItem.petBgButton.onClick.AddListener(() => OnPetItemClick(index));
                }
            }
            else
            {
                SeerGoLogger.LogWarning($"[融合] PetItem预制体缺少PetBagItem组件: {petItemObj.name}");
            }
        }

        SeerGoLogger.Log($"<color=cyan>[融合] 加载了 {petBagItems.Count} 个精灵</color>");
    }

    /// <summary>
    /// 点击宠物项
    /// </summary>
    private void OnPetItemClick(int index)
    {
        if (index < 0 || index >= allPets.Count || index >= petBagItems.Count)
            return;

        PetData petData = allPets[index];
        PetBagItem item = petBagItems[index];

        // 检查是否已被禁用（另一个位置已选择）
        bool isDisabled = false;
        if (isSelectingLeft && selectedRightPet != null && petData.Id == selectedRightPet.Id)
            isDisabled = true;
        if (!isSelectingLeft && selectedLeftPet != null && petData.Id == selectedLeftPet.Id)
            isDisabled = true;

        if (isDisabled)
        {
            SeerGoLogger.LogWarning($"[融合] 该精灵已被另一位置选择");
            return;
        }

        // 清除之前的选中高亮
        if (selectedPetIndex >= 0 && selectedPetIndex < petBagItems.Count)
        {
            petBagItems[selectedPetIndex].petBgImg.color = normalPetBgColor;
        }

        // 设置新的选中状态
        selectedPetIndex = index;
        currentSelectingPet = petData;
        item.petBgImg.color = selectedPetBgColor;

        SeerGoLogger.Log($"[融合] 选中精灵: {petData.Basic.Name} (ID:{petData.Id})");

        // 自动确认选择
        ConfirmPetSelection();
    }

    /// <summary>
    /// 确认精灵选择
    /// </summary>
    private void ConfirmPetSelection()
    {
        if (currentSelectingPet == null)
        {
            SeerGoLogger.LogWarning("[融合] 请先选择一只精灵");
            return;
        }

        // 根据是左边还是右边，设置对应的精灵
        if (isSelectingLeft)
        {
            selectedLeftPet = currentSelectingPet;
            // 设置精灵图片
            Sprite petSprite = ResourcesManager.Instance.Load<Sprite>($"Sprites/PetImage/Pet_{selectedLeftPet.Id}/1");
            if (petSprite != null)
                LeftSpriteImage.sprite = petSprite;
            // 设置按钮背景
            Sprite itemSprite = ResourcesManager.Instance.Load<Sprite>("Sprites/Function/Fusion/SpriteItem");
            if (itemSprite != null)
                LeftSpriteButton.GetComponent<Image>().sprite = itemSprite;
            // 设置名称
            if (LeftPetName != null)
                LeftPetName.text = selectedLeftPet.Basic.Name;
        }
        else
        {
            selectedRightPet = currentSelectingPet;
            // 设置精灵图片
            Sprite petSprite = ResourcesManager.Instance.Load<Sprite>($"Sprites/PetImage/Pet_{selectedRightPet.Id}/1");
            if (petSprite != null)
                RightSpriteImage.sprite = petSprite;
            // 设置按钮背景
            Sprite itemSprite = ResourcesManager.Instance.Load<Sprite>("Sprites/Function/Fusion/SpriteItem");
            if (itemSprite != null)
                RightSpriteButton.GetComponent<Image>().sprite = itemSprite;
            // 设置名称
            if (RightPetName != null)
                RightPetName.text = selectedRightPet.Basic.Name;
        }

        // 关闭选择面板
        SelectPetPanel.SetActive(false);
        ResetSpriteButtonHighlight();

        SeerGoLogger.Log($"<color=cyan>[融合] 确认选择精灵: {currentSelectingPet.Basic.Name}</color>");

        // 更新融合按钮状态
        UpdateFusionButtonState();
    }

    /// <summary>
    /// 清空所有宠物项
    /// </summary>
    private void ClearPetBagItems()
    {
        foreach (var item in petBagItems)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }
        petBagItems.Clear();
    }

    /// <summary>
    /// 获取左边选择的精灵
    /// </summary>
    public PetData GetSelectedLeftPet()
    {
        return selectedLeftPet;
    }

    /// <summary>
    /// 获取右边选择的精灵
    /// </summary>
    public PetData GetSelectedRightPet()
    {
        return selectedRightPet;
    }

    /// <summary>
    /// 重置所有精灵选择
    /// </summary>
    public void ResetPetSelection()
    {
        selectedLeftPet = null;
        selectedRightPet = null;
        currentSelectingPet = null;
        selectedPetIndex = -1;

        // 重置按钮颜色和图片
        if (LeftSpriteButton != null)
        {
            LeftSpriteButton.GetComponent<Image>().color = normalPetBgColor;
            if (defaultSpriteButtonSprite != null)
                LeftSpriteButton.GetComponent<Image>().sprite = defaultSpriteButtonSprite;
        }
        if (RightSpriteButton != null)
        {
            RightSpriteButton.GetComponent<Image>().color = normalPetBgColor;
            if (defaultSpriteButtonSprite != null)
                RightSpriteButton.GetComponent<Image>().sprite = defaultSpriteButtonSprite;
        }

        // 恢复精灵图片和清空名称
        if (LeftSpriteImage != null)
        {
            if (defaultSpriteImageSprite != null)
                LeftSpriteImage.sprite = defaultSpriteImageSprite;
            else
                LeftSpriteImage.sprite = null;
        }
        if (RightSpriteImage != null)
        {
            if (defaultSpriteImageSprite != null)
                RightSpriteImage.sprite = defaultSpriteImageSprite;
            else
                RightSpriteImage.sprite = null;
        }
        if (LeftPetName != null)
            LeftPetName.text = "";
        if (RightPetName != null)
            RightPetName.text = "";
    }
}