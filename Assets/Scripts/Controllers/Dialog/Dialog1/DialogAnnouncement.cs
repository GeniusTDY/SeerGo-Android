using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;  // 需要这个来使用IEnumerator

public class DialogAnnouncement : Dialog1Base
{
    [Header("UI组件")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private ScrollRect scrollRect;  // 从public改为[SerializeField]
    [SerializeField] private RectTransform content;  // 从public改为[SerializeField]

    [Header("边距设置")]
    [SerializeField] private float topBottomMargin = 20f;  // 上下边距
    [SerializeField] private float leftRightMargin = 20f;  // 左右边距
    [SerializeField] private float minContentHeight = 200f;  // 内容区域最小高度

    // 在弹窗完全显示后，再计算和设置内容高度
    public IEnumerator LateInitialize()
    {
        // 确保所有UI都完成布局
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        UpdateContentHeight();
    }

    /// <summary>
    /// 更新内容区域高度
    /// </summary>
    public void UpdateContentHeight()
    {
        if (content == null || messageText == null || scrollRect == null) return;

        // 1. 获取文本真实高度
        float textHeight = messageText.preferredHeight;

        // 2. 加上边距计算总高度
        float totalHeight = textHeight + (topBottomMargin * 2);

        // 3. 确保不低于最小高度
        totalHeight = Mathf.Max(totalHeight, minContentHeight);

        // 4. 设置内容区域高度
        content.sizeDelta = new Vector2(content.sizeDelta.x, totalHeight);

        // 5. 强制刷新UI布局
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        Canvas.ForceUpdateCanvases();

        // 6. 滚动到顶部
        scrollRect.verticalNormalizedPosition = 1f;
    }

    /// <summary>
    /// 初始化弹窗内容
    /// </summary>
    public void Initialize(string title, string message)
    {
        if (titleText != null)
            titleText.text = string.IsNullOrEmpty(title) ? "提示" : title;

        if (messageText != null)
            messageText.text = message ?? "";

        isClosing = false;

        // 延迟一帧更新高度，确保文本布局计算完成
        StartCoroutine(DelayedUpdateHeight());
    }

    /// <summary>
    /// 延迟更新高度
    /// </summary>
    private IEnumerator DelayedUpdateHeight()
    {
        // 等待一帧，让TextMeshPro完成文本网格生成
        yield return null;
        UpdateContentHeight();
    }

    /// <summary>
    /// 设置消息文本（外部调用）
    /// </summary>
    public void SetMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
            StartCoroutine(DelayedUpdateHeight());
        }
    }

    /// <summary>
    /// 设置标题文本
    /// </summary>
    public void SetTitle(string title)
    {
        if (titleText != null)
            titleText.text = title;
    }

    /// <summary>
    /// 设置边距
    /// </summary>
    public void SetMargins(float topBottom, float leftRight)
    {
        topBottomMargin = topBottom;
        leftRightMargin = leftRight;
        UpdateContentHeight();
    }

    /// <summary>
    /// 设置最小高度
    /// </summary>
    public void SetMinHeight(float minHeight)
    {
        minContentHeight = minHeight;
        UpdateContentHeight();
    }

    /// <summary>
    /// 手动刷新高度（用于编辑器调试）
    /// </summary>
    [ContextMenu("刷新高度")]
    public void RefreshHeight()
    {
        UpdateContentHeight();
    }
}