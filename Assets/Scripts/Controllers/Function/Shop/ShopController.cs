using QFramework;
using SeerGoGame.Controller;
using SeerGoGame.Events;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Button = UnityEngine.UI.Button;

public class ShopController : BaseController
{
    [Header("UI References")]
    [SerializeField] private GameObject ItemPanel;
    [SerializeField] private Canvas FunctionCanvas;
    [SerializeField] private Button CloseButton;
    [SerializeField] private TextMeshProUGUI PageText;
    [SerializeField] private Button PreButton;
    [SerializeField] private Button NextButton;
    [Header("Item预制体")]
    [SerializeField] private GameObject ItemPrefab;

    private const int PageSize = 15;
    private int CurrentPage = 1;
    private int TotalPages = 1;

    private IItemModel itemModel;
    private IArchiveModel archiveModel;
    private Queue<ShopItemCell> itemPool = new Queue<ShopItemCell>(); // 对象池
    private List<ShopItemCell> activeItems = new List<ShopItemCell>(); // 当前活动的物品

    // 缓存排序后的物品列表，避免每次重新排序
    private List<ItemConfig> sortedShopItems;

    void Start()
    {
        this.RegisterEvent<ShopOpenEvent>(OnShopOpenEvent)
            .UnRegisterWhenGameObjectDestroyed(gameObject);
        itemModel = this.GetModel<IItemModel>();
        archiveModel = this.GetModel<IArchiveModel>();
        InitializePool(); // 初始化对象池
        PreButton.onClick.AddListener(OnPrePage);
        NextButton.onClick.AddListener(OnNextPage);
        CloseButton.onClick.AddListener(OnClose);
    }

    private void OnClose()
    {
        FunctionPanelLoader.Instance.ClosePanel("Shop/ShopPanel");
    }

    private void OnShopOpenEvent(ShopOpenEvent @event)
    {
        CurrentPage = 1; // 重新打开商店时重置到第一页
        CalculateTotalPages();
        LoadShopItemData();
        UpdatePageUI();
    }

    private void InitializePool()
    {
        // 预先创建PageSize个对象放入池中
        for (int i = 0; i < PageSize; i++)
        {
            CreatePoolItem();
        }
    }

    private void CreatePoolItem()
    {
        GameObject itemObject = Instantiate(ItemPrefab, ItemPanel.transform);
        ShopItemCell shopItemCell = itemObject.GetComponent<ShopItemCell>();
        shopItemCell.gameObject.SetActive(false);
        itemPool.Enqueue(shopItemCell);
    }

    private ShopItemCell GetPoolItem()
    {
        if (itemPool.Count == 0)
        {
            CreatePoolItem(); // 池为空时创建新对象
        }

        ShopItemCell item = itemPool.Dequeue();
        item.gameObject.SetActive(true);
        return item;
    }

    private void ReturnAllItemsToPool()
    {
        // 将所有活动物品重置并返回对象池
        foreach (var item in activeItems)
        {
            if (item != null)
            {
                item.ResetCell(); // 重置状态
                item.gameObject.SetActive(false);
                itemPool.Enqueue(item);
            }
        }
        activeItems.Clear();
    }

    private void CalculateTotalPages()
    {
        if (itemModel == null || itemModel.ItemConfigs == null)
        {
            TotalPages = 1;
            return;
        }

        // 获取排序后的物品列表
        GetSortedShopItems();

        // 计算总页数
        TotalPages = Mathf.CeilToInt((float)sortedShopItems.Count / PageSize);
        if (TotalPages < 1) TotalPages = 1;
    }

    /// <summary>
    /// 获取排序后的商店物品列表（按id升序）
    /// </summary>
    private void GetSortedShopItems()
    {
        if (sortedShopItems == null)
        {
            sortedShopItems = new List<ItemConfig>();
        }

        sortedShopItems.Clear();

        // 过滤出useScope包含INSHOP的物品，按id升序排列
        sortedShopItems = itemModel.ItemConfigs.Values
            .Where(item => itemModel.IsContainScope(item, "INSHOP"))
            .OrderBy(item => item.id)
            .ToList();
    }

    private void LoadShopItemData()
    {
        // 将所有活动物品返回对象池
        ReturnAllItemsToPool();

        if (itemModel == null || itemModel.ItemConfigs == null)
        {
            return;
        }

        // 确保有排序后的物品列表
        if (sortedShopItems == null || sortedShopItems.Count == 0)
        {
            GetSortedShopItems();
        }

        // 确保当前页不超过总页数
        if (CurrentPage > TotalPages && TotalPages > 0)
        {
            CurrentPage = TotalPages;
        }
        else if (TotalPages == 0)
        {
            CurrentPage = 1;
        }

        // 获取当前页的数据
        var currentPageItems = sortedShopItems
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        // 从对象池获取物品并初始化
        for (int i = 0; i < currentPageItems.Count; i++)
        {
            ShopItemCell shopItemCell = GetPoolItem();
            shopItemCell.Initialize(currentPageItems[i], OnItemClicked, archiveModel, FunctionPanelLoader.Instance.transform.GetComponent<Canvas>());
            activeItems.Add(shopItemCell);

            // 确保对象在Hierarchy中的顺序与数据顺序一致
            shopItemCell.transform.SetSiblingIndex(i);
        }
    }

    private void OnItemClicked(ItemConfig itemData)
    {
        //SeerGoLogger.Log($"点击了物品: {itemData.name}");
    }

    private void UpdatePageUI()
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
            LoadShopItemData();
            UpdatePageUI();
        }
    }

    private void OnNextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            LoadShopItemData();
            UpdatePageUI();
        }
    }

    // 可选：清理对象池
    private void OnDestroy()
    {
        foreach (var item in itemPool)
        {
            if (item != null && item.gameObject != null)
            {
                Destroy(item.gameObject);
            }
        }
        itemPool.Clear();

        foreach (var item in activeItems)
        {
            if (item != null && item.gameObject != null)
            {
                Destroy(item.gameObject);
            }
        }
        activeItems.Clear();
    }
}