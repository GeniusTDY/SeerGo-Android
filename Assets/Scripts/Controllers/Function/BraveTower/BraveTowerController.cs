using SeerGoGame.Controller;
using SeerGoGame.Events;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using QFramework;
using Button = UnityEngine.UI.Button;

public class BraveTowerController : BaseController
{
    [SerializeField] private ScrollRect ScrollRect;
    [SerializeField] private RectTransform Content;
    [SerializeField] private GameObject FloorItemPrefab;
    [SerializeField] private GameObject Right;
    [SerializeField] private Button CloseButton;
    [SerializeField] private Button SelectButton;
    [SerializeField] private Button BeginButton;
    [SerializeField] private Image BeginButtonBg;
    [SerializeField] private TextMeshProUGUI CurrentFloorNum;

    private const string BaseBeginButtonBgPath = "Sprites/Function/BraveTower/BeginChallenge";
    private const string BaseContinueButtonBgPath = "Sprites/Function/BraveTower/ContinueChallenge";

    private const int FloorStep = 10;
    private const int DefaultSelectFloor = 1;

    private IArchiveModel _archiveModel;
    private IArchiveSystem _archiveSystem;
    private IMapLoadSystem _mapLoadSystem;
    private int _selectedFloorNum;

    /// <summary>勇者之塔配置数据</summary>
    private BraveTowerConfig _braveTowerConfig;

    void Start()
    {
        _archiveModel = this.GetModel<IArchiveModel>();
        _archiveSystem = this.GetSystem<IArchiveSystem>();
        _mapLoadSystem = this.GetSystem<IMapLoadSystem>();

        // 加载勇者之塔配置
        _braveTowerConfig = BraveTowerConfigLoader.LoadConfig();

        this.RegisterEvent<BraveTowerChangeEventOpenEvent>(OnBraveTowerOpenEvent)
            .UnRegisterWhenGameObjectDestroyed(gameObject);

        CloseButton.onClick.AddListener(OnCloseClick);
        SelectButton.onClick.AddListener(OnSelectFloorClick);
        BeginButton.onClick.AddListener(OnBeginClick);

        // 默认隐藏楼层选择区域
        Right.SetActive(false);

        // 从存档读取当前挑战层数
        RefreshCurrentFloor();
    }

    /// <summary>
    /// 刷新当前层数显示与BeginButton背景
    /// </summary>
    private void RefreshCurrentFloor()
    {
        int currentFloor = _archiveModel.CurrentData.YiteRogueData.CurrentBraveTowerFloor;
        _selectedFloorNum = currentFloor;
        CurrentFloorNum.text = currentFloor.ToString();
        UpdateBeginButtonBg();
    }

    /// <summary>
    /// 根据当前选中层数切换BeginButton背景图
    /// 层数1 → BeginChallenge, 层数>1 → ContinueChallenge
    /// </summary>
    private void UpdateBeginButtonBg()
    {
        string bgPath = _selectedFloorNum <= 1 ? BaseBeginButtonBgPath : BaseContinueButtonBgPath;
        var sprite = ResourcesManager.Instance.Load<Sprite>(bgPath);
        if (BeginButtonBg != null && sprite != null)
        {
            BeginButtonBg.sprite = sprite;
        }
    }

    private void OnCloseClick()
    {
        Right.SetActive(false);
        FunctionPanelLoader.Instance.ClosePanel("BraveTower/BraveTowerPanel");
    }

    private void OnBraveTowerOpenEvent(BraveTowerChangeEventOpenEvent @event)
    {
        RefreshCurrentFloor();
    }

    /// <summary>
    /// 点击选择楼层按钮：显示Right，基于HistoryBraveTowerFloor动态生成楼层列表
    /// floor <= history 可点击，floor > history 不可点击（灰色）
    /// </summary>
    private void OnSelectFloorClick()
    {
        Right.SetActive(true);

        // 清空已有item
        foreach (Transform child in Content)
        {
            Destroy(child.gameObject);
        }

        int historyFloor = _archiveModel.CurrentData.YiteRogueData.HistoryBraveTowerFloor;

        // 生成楼层item：1, 11, 21, ... 直到 floor < 配置最大层数
        int maxFloor = _braveTowerConfig != null ? _braveTowerConfig.FloorCount : 60;
        for (int floor = 1; floor < maxFloor; floor += FloorStep)
        {
            GameObject item = Instantiate(FloorItemPrefab, Content);
            var cell = item.GetComponent<FloorItemCell>();
            if (cell != null)
            {
                bool isClickable = (floor <= historyFloor);
                // 默认选中第1层
                bool isSelected = (floor == DefaultSelectFloor) && isClickable;
                cell.SetFloor(floor, isSelected, isClickable, OnFloorItemSelected);
            }
        }
    }

    /// <summary>
    /// 选中某楼层：刷新高亮 → 切换BeginButton背景 → 隐藏Right → 打印
    /// </summary>
    private void OnFloorItemSelected(int floorNum)
    {
        _selectedFloorNum = floorNum;
        CurrentFloorNum.text = floorNum.ToString();
        UpdateBeginButtonBg();

        // 刷新所有item的选中高亮状态
        foreach (Transform child in Content)
        {
            var cell = child.GetComponent<FloorItemCell>();
            if (cell != null)
            {
                cell.SetSelected(cell.FloorNumber == floorNum);
            }
        }

        Right.SetActive(false);

        SeerGoLogger.Log($"[勇者之塔] 选择楼层: {floorNum}");
    }

    private void OnBeginClick()
    {
        // 将选择的层数写入存档
        _archiveModel.CurrentData.YiteRogueData.CurrentBraveTowerFloor = _selectedFloorNum;
        _archiveSystem.SaveGame("autosave0", _archiveModel.CurrentData);

        SeerGoLogger.Log($"[勇者之塔] 开始挑战, 当前层数: {_selectedFloorNum}");
        OnCloseClick();

        // 加载勇者之塔地图
        _mapLoadSystem.RequestMapLoad(-50500);
    }
}
