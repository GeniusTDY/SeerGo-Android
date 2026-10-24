using PixeLadder.SimpleTooltip;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Pool;
using SeerGoGame.Utilities;
using System;
using System.Collections;

public class FunctionPanelLoader : MonoBehaviour
{
    [Header("配置")]
    [SerializeField] private Transform panelContainer; // 面板挂载的父节点

    [Header("对象池设置")]
    [SerializeField] private int maxPoolSizePerPanel = 1; // 每个面板的最大缓存数量
    [SerializeField] private int defaultCollectionCheck = 10; // 集合检查大小

    private Dictionary<string, GameObject> panelPrefabCache = new Dictionary<string, GameObject>(); // 预制体缓存
    private Dictionary<string, ObjectPool<GameObject>> panelPools = new Dictionary<string, ObjectPool<GameObject>>(); // 对象池
    private Dictionary<string, GameObject> activePanels = new Dictionary<string, GameObject>(); // 当前活动的面板

    private const string BASE_PATH = "Prefabs/Panel/";

    // 单例实例
    private static FunctionPanelLoader _instance;

    /// <summary>
    /// 获取单例实例
    /// </summary>
    public static FunctionPanelLoader Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<FunctionPanelLoader>();

                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(FunctionPanelLoader).Name);
                    _instance = singletonObject.AddComponent<FunctionPanelLoader>();
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        // 确保只有一个实例存在
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        if (panelContainer == null)
        {
            panelContainer = transform;
        }
    }

    public Canvas GetCanvas()
    {
        return panelContainer.GetComponent<Canvas>();
    }

    /// <summary>
    /// 加载功能面板
    /// </summary>
    /// <param name="panelName">面板名称，格式如："Shop/ShopPanel" 或 "Bag/BagPanel"</param>
    /// <param name="onPanelLoaded">面板加载完成后的回调</param>
    public void LoadPanel(string panelName, Action<GameObject> onPanelLoaded = null)
    {
        // 如果面板已经加载，直接返回
        if (activePanels.ContainsKey(panelName) && activePanels[panelName] != null)
        {
            SeerGoLogger.Log($"面板 {panelName} 已经加载，跳过重复加载");
            onPanelLoaded?.Invoke(activePanels[panelName]);
            return;
        }

        // 获取或创建对象池
        ObjectPool<GameObject> pool = GetOrCreatePool(panelName);
        if (pool == null)
        {
            SeerGoLogger.LogError($"创建面板 {panelName} 的对象池失败");
            onPanelLoaded?.Invoke(null);
            return;
        }

        // 从对象池获取面板实例
        GameObject panel = pool.Get();

        if (panel != null)
        {
            // 添加到活动面板列表
            activePanels[panelName] = panel;

            // 确保面板是激活状态
            panel.SetActive(true);

            // 等待一帧确保面板的Awake和Start已执行，然后调用回调
            StartCoroutine(InvokeCallbackAfterFrame(panelName, panel, onPanelLoaded));
        }
        else
        {
            SeerGoLogger.LogError($"从对象池获取的面板实例为空: {panelName}");
            onPanelLoaded?.Invoke(null);
        }
    }

    /// <summary>
    /// 协程：在一帧后调用回调
    /// </summary>
    private IEnumerator InvokeCallbackAfterFrame(string panelName, GameObject panel, Action<GameObject> callback)
    {
        // 等待一帧，确保面板的Awake和Start已执行
        yield return null;

        // 再次检查面板是否仍然存在且激活
        if (panel != null && activePanels.ContainsKey(panelName) && activePanels[panelName] == panel)
        {
            callback?.Invoke(panel);
        }
        else
        {
            SeerGoLogger.LogWarning($"面板 {panelName} 在等待期间被关闭或替换");
        }
    }

    /// <summary>
    /// 获取或创建对象池
    /// </summary>
    private ObjectPool<GameObject> GetOrCreatePool(string panelName)
    {
        // 如果已存在对象池，直接返回
        if (panelPools.TryGetValue(panelName, out ObjectPool<GameObject> existingPool))
        {
            return existingPool;
        }

        // 获取预制体
        GameObject prefab = GetPanelPrefab(panelName);
        if (prefab == null)
        {
            SeerGoLogger.LogError($"未找到面板预制体: {panelName}");
            return null;
        }

        // 创建新的对象池
        ObjectPool<GameObject> newPool = new ObjectPool<GameObject>(
            createFunc: () => CreatePanelInstance(panelName, prefab),
            actionOnGet: OnGetPanelFromPool,
            actionOnRelease: OnReleasePanelToPool,
            actionOnDestroy: OnDestroyPanelInstance,
            collectionCheck: defaultCollectionCheck > 0,
            defaultCapacity: 1,
            maxSize: maxPoolSizePerPanel
        );

        // 缓存对象池
        panelPools.Add(panelName, newPool);
        return newPool;
    }

    /// <summary>
    /// 获取面板预制体
    /// </summary>
    private GameObject GetPanelPrefab(string panelName)
    {
        // 检查缓存
        if (panelPrefabCache.TryGetValue(panelName, out GameObject cachedPrefab))
        {
            return cachedPrefab;
        }

        // 加载预制体
        string fullPath = BASE_PATH + panelName;
        GameObject prefab = Resources.Load<GameObject>(fullPath);

        if (prefab != null)
        {
            panelPrefabCache.Add(panelName, prefab);
        }
        else
        {
            SeerGoLogger.LogError($"加载预制体失败: {fullPath}");
        }

        return prefab;
    }

    /// <summary>
    /// 创建面板实例
    /// </summary>
    private GameObject CreatePanelInstance(string panelName, GameObject prefab)
    {
        GameObject instance = Instantiate(prefab, panelContainer);
        instance.name = panelName;
        instance.SetActive(false);
        SetupPanel(instance);
        return instance;
    }

    /// <summary>
    /// 从对象池获取时的处理
    /// </summary>
    private void OnGetPanelFromPool(GameObject panel)
    {
        panel.SetActive(true);
        panel.transform.SetAsLastSibling(); // 确保在最上层
    }

    /// <summary>
    /// 释放到对象池时的处理
    /// </summary>
    private void OnReleasePanelToPool(GameObject panel)
    {
        // 统一清理残留tooltip，避免面板关闭后tooltip仍然显示
        if (TooltipManager.Instance != null)
        {
            TooltipManager.Instance.ForceHideAllImmediate();
        }
        panel.SetActive(false);
    }

    /// <summary>
    /// 销毁实例时的处理
    /// </summary>
    private void OnDestroyPanelInstance(GameObject panel)
    {
        Destroy(panel);
    }

    /// <summary>
    /// 设置面板位置
    /// </summary>
    private void SetupPanel(GameObject panel)
    {
        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.localPosition = Vector3.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.localScale = Vector3.one;
        }
    }

    /// <summary>
    /// 关闭指定面板
    /// </summary>
    public void ClosePanel(string panelName)
    {
        if (activePanels.TryGetValue(panelName, out GameObject panel) && panel != null)
        {
            // 释放到对象池
            if (panelPools.TryGetValue(panelName, out ObjectPool<GameObject> pool))
            {
                pool.Release(panel);
            }
            else
            {
                // 如果没有对象池，直接销毁
                Destroy(panel);
            }

            // 从活动面板列表中移除
            activePanels.Remove(panelName);
        }
    }

    /// <summary>
    /// 关闭所有面板
    /// </summary>
    public void CloseAllPanels()
    {
        foreach (var panelEntry in activePanels)
        {
            if (panelEntry.Value != null)
            {
                if (panelPools.TryGetValue(panelEntry.Key, out ObjectPool<GameObject> pool))
                {
                    pool.Release(panelEntry.Value);
                }
                else
                {
                    Destroy(panelEntry.Value);
                }
            }
        }
        activePanels.Clear();
    }

    /// <summary>
    /// 强制关闭指定面板（不检查条件）
    /// </summary>
    public void ForceClosePanel(string panelName)
    {
        if (activePanels.TryGetValue(panelName, out GameObject panel) && panel != null)
        {
            Destroy(panel);
            activePanels.Remove(panelName);
        }
    }

    /// <summary>
    /// 获取指定面板
    /// </summary>
    public GameObject GetPanel(string panelName)
    {
        if (activePanels.TryGetValue(panelName, out GameObject panel))
        {
            return panel;
        }
        return null;
    }

    /// <summary>
    /// 检查面板是否已加载
    /// </summary>
    public bool IsPanelLoaded(string panelName)
    {
        return activePanels.ContainsKey(panelName) && activePanels[panelName] != null && activePanels[panelName].activeSelf;
    }

    /// <summary>
    /// 获取所有已加载的面板名称
    /// </summary>
    public List<string> GetAllActivePanelNames()
    {
        return new List<string>(activePanels.Keys);
    }

    /// <summary>
    /// 清理对象池
    /// </summary>
    public void ClearAllPools()
    {
        CloseAllPanels();

        foreach (var pool in panelPools.Values)
        {
            pool.Clear();
        }

        panelPools.Clear();
        panelPrefabCache.Clear();
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            ClearAllPools();
            _instance = null;
        }
    }

    /// <summary>
    /// 检查单例是否存在
    /// </summary>
    public static bool IsInitialized
    {
        get { return _instance != null; }
    }
}