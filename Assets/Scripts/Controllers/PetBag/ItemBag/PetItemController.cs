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
using Button = UnityEngine.UI.Button;

public class PetItemController : BaseController
{
    [Header("左半背包")]
    [SerializeField] private GameObject ItemPanel;
    [SerializeField] private Button CloseButton;

    [Header("右半背包")]
    [SerializeField] private TextMeshProUGUI PageText;
    [SerializeField] private Button PreButton;
    [SerializeField] private Button NextButton;

    [Header("Item预制体")]
    [SerializeField] private GameObject ItemPrefab;

    [SerializeField] private PetBagController petBagController;
    [SerializeField] private Transform PetBagCanvas;
    private const int PageSize = 12;
    private int CurrentPage = 1;
    private int TotalPages = 1;

    private IArchiveModel archiveModel;
    private IItemModel itemModel;
    private IUISystem _UISystem;

    private List<ItemData> filteredItems = new List<ItemData>(); // 过滤后的物品列表
    private List<PetItemCell> itemCellPool = new List<PetItemCell>(); // 对象池
    private List<PetItemCell> activeCells = new List<PetItemCell>(); // 当前活动的Cell

    private const string BasePath = "Sprites/Item/";

    void Start()
    {
        this.RegisterEvent<PetBagItemOpenEvent>(OnPetBagItemOpenEvent)
            .UnRegisterWhenGameObjectDestroyed(gameObject);
        this.RegisterEvent<UseItemSuccessEvent>(OnOutBattleUseItem).UnRegisterWhenCurrentSceneUnloaded();

        archiveModel = this.GetModel<IArchiveModel>();
        itemModel = this.GetModel<IItemModel>();
        _UISystem = this.GetSystem<IUISystem>();

        CloseButton.onClick.AddListener(OnClose);
        PreButton.onClick.AddListener(OnPrePage);
        NextButton.onClick.AddListener(OnNextPage);

        InitializeFilteredItems();
        LoadCurrentPageItems();
    }

    private void OnOutBattleUseItem(UseItemSuccessEvent e)
    {
        ItemConfig itemConfig = itemModel.GetById(e.ItemId);
        PetData petData = archiveModel.CurrentData.PetBag[e.PetIndex];

        // OUTBATTLE 场景：控制器负责实际 HP/PP 等数据修改
        if (e.UseSence == "OUTBATTLE" && itemConfig.type == "healHP")
        {
            int healAmount = ParseHealAmount(itemConfig, petData);
            petData.CurrentStatus.Hp = Mathf.Clamp(
                petData.CurrentStatus.Hp + healAmount,
                0,
                petData.CurrentStatus.MaxHp
            );
        }

        // 保存数据
        this.SendCommand(new SaveArchiveDataCommand(archiveModel.CurrentData));
        // 刷新背包
        OnClose();
    }

    /// <summary>
    /// 解析恢复量：优先 sideEffectArg，兼容旧版 effectValue
    /// </summary>
    private int ParseHealAmount(ItemConfig itemConfig, PetData petData)
    {
        if (!string.IsNullOrEmpty(itemConfig.sideEffectArg))
        {
            if (int.TryParse(itemConfig.sideEffectArg, out int val) && val > 0)
                return val;
            if (int.TryParse(itemConfig.sideEffectArg, out int full) && full == -1)
                return petData.CurrentStatus.MaxHp - petData.CurrentStatus.Hp;
        }
        if (itemConfig.type == "heal_full")
            return petData.CurrentStatus.MaxHp - petData.CurrentStatus.Hp;
        return (int)itemConfig.effectValue;
    }

    private void OnPetBagItemOpenEvent(PetBagItemOpenEvent @event)
    {
        InitializeFilteredItems();
        LoadCurrentPageItems();
    }

    // 初始化过滤后的物品列表
    private void InitializeFilteredItems()
    {
        filteredItems.Clear();
        List<ItemData> itemStorage = archiveModel.CurrentData.ItemStorage;

        for (int i = 0; i < itemStorage.Count; i++)
        {
            if (itemStorage[i].Id == 1 || itemStorage[i].Id == 2)
            {
                continue;
            }
            ItemConfig itemConfig = itemModel.GetById(itemStorage[i].Id);
            if (itemModel.IsContainScope(itemConfig, "INPETBAG"))
                filteredItems.Add(itemStorage[i]);
        }

        // 计算总页数
        TotalPages = Mathf.Max(1, Mathf.CeilToInt(filteredItems.Count / (float)PageSize));
        UpdatePageInfo();
    }

