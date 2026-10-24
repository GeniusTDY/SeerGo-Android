using QFramework;
using SeerGoGame.Controller;
using SeerGoGame.Interfaces;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/**
 * UI点击事件
 */
public class WorldMapCanvasController : BaseController
{
    private GraphicRaycaster raycaster;
    private IUISystem UISystem;
    private IMapLoadSystem mapSystem;
    private IMapModel mapModel;

    // 映射UI名称到地图ID
    private readonly Dictionary<string, int> uiToMapId = new Dictionary<string, int>
    {
        {"Captain", -50004},     // 船长室
        {"Ziliaoshi", -50009},   // 资料室
        {"Liaowancang", -50007}, // 瞭望仓
        {"Liaowanlutai", -50103},// 瞭望露台
        {"Donlishi", -50006},   // 动力室
        {"Jixieshi", -50008},   // 机械室
        {"Famingshi", -50107},  // 发明室
        {"Shiyanshi", -50005},  // 实验室
        {"Chuangsoncang", -50001},// 传送仓
        {"Taikong", -50102},    // 太空站
        {"JiaoGuang", -50101}   // 教官室
    };

    void Start()
    {
        raycaster = GetComponent<GraphicRaycaster>();
        if (raycaster == null)
        {
            SeerGoLogger.LogError("在WorldMapCanvas上未找到GraphicRaycaster组件！", this);
        }
        UISystem = this.GetSystem<IUISystem>();
        mapSystem = this.GetSystem<IMapLoadSystem>();
        mapModel = this.GetModel<IMapModel>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
#if UNITY_EDITOR
            if (EventSystem.current.IsPointerOverGameObject())
            {
                DetectUIClick();
            }
#elif UNITY_ANDROID
            if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                DetectUIClick();
            }
#else
            if (EventSystem.current.IsPointerOverGameObject())
            {
                DetectUIClick();
            }
#endif
        }
    }

    public bool IsPointerOverUIObject(int fingerID)
    {
        return EventSystem.current.IsPointerOverGameObject(fingerID);
    }

    private void DetectUIClick()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerData, results);

        if (results.Count > 0)
        {
            string clickedObjectName = results[0].gameObject.name;

            if (uiToMapId.TryGetValue(clickedObjectName, out int mapId))
            {
                // 处理地图跳转
                PlayerAvatarPosHelper.Instance.ChangePlanetAvatarById(mapModel.CurrentMap.Category, false);
                mapSystem.RequestMapLoad(mapId);
                UISystem.SetUICanvasGroup("WorldMapCanvas", false);
            }
            else
            {
                // 特殊按钮处理
                if (clickedObjectName == "Close")
                {
                    UISystem.SetUICanvasGroup("WorldMapCanvas", false);
                }
                // LeftMap和RightMap不执行任何操作
            }
        }
    }
}