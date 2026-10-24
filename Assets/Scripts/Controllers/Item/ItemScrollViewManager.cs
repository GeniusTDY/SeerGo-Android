using QFramework;
using SeerGoGame.Commands;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ItemScrollViewManager : BaseBattleController
{
    [Header("UI References")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private GridLayoutGroup gridLayoutGroup;
    [SerializeField] private Slider slider;
    [SerializeField] private Canvas uiCanvasCache;

    [Header("Prefab References")]
    [SerializeField] private GameObject itemPrefab;

    [Header("Grid Settings")]
    [SerializeField] private int maxItemsPerRow = 4;
    [SerializeField] private Vector2 cellSize = new Vector2(52, 52);
    [SerializeField] private Vector2 spacing = new Vector2(20, 10);
    [SerializeField] private RectOffset padding; // 移除初始化，在Awake中初始化

    [Header("ItemTotalCount Data")]
    [SerializeField] private int ItemTotalCount = 20;

    [Header("Object Pool Settings")]
    [SerializeField] private int initialPoolSize = 20;
    [SerializeField] private int maxPoolSize = 100;
    [SerializeField] private Transform poolContainer; // 可选：对象池容器

    private List<ItemData> itemsData = new List<ItemData>();
    private List<ItemCell> activeItems = new List<ItemCell>();
    private Queue<ItemCell> itemPool = new Queue<ItemCell>();

    // 当前选择的宠物ID
    private int _currentSelectedPetId;

    private void Awake()
    {
        // 在Awake中初始化padding
        if (padding == null)
        {
            padding = new RectOffset(10, 10, 10, 10);
        }
    }

    private void Start()
    {
        InitializeGridSettings();
        InitializeObjectPool();
    }

    private void InitializeGridSettings()
    {
        if (gridLayoutGroup != null)
        {
            gridLayoutGroup.cellSize = cellSize;
            gridLayoutGroup.spacing = spacing;

            // 设置padding
            if (padding != null)
            {
                gridLayoutGroup.padding = padding;
            }
            else
            {
                // 如果没有设置padding，使用默认值
                gridLayoutGroup.padding = new RectOffset(10, 10, 10, 10);
            }

            gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayoutGroup.constraintCount = maxItemsPerRow;
        }

        // 监听Slider变化
        slider.onValueChanged.AddListener(value => {
            scrollRect.verticalNormalizedPosition = 1f - value;
        });

        // 监听ScrollRect变化
        scrollRect.onValueChanged.AddListener(value => {
            slider.value = 1f - value.y;
        });
    }

    private void InitializeObjectPool()
    {
        if (itemPrefab == null)
        {
            SeerGoLogger.LogError("Item Prefab 未设置!");
            return;
        }

        // 创建对象池容器（如果未设置）
        if (poolContainer == null)
        {
            GameObject poolObj = new GameObject("ItemPool");
            poolContainer = poolObj.transform;
            poolContainer.SetParent(transform);
            poolContainer.localPosition = Vector3.zero;
        }

        // 预创建对象池
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreatePooledItem();
        }
    }

    private ItemCell CreatePooledItem()
    {
        if (itemPrefab == null) return null;

        GameObject itemObj = Instantiate(itemPrefab, poolContainer);
        ItemCell itemComponent = itemObj.GetComponent<ItemCell>();

        if (itemComponent != null)
        {
            itemObj.SetActive(false);
            itemPool.Enqueue(itemComponent);
        }
        else
        {
            SeerGoLogger.LogError($"ItemPrefab 上没有找到 ScrollViewItem 组件: {itemPrefab.name}");
            Destroy(itemObj);
        }

        return itemComponent;
    }

    private ItemCell GetItemFromPool()
    {
        // 如果对象池为空且未达到最大限制，创建新对象
        if (itemPool.Count == 0)
        {
            if (activeItems.Count + itemPool.Count < maxPoolSize)
            {
                CreatePooledItem();
            }
            else
            {
                SeerGoLogger.LogWarning("对象池已达最大限制，无法创建新对象");
                return null;
            }
        }

        // 从对象池中取出对象
        ItemCell item = itemPool.Dequeue();
        item.gameObject.SetActive(true);
        item.transform.SetParent(content);
        item.transform.localScale = Vector3.one;
        item.transform.localPosition = Vector3.zero;

        return item;
    }

    private void ReturnItemToPool(ItemCell item)
    {
        if (item == null) return;

        item.gameObject.SetActive(false);
        item.transform.SetParent(poolContainer);
        itemPool.Enqueue(item);
    }

    public void PopulateScrollView()
    {
        // 回收所有活跃的Item
        foreach (var item in activeItems)
        {
            if (item != null)
            {
                ReturnItemToPool(item);
            }
        }
        activeItems.Clear();

        // 检查prefab是否有效
        if (itemPrefab == null)
        {
            SeerGoLogger.LogError("Item Prefab 未设置!");
            return;
        }

        // 检查content是否有效
        if (content == null)
        {
            SeerGoLogger.LogError("Content 未设置!");
            return;
        }

        // 从对象池获取并初始化新的Item
        foreach (var itemData in itemsData)
        {
            ItemCell itemComponent = GetItemFromPool();

            if (itemComponent != null)
            {
                itemComponent.Initialize(itemData, OnItemClicked, uiCanvasCache);
                activeItems.Add(itemComponent);
            }
            else
            {
                SeerGoLogger.LogWarning("无法从对象池获取ItemCell，可能已达最大限制");
                break;
            }
        }

        // 更新Content大小
        UpdateContentSize();
    }

    private void UpdateContentSize()
    {
        if (content == null) return;

        // 确保gridLayoutGroup不为null
        if (gridLayoutGroup == null)
        {
            SeerGoLogger.LogError("GridLayoutGroup 未设置!");
            return;
        }

        // 计算需要的行数
        int totalRows = Mathf.CeilToInt((float)itemsData.Count / maxItemsPerRow);

        // 获取当前的padding值
        RectOffset currentPadding = gridLayoutGroup.padding;
        int topPadding = currentPadding != null ? currentPadding.top : 0;
        int bottomPadding = currentPadding != null ? currentPadding.bottom : 0;

        // 计算Content的高度
        float contentHeight = totalRows * cellSize.y +
                             Mathf.Max(0, totalRows - 1) * spacing.y +
                             topPadding + bottomPadding;

        // 设置Content大小
        content.sizeDelta = new Vector2(content.sizeDelta.x, contentHeight);
    }

    private void OnItemClicked(ItemData itemData)
    {
        SeerGoLogger.Log($"点击了物品: {itemData.Num}, ID: {itemData.Id}");

        // 发送战斗使用道具命令
        this.SendCommand(new UseItemCommand(itemData.Id, _currentSelectedPetId, true));
        ShowItemInfo(itemData);
    }

    private void ShowItemInfo(ItemData itemData)
    {
        // 实现显示物品信息的逻辑
    }

    private void OnScrollbarValueChanged(float value)
    {
        // 可选的滑动条值变化处理
    }

    public void ClearAllItems()
    {
        // 回收所有活跃的Item
        foreach (var item in activeItems)
        {
            if (item != null)
            {
                ReturnItemToPool(item);
            }
        }
        activeItems.Clear();
        itemsData.Clear();

        UpdateContentSize();
    }

    public void AddItem(ItemData newItem)
    {
        if (newItem == null) return;

        itemsData.Add(newItem);

        if (itemPrefab == null || content == null)
        {
            SeerGoLogger.LogError("ItemPrefab 或 Content 未设置!");
            return;
        }

        // 从对象池获取ItemCell
        ItemCell itemComponent = GetItemFromPool();

        if (itemComponent != null)
        {
            itemComponent.Initialize(newItem, OnItemClicked, uiCanvasCache);
            activeItems.Add(itemComponent);
        }
        else
        {
            SeerGoLogger.LogWarning("无法从对象池获取ItemCell，可能已达最大限制");
        }

        UpdateContentSize();
    }

    public void RemoveItem(int itemId)
    {
        for (int i = itemsData.Count - 1; i >= 0; i--)
        {
            if (itemsData[i].Id == itemId)
            {
                if (i < activeItems.Count && activeItems[i] != null)
                {
                    ReturnItemToPool(activeItems[i]);
                }

                itemsData.RemoveAt(i);
                activeItems.RemoveAt(i);

                UpdateContentSize();
                break;
            }
        }
    }

    // 重新加载数据
    public void ReloadData(List<ItemData> newData, int currentPetIndex)
    {
        _currentSelectedPetId = currentPetIndex;
        itemsData = new List<ItemData>(newData);
        ItemTotalCount = newData.Count;
        PopulateScrollView();
    }

    // 获取所有物品数据
    public List<ItemData> GetAllItems()
    {
        return new List<ItemData>(itemsData);
    }

    // 获取特定物品
    public ItemData GetItemById(int itemId)
    {
        return itemsData.Find(item => item.Id == itemId);
    }

    // 对象池统计信息
    public int GetActiveItemCount() => activeItems.Count;
    public int GetPooledItemCount() => itemPool.Count;
    public int GetTotalItemCount() => activeItems.Count + itemPool.Count;

    private void OnDestroy()
    {
        // 清理对象池
        while (itemPool.Count > 0)
        {
            ItemCell item = itemPool.Dequeue();
            if (item != null && item.gameObject != null)
            {
                Destroy(item.gameObject);
            }
        }
        itemPool.Clear();

        // 清理活跃对象
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