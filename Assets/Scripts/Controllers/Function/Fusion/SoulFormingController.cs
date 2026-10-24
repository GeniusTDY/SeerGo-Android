using QFramework;
using SeerGoGame.Controller;
using SeerGoGame.Events;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using Random = UnityEngine.Random;

public class SoulFormingController : BaseController
{
    [Header("UI组件")]
    [SerializeField] private TextMeshProUGUI PageNum;         // 页码显示
    [SerializeField] private Button CloseButton;   // 关闭按钮
    [SerializeField] private Button PreButton;     // 上一页按钮
    [SerializeField] private Button NextButton;    // 下一页按钮
    [SerializeField] private Button ConfirmButton; // 确认按钮
    [SerializeField] private GameObject SoulItem;   // 物品项预制体
    [SerializeField] private Transform SoulItemParent; // 物品项父容器

    private IItemModel itemModel;
    private IArchiveModel archiveModel;
    private IArchiveSystem archiveSystem;
    private IPetInfoSystem petInfoSystem;
    private IPetAttributeCalcSystem peAttributeCalcSystem;
    private ISkillSystem skillSystem;
    private IExpConfigSystem expConfigSystem;

    // 珠子物品实例列表
    private class BeadItemInfo
    {
        public int ItemId;           // 物品配置ID
        public int ItemCount;        // 拥有数量
        public int PetId;            // 精灵ID
        public string PetName;       // 精灵名称
        public int FeatureId;        // 特性ID
        public string FeatureName;   // 特性名称
        public int StorageIndex;     // 在ItemStorage中的索引
    }
    private List<BeadItemInfo> beadItems = new List<BeadItemInfo>();
    // 物品项缓存
    private List<GameObject> soulItemObjects = new List<GameObject>();
    // 选中的珠子索引列表
    private List<int> selectedIndices = new List<int>();
    // 当前页选中状态
    private HashSet<int> currentPageSelected = new HashSet<int>();

    // 分页相关
    private const int ITEMS_PER_PAGE = 5;
    private int currentPage = 1;
    private int totalPages = 1;

    void Start()
    {
        this.RegisterEvent<SoulFormingChangeEventOpenEvent>(OnSoulFormingOpenEvent)
    .UnRegisterWhenGameObjectDestroyed(gameObject);
        itemModel = this.GetModel<IItemModel>();
        archiveModel = this.GetModel<IArchiveModel>();
        archiveSystem = this.GetSystem<IArchiveSystem>();
        petInfoSystem = this.GetSystem<IPetInfoSystem>();
        peAttributeCalcSystem = this.GetSystem<IPetAttributeCalcSystem>();
        skillSystem = this.GetSystem<ISkillSystem>();
        expConfigSystem = this.GetSystem<IExpConfigSystem>();

        // 绑定按钮事件
        CloseButton.onClick.AddListener(OnCloseClick);
        PreButton.onClick.AddListener(() => ChangePage(-1));
        NextButton.onClick.AddListener(() => ChangePage(1));
        ConfirmButton.onClick.AddListener(OnConfirmClick);
    }

    private void OnSoulFormingOpenEvent(SoulFormingChangeEventOpenEvent @event)
    {
        SeerGoLogger.Log("[灵魂融合] 收到打开事件，加载珠子数据...");
        LoadBeadItems();
    }

    /// <summary>
    /// 从存档的itemStorage中获取玩家拥有的珠子（type="bead"）
    /// </summary>
    private void LoadBeadItems()
    {
        beadItems.Clear();
        selectedIndices.Clear();
        currentPageSelected.Clear();

        // 获取存档中的物品存储
        var itemStorage = archiveModel.CurrentData.ItemStorage;

        // 获取所有珠子类型的道具配置（用于过滤）
        var beadConfigIds = new HashSet<int>();
        var beadConfigs = itemModel.GetByType("bead");
        foreach (var config in beadConfigs)
            beadConfigIds.Add(config.id);

        // 遍历玩家背包中的物品，记录索引
        int storageIndex = 0;
        foreach (var itemData in itemStorage)
        {
            // 检查是否是珠子类型
            if (!beadConfigIds.Contains(itemData.Id))
            {
                storageIndex++;
                continue;
            }

            var itemConfig = itemModel.GetById(itemData.Id);
            if (itemConfig == null)
            {
                storageIndex++;
                continue;
            }

            // 每个物品实例都要显示
            for (int i = 0; i < itemData.Num; i++)
            {
                var beadInfo = new BeadItemInfo
                {
                    ItemId = itemData.Id,
                    ItemCount = 1,
                    PetId = itemData.FusionPetId,
                    FeatureId = itemData.FusionFeatureId,
                    StorageIndex = storageIndex
                };

                // 获取精灵名称
                var monster = petInfoSystem.GetMonsterById(itemData.FusionPetId);
                beadInfo.PetName = monster != null ? monster.DefName : $"精灵{itemData.FusionPetId}";

                // 获取特性名称
                var seConfig = NewSeManager.Instance.GetConfigById(itemData.FusionFeatureId);
                beadInfo.FeatureName = seConfig != null ? seConfig.Desc : $"特性{itemData.FusionFeatureId}";

                beadItems.Add(beadInfo);
                SeerGoLogger.Log($"<color=cyan>[灵魂融合] 发现珠子: {itemConfig.name} 精灵:{beadInfo.PetName} 特性:{beadInfo.FeatureName}</color>");
            }
            storageIndex++;
        }

        // 计算分页
        totalPages = Mathf.Max(1, Mathf.CeilToInt((float)beadItems.Count / ITEMS_PER_PAGE));
        currentPage = 1;

        SeerGoLogger.Log($"<color=cyan>[灵魂融合] 共找到 {beadItems.Count} 个珠子实例，分 {totalPages} 页</color>");

        // 显示第一页
        UpdatePage();
    }

