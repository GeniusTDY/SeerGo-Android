using UnityEngine;
using TMPro;

public class Dialog2 : Dialog2Base
{
    [Header("UI组件")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;

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
    }
}