using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using SeerGoGame.Utilities;

public class BattleLoadProgressController : MonoBehaviour
{
    [Header("UI引用")]
    public float speed = 0.5f;
    public float targetProgress = 1f;
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI progressText;

    // 队列管理
    private Dictionary<string, int> _queueTasks = new Dictionary<string, int>();
    private readonly object _queueLock = new object();

    // 状态控制
    private bool _isActive = false;
    private float _currentProgress = 0f;

    // 单例模式
    private static BattleLoadProgressController instance;
    public static BattleLoadProgressController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<BattleLoadProgressController>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("BattleLoadProgressController");
                    instance = obj.AddComponent<BattleLoadProgressController>();
                }
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
            InitializeComponents();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 确保开始时隐藏加载界面
        // SetActiveState(false);
    }

    void Update()
    {
        // 如果非激活状态，不执行Update逻辑
        if (!_isActive) return;

        UpdateProgress();
    }

    /// <summary>
    /// 初始化组件引用
    /// </summary>
    private void InitializeComponents()
    {
        // 尝试自动查找组件（如果未在Inspector中分配）
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (progressText == null)
            progressText = GetComponentInChildren<TextMeshProUGUI>();
    }

    /// <summary>
    /// 更新进度条逻辑
    /// </summary>
    private void UpdateProgress()
    {
        int totalRemainingTasks = GetTotalRemainingTasks();

        // 如果有任务未完成
        if (totalRemainingTasks > 0)
        {
            // 平滑过渡到99%，避免直接跳到100%
            if (_currentProgress < 0.99f)
            {
                _currentProgress += Time.deltaTime * speed;
                _currentProgress = Mathf.Min(_currentProgress, 0.99f);
                UpdateSliderValue(_currentProgress);
            }
        }
        // 所有任务完成，但进度条还未到目标值
        else if (totalRemainingTasks == 0 && _currentProgress < targetProgress)
        {
            _currentProgress = targetProgress;
            UpdateSliderValue(_currentProgress);

            // 进度完成后的处理
            if (_currentProgress >= targetProgress)
            {
                OnAllTasksCompleted();
            }
        }
    }

    /// <summary>
    /// 更新滑块值并刷新文本
    /// </summary>
    private void UpdateSliderValue(float value)
    {
        UpdateProgressText();
    }

    /// <summary>
    /// 所有任务完成时的处理
    /// </summary>
    private void OnAllTasksCompleted()
    {
        // 延迟一小段时间再隐藏，让用户看到100%完成
        StartCoroutine(HideAfterDelay(0.5f));
    }

    private System.Collections.IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        ClearAllQueues();
        SetActiveState(false);
    }

    /// <summary>
    /// 设置控制器激活状态
    /// </summary>
    private void SetActiveState(bool isActive)
    {
        _isActive = isActive;
        _currentProgress = isActive ? 0f : 0f;

        SetCanvasGroupProperties(canvasGroup, isActive);

        // if (enableLogs) SeerGoLogger.Log($"加载控制器 {(isActive ? "激活" : "禁用")}");
    }

    /// <summary>
    /// 注册新队列并设置初始任务数
    /// </summary>
    public bool RegisterQueue(string queueIdentifier, int initialTaskCount)
    {
        if (string.IsNullOrEmpty(queueIdentifier))
        {
            SeerGoLogger.LogError("队列标识符不能为空");
            return false;
        }

        lock (_queueLock)
        {
            if (_queueTasks.ContainsKey(queueIdentifier))
            {
                return false;
            }

            _queueTasks[queueIdentifier] = Mathf.Max(0, initialTaskCount);

            // 激活加载界面
            if (!_isActive && initialTaskCount > 0)
            {
                SetActiveState(true);
            }

            return true;
        }
    }

    /// <summary>
    /// 为指定队列设置任务数量
    /// </summary>
    public void SetQueueTaskCount(string queueIdentifier, int count)
    {
        lock (_queueLock)
        {
            if (_queueTasks.ContainsKey(queueIdentifier))
            {
                int oldCount = _queueTasks[queueIdentifier];
                _queueTasks[queueIdentifier] = Mathf.Max(0, count);

                // 如果从0变为有任务，激活界面
                if (oldCount == 0 && count > 0 && !_isActive)
                {
                    SetActiveState(true);
                }

            }
            else
            {
                SeerGoLogger.LogError($"队列 '{queueIdentifier}' 不存在");
            }
        }
    }

    /// <summary>
    /// 完成指定队列的一个任务
    /// </summary>
    public void CompleteTask(string queueIdentifier)
    {
        CompleteTasks(queueIdentifier, 1);
    }

    /// <summary>
    /// 完成指定队列的多个任务
    /// </summary>
    public void CompleteTasks(string queueIdentifier, int count)
    {
        if (count <= 0) return;

        lock (_queueLock)
        {
            if (_queueTasks.ContainsKey(queueIdentifier))
            {
                int oldCount = _queueTasks[queueIdentifier];
                _queueTasks[queueIdentifier] = Mathf.Max(0, oldCount - count);
            }
            else
            {
                SeerGoLogger.LogError($"队列 '{queueIdentifier}' 不存在");
            }
        }
    }

    /// <summary>
    /// 获取指定队列的剩余任务数
    /// </summary>
    public int GetQueueRemainingTasks(string queueIdentifier)
    {
        lock (_queueLock)
        {
            return _queueTasks.ContainsKey(queueIdentifier) ? _queueTasks[queueIdentifier] : -1;
        }
    }

    /// <summary>
    /// 获取所有队列的总剩余任务数
    /// </summary>
    public int GetTotalRemainingTasks()
    {
        lock (_queueLock)
        {
            return _queueTasks.Values.Sum();
        }
    }

    /// <summary>
    /// 检查所有队列是否都已完成
    /// </summary>
    public bool AreAllQueuesCompleted()
    {
        return GetTotalRemainingTasks() <= 0;
    }

    /// <summary>
    /// 检查指定队列是否已完成
    /// </summary>
    public bool IsQueueCompleted(string queueIdentifier)
    {
        lock (_queueLock)
        {
            return _queueTasks.ContainsKey(queueIdentifier) ? _queueTasks[queueIdentifier] <= 0 : false;
        }
    }

    /// <summary>
    /// 移除指定队列（强制完成）
    /// </summary>
    public void RemoveQueue(string queueIdentifier)
    {
        lock (_queueLock)
        {
            if (_queueTasks.ContainsKey(queueIdentifier))
            {
                _queueTasks.Remove(queueIdentifier);
            }
        }
    }

    /// <summary>
    /// 获取所有活跃队列标识符
    /// </summary>
    public List<string> GetActiveQueueIdentifiers()
    {
        lock (_queueLock)
        {
            return new List<string>(_queueTasks.Keys);
        }
    }

    /// <summary>
    /// 清空所有队列（重置进度控制器）
    /// </summary>
    public void ClearAllQueues()
    {
        lock (_queueLock)
        {
            _queueTasks.Clear();
        }
    }

    /// <summary>
    /// 强制完成所有队列（用于紧急情况）
    /// </summary>
    public void ForceCompleteAll()
    {
        lock (_queueLock)
        {
            foreach (var queue in _queueTasks.Keys.ToList())
            {
                _queueTasks[queue] = 0;
            }
        }
    }

    /// <summary>
    /// 设置CanvasGroup属性
    /// </summary>
    private void SetCanvasGroupProperties(CanvasGroup group, bool isEnabled)
    {
        if (group != null)
        {
            group.alpha = isEnabled ? 1 : 0;
            group.interactable = isEnabled;
            group.blocksRaycasts = isEnabled;
        }
    }

    /// <summary>
    /// 更新进度文本显示
    /// </summary>
    private void UpdateProgressText()
    {
        if (progressText != null)
        {
            int totalTasks = GetTotalRemainingTasks();
            int activeQueues = _queueTasks.Count;
            int percentage = Mathf.RoundToInt(_currentProgress * 100);

            progressText.text = $"{percentage}%";

        }
    }

    /// <summary>
    /// 手动显示加载界面（用于测试）
    /// </summary>
    public void ShowLoading()
    {
        SetActiveState(true);
    }

    /// <summary>
    /// 手动隐藏加载界面（用于测试）
    /// </summary>
    public void HideLoading()
    {
        SetActiveState(false);
    }

    void OnDestroy()
    {
        // 清理资源
        if (instance == this)
        {
            instance = null;
        }
    }
}