    /// <summary>
    /// 更新当前页显示
    /// </summary>
    private void UpdatePage()
    {
        // 清空现有物品项
        ClearSoulItems();
        currentPageSelected.Clear();

        // 计算当前页显示的物品范围
        int startIndex = (currentPage - 1) * ITEMS_PER_PAGE;
        int endIndex = Mathf.Min(startIndex + ITEMS_PER_PAGE, beadItems.Count);

        // 创建物品项
        for (int i = startIndex; i < endIndex; i++)
        {
            CreateSoulItem(i, beadItems[i]);
        }

        // 更新页码显示
        PageNum.text = $"{currentPage}/{totalPages}";

        SeerGoLogger.Log($"<color=cyan>[灵魂融合] 显示第 {currentPage}/{totalPages} 页，选中 {selectedIndices.Count} 个</color>");
    }

    /// <summary>
    /// 创建单个物品项
    /// </summary>
    private void CreateSoulItem(int index, BeadItemInfo beadInfo)
    {
        var itemConfig = itemModel.GetById(beadInfo.ItemId);
        if (itemConfig == null) return;

        // 创建物品项
        GameObject itemObj = Instantiate(SoulItem, SoulItemParent);
        soulItemObjects.Add(itemObj);

        // 获取或添加 SoulItemCell 组件并初始化
        var cell = itemObj.GetComponent<SoulItemCell>();
        if (cell == null)
            cell = itemObj.AddComponent<SoulItemCell>();

        // 检查当前页内是否被选中
        bool isSelected = selectedIndices.Contains(index);
        if (isSelected)
            currentPageSelected.Add(index);

        // 传入宠物名称和特性名称
        cell.InitializeWithFeature(itemConfig, beadInfo.PetName, beadInfo.FeatureName, isSelected, (id) => OnSoulItemClick(index, id));
    }

    /// <summary>
    /// 清空所有物品项
    /// </summary>
    private void ClearSoulItems()
    {
        foreach (var obj in soulItemObjects)
        {
            if (obj != null)
                Destroy(obj);
        }
        soulItemObjects.Clear();
    }

    /// <summary>
    /// 点击物品项
    /// </summary>
    private void OnSoulItemClick(int index, int itemId)
    {
        var beadInfo = beadItems[index];
        SeerGoLogger.Log($"[灵魂融合] 点击珠子: 索引={index}, 精灵={beadInfo.PetName}, 特性={beadInfo.FeatureName}");

        // 先重置当前页所有选中项的背景
        ResetAllPageSelected();

        // 切换选中状态
        if (currentPageSelected.Contains(index))
        {
            // 取消选中
            currentPageSelected.Remove(index);
            selectedIndices.Remove(index);
            UpdateSingleItemState(index, false);
        }
        else
        {
            // 选中
            currentPageSelected.Add(index);
            selectedIndices.Add(index);
            UpdateSingleItemState(index, true);
        }

        SeerGoLogger.Log($"[灵魂融合] 当前选中 {selectedIndices.Count} 个珠子");
    }

    /// <summary>
    /// 重置当前页所有选中项的选中状态
    /// </summary>
    private void ResetAllPageSelected()
    {
        foreach (var idx in currentPageSelected)
        {
            UpdateSingleItemState(idx, false);
        }
        // 同时清除 selectedIndices 中的记录，确保不会遗留之前选中的项
        foreach (var idx in currentPageSelected)
        {
            selectedIndices.Remove(idx);
        }
        currentPageSelected.Clear();
    }

    /// <summary>
    /// 更新单个物品项的选中状态
    /// </summary>
    private void UpdateSingleItemState(int index, bool isSelected)
    {
        int startIndex = (currentPage - 1) * ITEMS_PER_PAGE;
        int localIndex = index - startIndex;

        if (localIndex >= 0 && localIndex < soulItemObjects.Count)
        {
            var cell = soulItemObjects[localIndex].GetComponent<SoulItemCell>();
            if (cell != null)
                cell.SetSelected(isSelected);
        }
    }

