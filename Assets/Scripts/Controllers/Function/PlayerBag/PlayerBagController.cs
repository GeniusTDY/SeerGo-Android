using PixeLadder.SimpleTooltip;
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

public class PlayerBagController : BaseController
{
    [Header("左半背包")]
    [SerializeField] private GameObject ItemPanel;
    [SerializeField] private Canvas FunctionCanvas;
    [SerializeField] private TextMeshProUGUI PlayerName;
    [SerializeField] private TextMeshProUGUI GoldBeanNums;
    [SerializeField] private TextMeshProUGUI SeerBeanNums;
    [SerializeField] private Button SettingButton;
    [SerializeField] private Button CloseButton;
    [SerializeField] private Image SeerBeanIcon;
    [SerializeField] private Image GoldBeanIcon;

    [SerializeField] private Button ConfirmButton;
    [SerializeField] private TMP_InputField modifyName;


    [Header("右半背包")]
    [SerializeField] private TextMeshProUGUI PageText;
    [SerializeField] private Button PreButton;
    [SerializeField] private Button NextButton;

    [Header("Item预制体")]
    [SerializeField] private GameObject ItemPrefab;

    private const int PageSize = 9;
    private int CurrentPage = 1;
    private int TotalPages = 1;

    private IArchiveModel archiveModel;
    private IItemModel itemModel;

    private List<ItemData> filteredItems = new List<ItemData>(); // 过滤后的物品列表
    private List<PlayerBagItemCell> itemCellPool = new List<PlayerBagItemCell>(); // 对象池
    private List<PlayerBagItemCell> activeCells = new List<PlayerBagItemCell>(); // 当前活动的Cell

    private const string BasePath = "Sprites/Item/";

    void Start()
    {
        this.RegisterEvent<PlayerBagOpenEvent>(OnPlayerBagOpenEvent)
            .UnRegisterWhenGameObjectDestroyed(gameObject);

        archiveModel = this.GetModel<IArchiveModel>();
        itemModel = this.GetModel<IItemModel>();

        CloseButton.onClick.AddListener(OnClose);
        PreButton.onClick.AddListener(OnPrePage);
        NextButton.onClick.AddListener(OnNextPage);

        SettingButton.onClick.AddListener(OnSettingClick);
        ConfirmButton.onClick.AddListener(OnConfirmClick);

        // 限制输入字数：1~5个字
        modifyName.characterLimit = 5;

        // 默认隐藏改名相关UI
        SetRenameUIActive(false);

        InitTooltips();

        InitializeFilteredItems();
    }

    private void InitTooltips()
    {
        TooltipTrigger.AddTooltip(
              SeerBeanIcon.gameObject,
              null,
              "赛尔豆",
              Color.yellow,
              null,
              FunctionPanelLoader.Instance.transform.GetComponent<Canvas>()
          );

        TooltipTrigger.AddTooltip(
              GoldBeanIcon.gameObject,
              null,
              "金豆",
              Color.yellow,
              null,
              FunctionPanelLoader.Instance.transform.GetComponent<Canvas>()
          );
    }

    private void OnPlayerBagOpenEvent(PlayerBagOpenEvent @event)
    {
        InitializeFilteredItems();
        LoadCurrentPageItems();
    }

    // 初始化过滤后的物品列表
    private void InitializeFilteredItems()
    {
        PlayerName.text = archiveModel.CurrentData.Nickname;
        filteredItems.Clear();
        List<ItemData> itemStorage = archiveModel.CurrentData.ItemStorage;

        for (int i = 0; i < itemStorage.Count; i++)
        {
            if (itemStorage[i].Id == 1 || itemStorage[i].Id == 2)
            {
                (itemStorage[i].Id == 1 ? SeerBeanNums : GoldBeanNums).text = itemStorage[i].Num.ToString();
                continue;
            }
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

    private PlayerBagItemCell GetCellFromPool()
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
        PlayerBagItemCell bagItemCell = bagItem.GetComponent<PlayerBagItemCell>();
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
        PlayerBagItemCell bagItemCell = GetCellFromPool();

        if (bagItemCell == null)
            return;

        ItemConfig itemConfig = itemModel.GetById(itemData.Id);
        Sprite sprite = ResourcesManager.Instance.Load<Sprite>(BasePath + itemData.Id.ToString());

        bagItemCell.Initialize(itemData, archiveModel, sprite);
        string content = $"{itemConfig.description}\n当前拥有数量:<color=yellow>{itemData.Num}</color>";
        // 添加提示
        TooltipTrigger.AddTooltip(bagItemCell.gameObject, content, itemConfig.name ?? "无", Color.yellow, sprite, FunctionPanelLoader.Instance.transform.GetComponent<Canvas>());
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
        // 关闭背包时，重置改名UI状态
        SetRenameUIActive(false);

        // 关闭时，将对象池中的Cell也销毁
        foreach (var cell in itemCellPool)
        {
            Destroy(cell.gameObject);
        }
        itemCellPool.Clear();
        activeCells.Clear();

        FunctionPanelLoader.Instance.ClosePanel("PlayerBag/PlayerBagPanel");
    }

    /// <summary>
    /// 点击设置按钮：显示改名相关UI，隐藏设置按钮
    /// </summary>
    private void OnSettingClick()
    {
        SettingButton.gameObject.SetActive(false);
        SetRenameUIActive(true);
        modifyName.text = archiveModel.CurrentData.Nickname;
    }

    /// <summary>
    /// 点击确认改名按钮：验证输入并保存
    /// </summary>
    private void OnConfirmClick()
    {
        string newName = modifyName.text.Trim();

        // 校验字数：最少1个字，最多5个字
        if (string.IsNullOrEmpty(newName) || newName.Length < 1)
        {
            SeerGoLogger.LogWarning("[PlayerBag] 玩家名字不能为空");
            return;
        }
        if (newName.Length > 5)
        {
            SeerGoLogger.LogWarning("[PlayerBag] 玩家名字最多5个字");
            return;
        }

        // 更新存档数据
        archiveModel.CurrentData.Nickname = newName;
        // 更新UI显示
        PlayerName.text = newName;
        // 保存存档
        this.SendCommand(new SaveArchiveDataCommand(archiveModel.CurrentData));

        // 隐藏改名UI，恢复设置按钮
        SetRenameUIActive(false);

        SeerGoLogger.Log($"[PlayerBag] 玩家名字已修改为: {newName}");
    }

    /// <summary>
    /// 设置改名相关UI的显示/隐藏
    /// </summary>
    private void SetRenameUIActive(bool active)
    {
        ConfirmButton.gameObject.SetActive(active);
        modifyName.gameObject.SetActive(active);
        if (!active)
        {
            SettingButton.gameObject.SetActive(true);
        }
    }
}