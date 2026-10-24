using UnityEngine;
using TMPro;

public class DialogBattle : Dialog1Base
{
    [Header("UI组件")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private GameObject FightWinAnim;
    [SerializeField] private GameObject FightLoseAnim;

    /// <summary>
    /// 初始化弹窗内容
    /// </summary>
    public void Initialize(string title, string message, bool isWin)
    {
        if (titleText != null)
            titleText.text = string.IsNullOrEmpty(title) ? "提示" : title;
        if (messageText != null)
            messageText.text = message ?? "";

        FightWinAnim.SetActive(isWin);
        FightLoseAnim.SetActive(!isWin);

        isClosing = false;
    }
}