    /// <summary>
    /// 确认选择
    /// </summary>
    private void OnConfirmClick()
    {
        if (selectedIndices.Count == 0)
        {
            SeerGoLogger.Log("[灵魂融合] 请先选择珠子");
            return;
        }

        SeerGoLogger.Log($"[灵魂融合] 确认选择 {selectedIndices.Count} 个珠子:");
        foreach (var idx in selectedIndices)
        {
            var bead = beadItems[idx];
            SeerGoLogger.Log($"  - 精灵:{bead.PetName} 特性:{bead.FeatureName} StorageIndex:{bead.StorageIndex}");

            // 调用 ProcessGetPetReward，传入完整的珠子信息
            ProcessGetPetReward(bead.PetId, bead.ItemId, bead.PetId, bead.FeatureId);
        }

        // 清空选中状态
        selectedIndices.Clear();
        currentPageSelected.Clear();
    }
    public void ProcessGetPetReward(int petId, int itemId, int fusionPetId, int fusionFeatureId)
    {
        MonsterData monsterBaseData = petInfoSystem.GetMonsterById(petId);
        if (monsterBaseData == null)
        {
            SeerGoLogger.LogWarning($"无法获取怪物数据，petId:{petId}");
            return;
        }

        PetData newMonsterData = new PetData();

        newMonsterData.Id = petId;
        newMonsterData.Basic.Id = petId;
        newMonsterData.Basic.Name = monsterBaseData.DefName;
        newMonsterData.Basic.Gender = monsterBaseData.Gender;
        newMonsterData.Basic.Personality = Random.Range(1, 25);
        newMonsterData.Basic.Attributes = monsterBaseData.Type;
        newMonsterData.Basic.GetPetDate = DateTime.Now.ToString("yyyy-MM-dd");

        newMonsterData.Feature.FeatureId = fusionFeatureId; // 使用融合的特性ID
        newMonsterData.Exp.Id = petId;
        newMonsterData.Exp.Level = 1;
        newMonsterData.Exp.CurrentExp = expConfigSystem.GetExpValue(1, monsterBaseData.GrowthType);

        newMonsterData.Talent.Id = petId;
        newMonsterData.Talent.IV = Random.Range(1, 32);

        newMonsterData.Skills.Id = petId;
        List<LearnableMove> learnableMoves = petInfoSystem.GetLearnableMoves(petId);
        if (learnableMoves != null)
        {
            for (int i = 0; i < learnableMoves.Count; i++)
            {
                if (learnableMoves[i].LearningLv == 1)
                {
                    newMonsterData.Skills.OwnSkillId.Add(learnableMoves[i].ID);
                    EquippedSkill equippedSkill = new EquippedSkill();
                    equippedSkill.id = learnableMoves[i].ID;
                    equippedSkill.maxPP = skillSystem.GetSkillById(learnableMoves[i].ID.ToString()).MaxPP;
                    equippedSkill.currentPP = equippedSkill.maxPP;
                    newMonsterData.Skills.NormalSkill.Add(equippedSkill);
                }
            }
        }

        // 计算宠物属性
        peAttributeCalcSystem.CalculateAllStats(newMonsterData);

        // 减少元神珠数量（根据 Id、FusionPetId、FusionFeatureId 查找匹配的物品）
        var itemData = archiveModel.CurrentData.ItemStorage.Find(item =>
            item.Id == itemId &&
            item.FusionPetId == fusionPetId &&
            item.FusionFeatureId == fusionFeatureId);
        if (itemData != null)
        {
            itemData.Num--;
            SeerGoLogger.Log($"[灵魂融合] 消耗元神珠，剩余数量: {itemData.Num}");
            if (itemData.Num <= 0)
            {
                archiveModel.CurrentData.ItemStorage.Remove(itemData);
            }
        }
        else
        {
            SeerGoLogger.LogWarning($"[灵魂融合] 未找到匹配的元神珠物品，itemId:{itemId}, fusionPetId:{fusionPetId}, fusionFeatureId:{fusionFeatureId}");
        }

        bool isBagFull = archiveModel.CurrentData.PetBag.Count >= 6;

        if (archiveModel.CurrentData.PetBag.Count < 6)
        {
            archiveModel.CurrentData.PetBag.Add(newMonsterData);
        }
        else
        {
            archiveModel.CurrentData.PetStorage.Add(newMonsterData);
        }

        // 保存存档
        archiveSystem.SaveGame("autosave0", archiveModel.CurrentData);

        // 弹窗提示
        Dialog1GetPetManager.Instance.ShowPopupOnTransform(
            FunctionPanelLoader.Instance.GetCanvas().transform,
            isBagFull ?
                $"恭喜你获得了<color=green>{newMonsterData.Basic.Name}</color>，背包已满，已放入仓库" :
                $"恭喜你获得了<color=green>{newMonsterData.Basic.Name}</color>，你可以点击右下方的精灵按钮",
            newMonsterData,
            isBagFull,
            onConfirm: () =>
            {
                LoadBeadItems();
            }
        );



    }

    /// <summary>
    /// 切换页码
    /// </summary>
    private void ChangePage(int offset)
    {
        int newPage = currentPage + offset;
        if (newPage >= 1 && newPage <= totalPages)
        {
            currentPage = newPage;
            UpdatePage();
        }
    }

    /// <summary>
    /// 关闭面板
    /// </summary>
    private void OnCloseClick()
    {
        ClearSoulItems();
        FunctionPanelLoader.Instance.ClosePanel("Fusion/SoulFormingPanel");
    }
}
