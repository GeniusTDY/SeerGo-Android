using UnityEngine;
using System.Collections.Generic;
using SeerGoGame.Utilities;
using UnityEngine.Events;

public class DialogBuyItemManager : MonoBehaviour
{
    [Header("资源设置")]
    [SerializeField] private string popupResourcePath = "Prefabs/Dialog/DialogBuyItemPanel";

    [Header("默认设置")]
    [SerializeField] private Transform defaultCanvasTransform;

    [Header("队列设置")]
    [SerializeField] private bool useGlobalQueue = true;

    private static DialogBuyItemManager instance;
    private Dictionary<string, Transform> canvasRegistry = new Dictionary<string, Transform>();

    public static DialogBuyItemManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<DialogBuyItemManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("DialogBuyItemManager");
                    instance = go.AddComponent<DialogBuyItemManager>();
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
    /// 显示购买物品弹窗（使用默认Canvas）
    /// </summary>
    public void ShowBuyItemPopup(string title, string message, int iconId, int initialCount,
                               System.Action<int> onConfirm = null, System.Action onCancel = null)
    {
        ShowBuyItemPopupOnTransform(defaultCanvasTransform, title, message, iconId, initialCount, onConfirm, onCancel);
    }

    /// <summary>
    /// 通过Transform指定父级Canvas显示购买物品弹窗
    /// </summary>
    public void ShowBuyItemPopupOnTransform(Transform parentCanvas, string title, string message, int iconId, int initialCount,
                                         System.Action<int> onConfirm = null, System.Action onCancel = null)
    {
        if (parentCanvas == null)
        {
            SeerGoLogger.LogError("Parent canvas is null! Using default canvas.");
            parentCanvas = defaultCanvasTransform;
        }

        if (useGlobalQueue)
        {
            // 使用全局弹窗队列
            GlobalPopupQueueManager.Instance.EnqueuePopup(
                () => CreateBuyItemPopupImmediately(parentCanvas, title, message, iconId, initialCount, onConfirm, onCancel),
                () => { /* 全局管理器会处理完成回调 */ },
                "DialogBuyItem"
            );
        }
        else
        {
            // 直接创建弹窗（不使用队列）
            CreateBuyItemPopupImmediately(parentCanvas, title, message, iconId, initialCount, onConfirm, onCancel);
        }
    }

    /// <summary>
    /// 通过已注册的Canvas名称显示购买物品弹窗
    /// </summary>
    public void ShowBuyItemPopupOnCanvas(string canvasName, string title, string message, int iconId, int initialCount,
                                      System.Action<int> onConfirm = null, System.Action onCancel = null)
    {
        if (canvasRegistry.TryGetValue(canvasName, out Transform canvasTransform))
        {
            ShowBuyItemPopupOnTransform(canvasTransform, title, message, iconId, initialCount, onConfirm, onCancel);
        }
        else
        {
            SeerGoLogger.LogError($"Canvas '{canvasName}' is not registered! Using default canvas.");
            ShowBuyItemPopup(title, message, iconId, initialCount, onConfirm, onCancel);
        }
    }

    /// <summary>
    /// 显示购买物品子弹窗（用于嵌套弹窗场景）
    /// </summary>
    public void ShowBuyItemSubPopup(string title, string message, int iconId, int initialCount,
                                  System.Action<int> onConfirm = null, System.Action onCancel = null)
    {
        ShowBuyItemSubPopupOnTransform(defaultCanvasTransform, title, message, iconId, initialCount, onConfirm, onCancel);
    }

    /// <summary>
    /// 通过Transform指定父级Canvas显示购买物品子弹窗
    /// </summary>
    public void ShowBuyItemSubPopupOnTransform(Transform parentCanvas, string title, string message, int iconId, int initialCount,
                                            System.Action<int> onConfirm = null, System.Action onCancel = null)
    {
        if (parentCanvas == null)
        {
            SeerGoLogger.LogError("Parent canvas is null! Using default canvas.");
            parentCanvas = defaultCanvasTransform;
        }

        if (useGlobalQueue)
        {
            // 使用全局弹窗队列的子弹窗方法
            GlobalPopupQueueManager.Instance.EnqueueSubPopup(
                () => CreateBuyItemPopupImmediately(parentCanvas, title, message, iconId, initialCount, onConfirm, onCancel, true),
                () => { /* 全局管理器会处理完成回调 */ },
                "DialogBuyItem_Sub"
            );
        }
        else
        {
            // 直接创建弹窗（不使用队列）
            CreateBuyItemPopupImmediately(parentCanvas, title, message, iconId, initialCount, onConfirm, onCancel, false);
        }
    }