    // 加载当前页的物品
    private void LoadCurrentPageItems()
    {
        // 回收所有当前活动的Cell
        ReturnAllCellsToPool();

        // 计算当前页的起始索引
        int startIndex = (CurrentPage - 1) * PageSize;
        int endIndex = Mathf.Min(startIndex + PageSize, filteredItems.Count);

        // 创建当前页的物品
        for (int i = startIndex; i < endIndex; i++)
        {
            CreateItem(filteredItems[i], i);
        }
    }

    private PetItemCell GetCellFromPool()
    {
        // 从对象池中获取可用的Cell
        foreach (var cell in itemCellPool)
        {
            if (!cell.gameObject.activeSelf)
            {
                cell.gameObject.SetActive(true);
                activeCells.Add(cell);
                return cell;
            }
        }

        // 如果对象池中没有可用的Cell，创建新的
        GameObject bagItem = Instantiate(ItemPrefab, ItemPanel.transform);
        PetItemCell bagItemCell = bagItem.GetComponent<PetItemCell>();
        itemCellPool.Add(bagItemCell);
        activeCells.Add(bagItemCell);
        return bagItemCell;
    }

    private void ReturnAllCellsToPool()
    {
        // 将所有活动的Cell返回到对象池
        foreach (var cell in activeCells)
        {
            cell.ResetCell();
            cell.gameObject.SetActive(false);
        }
        activeCells.Clear();
    }

    private void CreateItem(ItemData itemData, int index)
    {
        PetItemCell bagItemCell = GetCellFromPool();

        if (bagItemCell == null)
            return;

        ItemConfig itemConfig = itemModel.GetById(itemData.Id);
        Sprite sprite = ResourcesManager.Instance.Load<Sprite>(BasePath + itemData.Id.ToString());

        // 设置点击事件
        Button itemButton = bagItemCell.GetComponent<Button>();
        if (itemButton != null)
        {
            itemButton.onClick.RemoveAllListeners(); // 先移除之前的监听器
            itemButton.onClick.AddListener(() => OnItemClick(itemData));
        }

        bagItemCell.Initialize(itemData, sprite);
        string content = $"{itemConfig.description}\n当前拥有数量:<color=yellow>{itemData.Num}</color>";
        // 添加提示
        TooltipTrigger.AddTooltip(bagItemCell.gameObject, content, itemConfig.name ?? "无", Color.yellow, sprite, FunctionPanelLoader.Instance.transform.GetComponent<Canvas>());
    }

    // 物品点击事件处理
    private void OnItemClick(ItemData itemData)
    {
        ItemConfig itemConfig = itemModel.GetById(itemData.Id);

        if (petBagController != null)
        {
            int index = petBagController.GetSelectedIndex();
            PetData petData = petBagController.GetSelectedPet();
            Dialog2Manager.Instance.ShowPopupOnTransform(PetBagCanvas, "",
                $"你确定要为你的<color=red>{petData.Basic.Name}</color>使用<color=red>{itemConfig.name}</color>吗?",
                () =>
                {
                    this.SendCommand(new UseItemCommand(itemData.Id, index, false));
                });
        }
    }

    private void UpdatePageInfo()
    {
        PageText.text = $"{CurrentPage}/{TotalPages}";
        PreButton.interactable = CurrentPage > 1;
        NextButton.interactable = CurrentPage < TotalPages;
    }

    private void OnPrePage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            UpdatePageInfo();
            LoadCurrentPageItems();
        }
    }

    private void OnNextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            UpdatePageInfo();
            LoadCurrentPageItems();
        }
    }

    private void OnClose()
    {
        // 关闭时，将对象池中的Cell也销毁
        foreach (var cell in itemCellPool)
        {
            Destroy(cell.gameObject);
        }
        itemCellPool.Clear();
        activeCells.Clear();

        _UISystem.SetUICanvasGroup("PetItemPanel", false);
        // 刷新背包
        this.SendCommand<ChangePetBagCommand>();
    }
}