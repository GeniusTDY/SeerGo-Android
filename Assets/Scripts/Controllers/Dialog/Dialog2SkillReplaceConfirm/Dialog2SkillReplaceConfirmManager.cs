using SeerGoGame.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Dialog2SkillReplaceConfirmManager : MonoBehaviour
{
    [Header("资源设置")]
    [SerializeField] private string popupResourcePath = "Prefabs/Dialog/Dialog2SkillReplaceConfirmPanel";

    [Header("默认设置")]
    [SerializeField] private Transform defaultCanvasTransform;

    [Header("队列设置")]
    [SerializeField] private bool useGlobalQueue = true;

    private static Dialog2SkillReplaceConfirmManager instance;
    private Dictionary<string, Transform> canvasRegistry = new Dictionary<string, Transform>();

    public static Dialog2SkillReplaceConfirmManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<Dialog2SkillReplaceConfirmManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("Dialog2SkillReplaceConfirmManager");
                    instance = go.AddComponent<Dialog2SkillReplaceConfirmManager>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        InitializeManager();
    }

    private void InitializeManager()
    {
        if (defaultCanvasTransform == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null) defaultCanvasTransform = canvas.transform;
        }
    }

    /// <summary>
    /// 显示弹窗（使用默认Canvas）
    /// </summary>
    public void ShowPopup(int originId, int replaceId,
                        System.Action onConfirm = null, System.Action onCancel = null)
    {
        ShowPopupOnTransform(defaultCanvasTransform, originId, replaceId, onConfirm, onCancel);
    }

    /// <summary>
    /// 通过Transform指定父级Canvas显示弹窗
    /// </summary>
    public void ShowPopupOnTransform(Transform parentCanvas, int originId, int replaceId,
                                  System.Action onConfirm = null, System.Action onCancel = null)
    {
        if (parentCanvas == null)
        {
            SeerGoLogger.LogError("Parent canvas is null! Using default canvas.");
            parentCanvas = defaultCanvasTransform;
        }

        if (useGlobalQueue)
        {
            // 使用全局弹窗队列
            GlobalPopupQueueManager.Instance.EnqueueSubPopup(
                () => CreatePopupImmediately(parentCanvas, originId, replaceId, onConfirm, onCancel),
                () => { /* 全局管理器会处理完成回调 */ },
                "Dialog2SkillReplaceConfirm"
            );
        }
        else
        {
            // 直接创建弹窗（不使用队列）
            CreatePopupImmediately(parentCanvas, originId, replaceId, onConfirm, onCancel);
        }
    }

    /// <summary>
    /// 立即显示确认弹窗（不通过全局队列，用于技能替换弹窗的后续确认）
    /// </summary>
    public void ShowConfirmImmediately(int originId, int replaceId,
                                    System.Action onConfirm = null, System.Action onCancel = null)
    {
        ShowConfirmImmediatelyOnTransform(defaultCanvasTransform, originId, replaceId, onConfirm, onCancel);
    }

    /// <summary>
    /// 在指定Canvas上立即显示确认弹窗（不通过全局队列）
    /// </summary>
    public void ShowConfirmImmediatelyOnTransform(Transform parentCanvas, int originId, int replaceId,
                                               System.Action onConfirm = null, System.Action onCancel = null)
    {
        if (parentCanvas == null)
        {
            SeerGoLogger.LogError("Parent canvas is null! Using default canvas.");
            parentCanvas = defaultCanvasTransform;
        }

        // 不通过全局队列，直接显示
        CreatePopupImmediately(parentCanvas, originId, replaceId, onConfirm, onCancel);
    }

    /// <summary>
    /// 通过已注册的Canvas名称显示弹窗
    /// </summary>
    public void ShowPopupOnCanvas(string canvasName, int originId, int replaceId,
                               System.Action onConfirm = null, System.Action onCancel = null)
    {
        if (canvasRegistry.TryGetValue(canvasName, out Transform canvasTransform))
        {
            ShowPopupOnTransform(canvasTransform, originId, replaceId, onConfirm, onCancel);
        }
        else
        {
            SeerGoLogger.LogError($"Canvas '{canvasName}' is not registered! Using default canvas.");
            ShowPopup(originId, replaceId, onConfirm, onCancel);
        }
    }

    /// <summary>
    /// 注册Canvas
    /// </summary>
    public void RegisterCanvas(string canvasName, Transform canvasTransform)
    {
        if (!canvasRegistry.ContainsKey(canvasName))
        {
            canvasRegistry.Add(canvasName, canvasTransform);
            SeerGoLogger.Log($"Canvas '{canvasName}' registered successfully.");
        }
        else
        {
            canvasRegistry[canvasName] = canvasTransform;
            SeerGoLogger.Log($"Canvas '{canvasName}' updated.");
        }
    }

    /// <summary>
    /// 立即创建弹窗（不经过队列）
    /// </summary>
    private void CreatePopupImmediately(Transform parentCanvas, int originId, int replaceId,
                                     System.Action onConfirm, System.Action onCancel)
    {
        if (parentCanvas == null)
        {
            SeerGoLogger.LogError("No valid canvas found for popup!");
            return;
        }

        // 加载弹窗预制件
        GameObject popupPrefab = Resources.Load<GameObject>(popupResourcePath);
        if (popupPrefab == null)
        {
            SeerGoLogger.LogError($"Failed to load popup prefab from path: {popupResourcePath}");
            return;
        }

        // 实例化弹窗
        GameObject popupObj = Instantiate(popupPrefab, parentCanvas);
        Dialog2SkillReplaceConfirm popup = popupObj.GetComponent<Dialog2SkillReplaceConfirm>();

        if (popup == null)
        {
            SeerGoLogger.LogError("Dialog component not found on popup prefab!");
            Destroy(popupObj);
            return;
        }

        // 设置弹窗位置和层级
        SetupPopupPosition(popupObj);

        // 包装回调函数
        UnityAction wrappedOnConfirm = () =>
        {
            // 先调用原始确认回调
            onConfirm?.Invoke();

            // 然后销毁当前弹窗
            Destroy(popupObj);

            // 标记子弹窗处理完成
            if (useGlobalQueue)
            {
                GlobalPopupQueueManager.Instance.FinishSubPopupProcessing();
            }
        };

        UnityAction wrappedOnCancel = () =>
        {
            // 先调用原始取消回调
            onCancel?.Invoke();

            // 然后销毁当前弹窗
            Destroy(popupObj);

            // 标记子弹窗处理完成
            if (useGlobalQueue)
            {
                GlobalPopupQueueManager.Instance.FinishSubPopupProcessing();
            }
        };

        // 初始化弹窗
        popup.Initialize(originId, replaceId, parentCanvas);
        popup.SetButtonEvents(wrappedOnConfirm, wrappedOnCancel);

    }

    /// <summary>
    /// 设置弹窗位置
    /// </summary>
    private void SetupPopupPosition(GameObject popupObj)
    {
        var rectTransform = popupObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }

        popupObj.transform.SetAsLastSibling();
    }

    /// <summary>
    /// 立即关闭所有弹窗
    /// </summary>
    public void CloseAllPopups()
    {
        if (!useGlobalQueue)
        {
            // 对于非队列模式，查找并销毁当前场景中的弹窗
            var popups = FindObjectsOfType<Dialog2SkillReplaceConfirm>();
            foreach (var popup in popups)
            {
                Destroy(popup.gameObject);
            }
        }
        else
        {
            SeerGoLogger.Log("使用全局队列模式，请通过GlobalPopupQueueManager关闭弹窗");
        }
    }

    /// <summary>
    /// 检查是否有弹窗正在显示
    /// </summary>
    public bool IsPopupShowing()
    {
        if (!useGlobalQueue)
        {
            // 对于非队列模式，检查当前是否有弹窗
            return FindObjectOfType<Dialog2SkillReplaceConfirm>() != null;
        }
        else
        {
            // 对于队列模式，通过全局管理器检查
            string status = GlobalPopupQueueManager.Instance.GetQueueStatus();
            return status.Contains("正在显示: True");
        }
    }

    /// <summary>
    /// 设置默认Canvas
    /// </summary>
    public void SetDefaultCanvas(Transform canvasTransform)
    {
        defaultCanvasTransform = canvasTransform;
    }

    /// <summary>
    /// 设置弹窗资源路径
    /// </summary>
    public void SetPopupResourcePath(string path)
    {
        popupResourcePath = path;
    }

    /// <summary>
    /// 设置是否使用全局队列
    /// </summary>
    public void SetUseGlobalQueue(bool useGlobal)
    {
        useGlobalQueue = useGlobal;
        SeerGoLogger.Log($"技能替换确认弹窗全局队列模式: {(useGlobal ? "启用" : "禁用")}");
    }

    /// <summary>
    /// 获取当前队列状态（如果使用全局队列）
    /// </summary>
    public string GetQueueStatus()
    {
        if (useGlobalQueue)
        {
            return GlobalPopupQueueManager.Instance.GetQueueStatus();
        }
        else
        {
            return "使用直接显示模式（非队列）";
        }
    }

    /// <summary>
    /// 检查是否使用全局队列
    /// </summary>
    public bool IsUsingGlobalQueue()
    {
        return useGlobalQueue;
    }
}