using System.Collections.Generic;
using UnityEngine;
using System.Collections;

/// <summary>
/// 全局弹窗队列管理器 - 确保同一时间只显示一个弹窗（支持嵌套弹窗）
/// </summary>
public class GlobalPopupQueueManager : MonoBehaviour
{
    private static GlobalPopupQueueManager instance;

    // 全局弹窗队列
    private Queue<PopupRequest> globalPopupQueue = new Queue<PopupRequest>();
    private bool isShowingPopup = false;
    private int subPopupDepth = 0; // 子弹窗嵌套深度，代替简单的bool标记

    public static GlobalPopupQueueManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GlobalPopupQueueManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("GlobalPopupQueueManager");
                    instance = go.AddComponent<GlobalPopupQueueManager>();
                }
            }
            return instance;
        }
    }

    /// <summary>
    /// 弹窗请求结构
    /// </summary>
    public struct PopupRequest
    {
        public System.Action ShowAction;      // 显示弹窗的动作
        public System.Action OnComplete;      // 完成回调
        public string PopupType;              // 弹窗类型（用于调试）
        public bool IsSubPopup;               // 是否是子弹窗
    }

    /// <summary>
    /// 添加弹窗到全局队列
    /// </summary>
    public void EnqueuePopup(System.Action showAction, System.Action onComplete, string popupType = "Unknown")
    {
        var request = new PopupRequest
        {
            ShowAction = showAction,
            OnComplete = onComplete,
            PopupType = popupType,
            IsSubPopup = false
        };

        globalPopupQueue.Enqueue(request);
        //SeerGoLogger.Log($"弹窗入队: {popupType}, 当前队列长度: {globalPopupQueue.Count}, 嵌套深度: {subPopupDepth}");

        // 如果没有弹窗正在显示且没有嵌套弹窗，开始显示队列
        if (!isShowingPopup && subPopupDepth == 0)
        {
            ShowNextPopup();
        }
    }

    /// <summary>
    /// 添加子弹窗到队列（用于嵌套弹窗场景）
    /// </summary>
    public void EnqueueSubPopup(System.Action showAction, System.Action onComplete, string popupType = "Unknown")
    {
        var request = new PopupRequest
        {
            ShowAction = showAction,
            OnComplete = onComplete,
            PopupType = popupType,
            IsSubPopup = true
        };

        globalPopupQueue.Enqueue(request);
        //SeerGoLogger.Log($"子弹窗入队: {popupType}, 当前队列长度: {globalPopupQueue.Count}, 嵌套深度: {subPopupDepth}");

        // 如果没有弹窗正在显示且没有嵌套弹窗，开始显示队列
        if (!isShowingPopup && subPopupDepth == 0)
        {
            ShowNextPopup();
        }
    }

    /// <summary>
    /// 显示下一个弹窗
    /// </summary>
    private void ShowNextPopup()
    {
        if (globalPopupQueue.Count == 0)
        {
            //SeerGoLogger.Log("队列为空，无需显示弹窗");
            return;
        }

        if (isShowingPopup)
        {
            //SeerGoLogger.LogWarning("已有弹窗正在显示，等待当前弹窗关闭");
            return;
        }

        if (subPopupDepth > 0)
        {
            //SeerGoLogger.LogWarning($"存在嵌套弹窗（深度: {subPopupDepth}），等待嵌套弹窗关闭");
            return;
        }

        var request = globalPopupQueue.Dequeue();
        isShowingPopup = true;

        //SeerGoLogger.Log($"开始显示弹窗: {request.PopupType} ({(request.IsSubPopup ? "子弹窗" : "主弹窗")})");

        // 执行显示动作
        request.ShowAction?.Invoke();

        // 执行完成回调（如果有）
        request.OnComplete?.Invoke();
    }

    /// <summary>
    /// 通知全局管理器弹窗已关闭
    /// </summary>
    public void NotifyPopupClosed()
    {
        isShowingPopup = false;
        //SeerGoLogger.Log($"弹窗关闭，准备显示下一个，当前嵌套深度: {subPopupDepth}");

        // 短暂延迟后显示下一个弹窗，确保UI正确更新
        StartCoroutine(ShowNextPopupWithDelay(0.1f));
    }

    private IEnumerator ShowNextPopupWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 只有在没有嵌套子弹窗且没有显示弹窗时才显示下一个
        if (subPopupDepth == 0 && !isShowingPopup)
        {
            ShowNextPopup();
        }
        else
        {
           // SeerGoLogger.Log($"延迟后检查状态 - 显示中: {isShowingPopup}, 嵌套深度: {subPopupDepth}, 跳过显示下一个");
        }
    }

    /// <summary>
    /// 标记开始处理子弹窗
    /// </summary>
    public void StartSubPopupProcessing()
    {
        subPopupDepth++;
        //SeerGoLogger.Log($"开始处理子弹窗，当前嵌套深度: {subPopupDepth}");
    }

    /// <summary>
    /// 标记子弹窗处理完成
    /// </summary>
    public void FinishSubPopupProcessing()
    {
        if (subPopupDepth > 0)
        {
            subPopupDepth--;
            //SeerGoLogger.Log($"子弹窗处理完成，当前嵌套深度: {subPopupDepth}");
        }
        else
        {
           // SeerGoLogger.LogWarning("尝试减少嵌套深度，但当前深度已经是0");
        }

        // 子弹窗处理完成后，检查是否显示下一个弹窗
        StartCoroutine(ShowNextPopupWithDelay(0.1f));
    }

    /// <summary>
    /// 立即关闭所有弹窗并清空队列
    /// </summary>
    public void CloseAllPopups()
    {
        globalPopupQueue.Clear();
        isShowingPopup = false;
        subPopupDepth = 0;

       // SeerGoLogger.Log("已关闭所有弹窗并清空队列");
    }

    /// <summary>
    /// 获取当前队列状态
    /// </summary>
    public string GetQueueStatus()
    {
        return $"正在显示: {isShowingPopup}, 子弹窗嵌套深度: {subPopupDepth}, 队列长度: {globalPopupQueue.Count}";
    }

    /// <summary>
    /// 检查是否可以显示新弹窗
    /// </summary>
    public bool CanShowPopup()
    {
        return !isShowingPopup && subPopupDepth == 0;
    }

    /// <summary>
    /// 获取当前嵌套深度
    /// </summary>
    public int GetSubPopupDepth()
    {
        return subPopupDepth;
    }
}