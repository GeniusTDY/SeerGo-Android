using QFramework;
using SeerGoGame.Commands;
using SeerGoGame.Controller;
using SeerGoGame.Interfaces;
using SeerGoGame.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/**
 * UI点击事件
 */
public class UICanvasController : BaseController
{
    private GraphicRaycaster raycaster;
    private IUISystem UISystem;
    private IMapModel mapModel;
    private IMapLoadSystem mapSystem;
    [Header("UI 按钮引用")]
    [SerializeField] private Button Guide;
    [SerializeField] private Button Mail;
    [SerializeField] private Button MiniMap;
    [SerializeField] private Button Home;
    [SerializeField] private Button PetsBag;
    [SerializeField] private Button PlayerBag;
    [SerializeField] private Button Battery;
    [SerializeField] private Button RemainingUses;

    void Start()
    {
        // 获取Canvas上的GraphicRaycaster组件
        raycaster = GetComponent<GraphicRaycaster>();
        if (raycaster == null)
        {
            SeerGoLogger.LogError("在Canvas上未找到GraphicRaycaster组件！", this);
        }
        UISystem = this.GetSystem<IUISystem>();
        mapModel = this.GetModel<IMapModel>();
        mapSystem = this.GetSystem<IMapLoadSystem>();
        // 设置按钮点击事件
        SetupButtonEvents();
    }

    private void SetupButtonEvents()
    {
        if (Guide != null)
            Guide.onClick.AddListener(OnGuideClicked);

        if (Mail != null)
            Mail.onClick.AddListener(OnMailClicked);

        if (MiniMap != null)
            MiniMap.onClick.AddListener(OnMiniMapClicked);

        if (Home != null)
            Home.onClick.AddListener(OnHomeClicked);

        if (PetsBag != null)
            PetsBag.onClick.AddListener(OnPetsBagClicked);

        if (PlayerBag != null)
            PlayerBag.onClick.AddListener(OnPlayerBagClicked);

        if (Battery != null)
            Battery.onClick.AddListener(OnBatteryClicked);

        if (RemainingUses != null)
            RemainingUses.onClick.AddListener(OnRemainingUsesClicked);
    }

    private void OnRemainingUsesClicked()
    {
        FunctionPanelLoader.Instance.LoadPanel("ItemRemainingUses/ItemRemainingUsesPanel", (panel) =>
        {
            if (panel != null)
            {
                this.SendCommand(new ChangeRemainingUsesCommand());
            }
        });
    }

    private void OnBatteryClicked()
    {
        FunctionPanelLoader.Instance.LoadPanel("Setting/SettingPanel", (panel) =>
        {
            if (panel != null)
            {
                this.SendCommand(new ChangeSettingCommand());
            }
        });
    }

    private void OnPlayerBagClicked()
    {
        FunctionPanelLoader.Instance.LoadPanel("PlayerBag/PlayerBagPanel", (panel) =>
        {
            if (panel != null)
            {
                this.SendCommand(new ChangePlayerBagCommand());
            }
        });
    }

    private void OnPetsBagClicked()
    {
        UISystem.SetUICanvasGroup("PetBagCanvas", true);
        this.SendCommand<ChangePetBagCommand>();
    }

    private void OnHomeClicked()
    {
        mapSystem.RequestMapLoad(-40000);
    }

    private void OnMiniMapClicked()
    {
        UISystem.SetUICanvasGroup("WorldMapCanvas", true);
        // 通过当前地图分类id显示头像
        PlayerAvatarPosHelper.Instance.ChangePlanetAvatarById(mapModel.CurrentMap.Category, true);
    }

    private void OnMailClicked()
    {
       
    }

    private void OnGuideClicked()
    {
       
    }

    public bool IsPointerOverUIObject(int fingerID)
    {
        return EventSystem.current.IsPointerOverGameObject(fingerID);
    }

}
