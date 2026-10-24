using PixeLadder.SimpleTooltip;
using QFramework;
using SeerGoGame.Controller;
using SeerGoGame.Helper;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

public class ScrollViewController : BaseController
{
    [Header("UI References")]
    public ScrollRect scrollRect;
    public RectTransform content;
    public GameObject galaxyItemPrefab;
    public GameObject planetItemPrefab;
    public Button backButton;
    public Canvas WorldMapCanvas;

    private IWorldMapModel worldMapModel;
    private IMapModel mapModel;
    private IMapLoadSystem mapLoadSystem;
    private IUISystem UISystem;


    private void Start()
    {
        // 初始化世界地图数据
        worldMapModel = this.GetModel<IWorldMapModel>();
        mapModel = this.GetModel<IMapModel>();
        mapLoadSystem = this.GetSystem<IMapLoadSystem>();
        UISystem = this.GetSystem<IUISystem>();
        // 返回默认不显示，星球页面显示
        SetBackButtonEnabled(false);
        backButton.onClick.AddListener(OnBackButtonClicked);
        LoadGalaxyData();
        // 默认关闭世界地图
        UISystem.SetUICanvasGroup("WorldMapCanvas", false);
    }

    private void SetBackButtonEnabled(bool isEnabled)
    {
        backButton.enabled = isEnabled;
        backButton.image.enabled = isEnabled;
    }

    // 返回星系页面
    private void OnBackButtonClicked()
    {
        // 加载星系数据
        SetBackButtonEnabled(false);
        LoadGalaxyData();
        // 清除字典
        GameObjectDictionary.ClearDictionary();
    }

    private void LoadGalaxyData()
    {
        // 清空现有内容
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
        // 创建每个Galaxy的UI项
        for (int i = 0; i < worldMapModel.GalaxyList.Count; i++)
        {
            CreateGalaxyItem(worldMapModel.GalaxyList[i], i);
        }
    }

    private void CreateGalaxyItem(GalaxyData galaxy, int index)
    {
        GameObject galaxyItem = Instantiate(galaxyItemPrefab, content);

        // 设置Galaxy项UI
        GalaxyItemUI galaxyUI = galaxyItem.GetComponent<GalaxyItemUI>();
        if (galaxyUI != null)
        {
            galaxyUI.SetGalaxyData(galaxy);
        }
        galaxyItem.AddComponent<AutoScaleAnim>();
        // 添加提示
        TooltipTrigger.AddTooltip(galaxyItem, null, galaxy.Name ?? "无", Color.yellow, null, WorldMapCanvas);

        //添加点击事件
        Button galaxyButton = galaxyItem.GetComponentInChildren<Button>();
        if (galaxyButton != null)
        {
            galaxyButton.onClick.AddListener(() => OnGalaxySelected(galaxy));
        }
    }

    // 星系点击事件
    public void OnGalaxySelected(GalaxyData galaxy)
    {
        // 点击星球展示列表
        // 清空现有内容
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // 创建每个Planet的UI项
        for (int i = 0; i < galaxy.Planets.Count; i++)
        {
            CreatePlanetItem(galaxy.Planets[i], i);
        }

        // 显示返回按钮
        SetBackButtonEnabled(true);
        // 通过当前地图id显示头像
        PlayerAvatarPosHelper.Instance.ChangePlanetAvatarById(mapModel.CurrentMap.Category, true);
    }

    // 创建星球
    private void CreatePlanetItem(PlanetData planetData, int index)
    {
        GameObject planetItem = Instantiate(planetItemPrefab, content);

        // 设置Galaxy项UI
        PlanetItemUI planetItemUI = planetItem.GetComponent<PlanetItemUI>();
        if (planetItemUI != null)
        {
            planetItemUI.SetPlanetData(planetData);
        }

        // 添加提示
        TooltipTrigger.AddTooltip(planetItem, null, planetData.Name ?? "无", Color.yellow, null, WorldMapCanvas);
        planetItem.AddComponent<AutoScaleAnim>();
        //添加点击事件
        Button galaxyButton = planetItem.GetComponentInChildren<Button>();
        if (galaxyButton != null)
        {
            galaxyButton.onClick.AddListener(() => OnPlanetSelected(planetData));
        }
        // 添加地图头像id
        GameObjectDictionary.RegisterGameObject(planetData.Id, planetItem);
    }

    private void OnPlanetSelected(PlanetData planetData)
    {
        //   取消上一个地图id显示头像
        PlayerAvatarPosHelper.Instance.ChangePlanetAvatarById(mapModel.CurrentMap.Category, false);

        // 跳转地图
        mapLoadSystem.RequestMapLoad(planetData.Id);
        // 关闭世界地图
        UISystem.SetUICanvasGroup("WorldMapCanvas", false);
    }
}