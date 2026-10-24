using SeerGoGame.Controller;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FloorItemCell : BaseController
{
    [Header("UI References")]
    [SerializeField] private Image FloorBg;
    [SerializeField] private TextMeshProUGUI FloorNum;
    [SerializeField] private Button ItemButton;

    public int FloorNumber { get; private set; }
    private bool _isSelected = false;
    private bool _isClickable = true;
    private System.Action<int> _onSelectCallback;

    private static readonly Color SelectedColor = new Color(1f, 0.95f, 0.5f, 1f);   // 淡黄色
    private static readonly Color NormalColor = Color.white;
    private static readonly Color DisabledColor = new Color(0.6f, 0.6f, 0.6f, 1f);  // 灰色

    void Awake()
    {
        // 优先序列化字段，其次从自身获取，最后从子对象查找
        if (ItemButton == null)
            ItemButton = GetComponent<Button>();
        if (ItemButton == null)
            ItemButton = GetComponentInChildren<Button>();
        // 都没找到则动态添加
        if (ItemButton == null)
            ItemButton = gameObject.AddComponent<Button>();

        ItemButton.onClick.AddListener(OnItemClick);
    }

    /// <summary>
    /// 设置楼层信息
    /// </summary>
    /// <param name="floorNum">楼层号</param>
    /// <param name="isSelected">是否选中（淡黄色）</param>
    /// <param name="isClickable">是否可点击，不可点则显示灰色</param>
    /// <param name="onSelect">选中回调</param>
    public void SetFloor(int floorNum, bool isSelected, bool isClickable, System.Action<int> onSelect)
    {
        FloorNumber = floorNum;
        _onSelectCallback = onSelect;
        _isClickable = isClickable;
        FloorNum.text = floorNum.ToString();

        ItemButton.interactable = isClickable;

        if (isClickable)
            _isSelected = isSelected;
        else
            _isSelected = false;

        UpdateBgState();
    }

    /// <summary>
    /// 设置选中状态（仅可点击楼层生效）
    /// </summary>
    public void SetSelected(bool selected)
    {
        if (!_isClickable) return;
        _isSelected = selected;
        UpdateBgState();
    }

    /// <summary>
    /// 更新背景颜色：不可点击=灰 / 选中=淡黄 / 可点击未选中=白
    /// </summary>
    private void UpdateBgState()
    {
        if (FloorBg == null) return;

        if (!_isClickable)
            FloorBg.color = DisabledColor;
        else if (_isSelected)
            FloorBg.color = SelectedColor;
        else
            FloorBg.color = NormalColor;
    }

    /// <summary>
    /// 点击楼层Item
    /// </summary>
    private void OnItemClick()
    {
        if (!_isClickable) return;
        _onSelectCallback?.Invoke(FloorNumber);
    }
}
