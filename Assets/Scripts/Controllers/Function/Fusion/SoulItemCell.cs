using PixeLadder.SimpleTooltip;
using SeerGoGame.Controller;
using SeerGoGame.Models;
using SeerGoGame.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

public class SoulItemCell : BaseController
{
    [Header("UI References")]
    [SerializeField] private Image ItemIcon;
    [SerializeField] private TextMeshProUGUI ItemCount;
    [SerializeField] private Button SoulBgButton;

    private Image bgImage;

    public int ItemId { get; private set; }
    public int ItemNum { get; private set; }
    public ItemConfig ItemConfig { get; private set; }

    private System.Action<int> onSelectCallback;
    private bool isSelected = false;

    private string petName;
    private string featureName;

    private const string BasePath = "Sprites/Item/";
    private const string DefaultSoulItemBgPath = "Sprites/Function/Fusion/SoulForming/ItemBg";
    private static readonly Color SelectedColor = new Color(1f, 0.95f, 0.5f, 1f); // 淡黄色

    /// <summary>
    /// 初始化珠子物品信息
    /// </summary>
    public void Initialize(ItemConfig config, int count, System.Action<int> callback = null)
    {
        InitializeWithFeature(config, "", "", false, callback);
    }

    /// <summary>
    /// 初始化珠子物品信息（带特性名称）
    /// </summary>
    public void InitializeWithFeature(ItemConfig config, string feature, System.Action<int> callback = null)
    {
        InitializeWithFeature(config, "", feature, false, callback);
    }

    /// <summary>
    /// 初始化珠子物品信息（带宠物名称和特性名称）
    /// </summary>
    /// <param name="config">物品配置</param>
    /// <param name="petName">宠物名称</param>
    /// <param name="featureName">特性名称</param>
    /// <param name="isSelected">是否选中</param>
    /// <param name="callback">选中回调</param>
    public void InitializeWithFeature(ItemConfig config, string pet, string feature, bool selected, System.Action<int> callback = null)
    {
        ItemConfig = config;
        ItemId = config.id;
        ItemNum = 1;
        petName = pet;
        featureName = feature;
        onSelectCallback = callback;
        isSelected = selected;

        // 自动获取自身对象上的 Image 组件作为背景图
        bgImage = GetComponent<Image>();
        if (bgImage != null)
            SeerGoLogger.Log($"[SoulItemCell] 获取到背景Image: {bgImage.name}");
        else
            SeerGoLogger.LogError($"[SoulItemCell] 未找到背景Image组件!");

        // 设置物品图标
        if (ItemIcon != null)
        {
            var sprite = ResourcesManager.Instance.Load<Sprite>($"{BasePath}{config.id}");
            if (sprite != null)
                ItemIcon.sprite = sprite;
        }

        // 设置名称显示（宠物名称 + 特性）

        if (ItemCount != null)
            ItemCount.text = "";

        // 设置选中状态
        UpdateBgState();

        // 添加Tooltip（先显示特性，后显示宠物名称）
        AddTooltip();

        // 添加按钮点击事件
        if (SoulBgButton != null)
        {
            SoulBgButton.onClick.RemoveAllListeners();
            SoulBgButton.onClick.AddListener(OnItemClick);
        }
    }

    /// <summary>
    /// 设置选中状态
    /// </summary>
    public void SetSelected(bool selected)
    {
        if (isSelected == selected)
            return;

        isSelected = selected;
        UpdateBgState();
        SeerGoLogger.Log($"[SoulItemCell] SetSelected: ID={ItemId}, Selected={isSelected}");
    }

    /// <summary>
    /// 更新背景状态
    /// </summary>
    private void UpdateBgState()
    {
        if (bgImage != null)
        {
            if (isSelected)
            {
                // 选中状态：淡黄色
                bgImage.color = SelectedColor;
                SeerGoLogger.Log($"[SoulItemCell] 设置选中背景色: {SelectedColor}");
            }
            else
            {
                // 默认状态：加载默认背景图
                var defaultSprite = ResourcesManager.Instance.Load<Sprite>(DefaultSoulItemBgPath);
                if (defaultSprite != null)
                {
                    bgImage.sprite = defaultSprite;
                    bgImage.color = Color.white;
                }
                else
                {
                    bgImage.color = Color.white;
                }
            }
        }
    }

    /// <summary>
    /// 添加Tooltip提示（先显示特性，后显示宠物名称）
    /// </summary>
    private void AddTooltip()
    {
        if (ItemConfig == null) return;

        // 格式：特性名称 + 宠物名称 + 物品描述
        string tooltipText = "";
        tooltipText += ItemConfig.description;

        if (!string.IsNullOrEmpty(petName))
            tooltipText += $"\n精灵: {petName}\n";
        if (!string.IsNullOrEmpty(featureName))
            tooltipText += $"特性: {featureName}\n";

        // 添加新的Tooltip
        TooltipTrigger.AddTooltip(
            gameObject,
            null,
            tooltipText,
            Color.yellow,
            null,
            FunctionPanelLoader.Instance.GetCanvas()
        );
    }

    /// <summary>
    /// 点击物品
    /// </summary>
    private void OnItemClick()
    {
        SeerGoLogger.Log($"[SoulItemCell] 点击珠子: ID={ItemId}, 名称={ItemConfig?.name}, Pet={petName}, Feature={featureName}");
        onSelectCallback?.Invoke(ItemId);
    }
}
