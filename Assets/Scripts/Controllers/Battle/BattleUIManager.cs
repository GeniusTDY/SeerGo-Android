using SeerGoGame.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 战斗UI管理器 - 负责按钮、面板、遮罩的显示控制
/// </summary>
public class BattleUIManager
{
    public enum ButtonType
    {
        SwitchPets,
        Fight,
        Item,
        Escape
    }

    // UI引用
    private readonly Image switchPetsButtonImage;
    private readonly Image fightButtonImage;
    private readonly Image itemButtonImage;
    private readonly Image escapeButtonImage;

    private readonly CanvasGroup functionPanel;
    private readonly CanvasGroup skillPanel;
    private readonly CanvasGroup switchPetPanel;
    private readonly CanvasGroup fightPetIconPanel;
    private readonly CanvasGroup fightSkillPanel;
    private readonly CanvasGroup itemPanel;

    // 资源缓存
    private readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
    private static readonly string BasePath = ResourcePathBuilder.GetFightButtonImagePath();
    private static readonly Dictionary<string, string> PathCache = new Dictionary<string, string>();

    public BattleUIManager(
        Image switchPetsButtonImage,
        Image fightButtonImage,
        Image itemButtonImage,
        Image escapeButtonImage,
        CanvasGroup functionPanel,
        CanvasGroup skillPanel,
        CanvasGroup switchPetPanel,
        CanvasGroup fightPetIconPanel,
        CanvasGroup fightSkillPanel,
        CanvasGroup itemPanel)
    {
        this.switchPetsButtonImage = switchPetsButtonImage;
        this.fightButtonImage = fightButtonImage;
        this.itemButtonImage = itemButtonImage;
        this.escapeButtonImage = escapeButtonImage;
        this.functionPanel = functionPanel;
        this.skillPanel = skillPanel;
        this.switchPetPanel = switchPetPanel;
        this.fightPetIconPanel = fightPetIconPanel;
        this.fightSkillPanel = fightSkillPanel;
        this.itemPanel = itemPanel;
    }

    /// <summary>
    /// 初始化：预加载所有按钮精灵图
    /// </summary>
    public void Initialize()
    {
        PreloadAllButtonSprites();
        SetButtonSelected(fightButtonImage, ButtonType.Fight);
        ShowPanel(fightSkillPanel, fightPetIconPanel);
    }

    #region 遮罩控制
    /// <summary>
    /// 设置遮罩启用/禁用
    /// </summary>
    public void SetMask(bool isEnabled)
    {
        SetCanvasGroupAlpha(functionPanel, isEnabled);
        SetCanvasGroupAlpha(skillPanel, isEnabled);
    }

    private void SetCanvasGroupAlpha(CanvasGroup canvasGroup, bool isEnabled)
    {
        canvasGroup.alpha = isEnabled ? 1 : 0.5f;
    }
    #endregion

    #region 面板控制
    /// <summary>
    /// 显示指定面板（自动隐藏其他面板）
    /// </summary>
    public void ShowPanel(CanvasGroup primaryPanel, CanvasGroup secondaryPanel = null)
    {
        ResetAllPanels();
        SetCanvasGroupProperties(primaryPanel, true);

        if (secondaryPanel != null)
        {
            SetCanvasGroupProperties(secondaryPanel, true);
        }
    }

    private void ResetAllPanels()
    {
        SetCanvasGroupProperties(switchPetPanel, false);
        SetCanvasGroupProperties(fightPetIconPanel, false);
        SetCanvasGroupProperties(fightSkillPanel, false);
        SetCanvasGroupProperties(itemPanel, false);

    }

    private void SetCanvasGroupProperties(CanvasGroup canvasGroup, bool isEnabled)
    {
        canvasGroup.alpha = isEnabled ? 1 : 0;
        canvasGroup.interactable = isEnabled;
        canvasGroup.blocksRaycasts = isEnabled;
    }
    #endregion

    #region 按钮控制
    /// <summary>
    /// 设置按钮为选中状态
    /// </summary>
    public void SetButtonSelected(ButtonType buttonType)
    {
        Image targetImage = GetButtonImage(buttonType);
        SetButtonSelected(targetImage, buttonType);
    }

    private void SetButtonSelected(Image selectedButtonImage, ButtonType buttonType)
    {
        ResetAllButtons();
        string selectedPath = GetSelectedPath(buttonType);
        Sprite selectedSprite = LoadSprite(selectedPath);

        if (selectedSprite != null)
        {
            selectedButtonImage.sprite = selectedSprite;
        }
    }

    private void ResetAllButtons()
    {
        switchPetsButtonImage.sprite = LoadSprite(BuildPath("petButton"));
        fightButtonImage.sprite = LoadSprite(BuildPath("fightButton"));
        itemButtonImage.sprite = LoadSprite(BuildPath("itemButton"));
        escapeButtonImage.sprite = LoadSprite(BuildPath("escapeButton"));
    }

    private Image GetButtonImage(ButtonType buttonType)
    {
        return buttonType switch
        {
            ButtonType.SwitchPets => switchPetsButtonImage,
            ButtonType.Fight => fightButtonImage,
            ButtonType.Item => itemButtonImage,
            ButtonType.Escape => escapeButtonImage,
            _ => fightButtonImage
        };
    }
    #endregion

    #region 资源加载
    private void PreloadAllButtonSprites()
    {
        var buttonPaths = new[]
        {
            ("fightButton", "fightSelected"),
            ("itemButton", "itemSelected"),
            ("escapeButton", "escapeSelected"),
            ("petButton", "petSelected")
        };

        foreach (var (normal, selected) in buttonPaths)
        {
            LoadSprite(BuildPath(normal));
            LoadSprite(BuildPath(selected));
        }
    }

    private static string BuildPath(string fileName)
    {
        if (!PathCache.TryGetValue(fileName, out string fullPath))
        {
            fullPath = $"{BasePath}/{fileName}";
            PathCache[fileName] = fullPath;
        }
        return fullPath;
    }

    private Sprite LoadSprite(string path)
    {
        if (!spriteCache.TryGetValue(path, out Sprite sprite))
        {
            sprite = Resources.Load<Sprite>(path);
            if (sprite != null) spriteCache[path] = sprite;
        }
        return sprite;
    }

    private string GetSelectedPath(ButtonType buttonType)
    {
        return buttonType switch
        {
            ButtonType.SwitchPets => BuildPath("petSelected"),
            ButtonType.Fight => BuildPath("fightSelected"),
            ButtonType.Item => BuildPath("itemSelected"),
            ButtonType.Escape => BuildPath("escapeSelected"),
            _ => BuildPath("fightSelected")
        };
    }
    #endregion
}