    /// <summary>
    /// 立即显示购买物品弹窗（不通过全局队列）
    /// </summary>
    public void ShowBuyItemPopupImmediately(string title, string message, int iconId, int initialCount,
                                          System.Action<int> onConfirm = null, System.Action onCancel = null)
    {
        ShowBuyItemPopupImmediatelyOnTransform(defaultCanvasTransform, title, message, iconId, initialCount, onConfirm, onCancel);
    }

    /// <summary>
    /// 在指定Canvas上立即显示购买物品弹窗（不通过全局队列）
    /// </summary>
    public void ShowBuyItemPopupImmediatelyOnTransform(Transform parentCanvas, string title, string message, int iconId, int initialCount,
                                                     System.Action<int> onConfirm = null, System.Action onCancel = null)
    {
        if (parentCanvas == null)
        {
            SeerGoLogger.LogError("Parent canvas is null! Using default canvas.");
            parentCanvas = defaultCanvasTransform;
        }

        // 不通过全局队列，直接显示
        CreateBuyItemPopupImmediately(parentCanvas, title, message, iconId, initialCount, onConfirm, onCancel, false);
    }

    /// <summary>
    /// 立即创建购买物品弹窗（不经过队列）
    /// </summary>
    private void CreateBuyItemPopupImmediately(Transform parentCanvas, string title, string message, int iconId, int initialCount,
                                             System.Action<int> onConfirm, System.Action onCancel, bool isSubPopup = false)
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
        DialogBuyItem popup = popupObj.GetComponent<DialogBuyItem>();

        if (popup == null)
        {
            SeerGoLogger.LogError("DialogBuyItem component not found on popup prefab!");
            Destroy(popupObj);
            return;
        }

        // 设置弹窗位置和层级
        SetupPopupPosition(popupObj);

        // 初始化弹窗内容
        popup.Initialize(title, message, iconId, initialCount);

        // 使用UnityAction类型包装回调函数
        UnityAction wrappedOnConfirm = () =>
        {
            if (popup == null) return;

            // 获取输入的数量
            int inputCount = popup.GetValidItemCount();

            // 先销毁当前弹窗
            Destroy(popupObj);

            // 调用带数量的确认回调
            onConfirm?.Invoke(inputCount);

            // 通知全局队列管理器
            if (useGlobalQueue)
            {
                if (isSubPopup)
                {
                    // 子弹窗处理完成
                    GlobalPopupQueueManager.Instance.FinishSubPopupProcessing();
                }
                else
                {
                    // 主弹窗关闭
                    GlobalPopupQueueManager.Instance.NotifyPopupClosed();
                }
            }
        };

        UnityAction wrappedOnCancel = () =>
        {
            // 先销毁当前弹窗
            Destroy(popupObj);

            // 调用取消回调
            onCancel?.Invoke();

            // 通知全局队列管理器
            if (useGlobalQueue)
            {
                if (isSubPopup)
                {
                    // 子弹窗处理完成
                    GlobalPopupQueueManager.Instance.FinishSubPopupProcessing();
                }
                else
                {
                    // 主弹窗关闭
                    GlobalPopupQueueManager.Instance.NotifyPopupClosed();
                }
            }
        };

        // 设置按钮事件
        popup.SetButtonEvents(wrappedOnConfirm, wrappedOnCancel);
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
            var popups = FindObjectsOfType<DialogBuyItem>();
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
            return FindObjectOfType<DialogBuyItem>() != null;
        }
        else
        {
            // 对于队列模式，通过全局管理器检查
            string status = GlobalPopupQueueManager.Instance.GetQueueStatus();
            return status.Contains("正在显示: True");
        }
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
        SeerGoLogger.Log($"确认弹窗全局队列模式: {(useGlobal ? "启用" : "禁用")}");
    }

    /// <summary>
    /// 检查是否使用全局队列
    /// </summary>
    public bool IsUsingGlobalQueue()
    {
        return useGlobalQueue;
    }
}