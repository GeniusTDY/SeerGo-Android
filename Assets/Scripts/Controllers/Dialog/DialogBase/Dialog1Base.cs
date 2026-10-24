using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Dialog1Base : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] protected Button confirmButton;

    protected UnityAction onConfirm;
    protected bool isClosing = false;

    protected virtual void Awake()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnConfirmClick);
        }
    }

    /// <summary>
    /// 设置按钮事件
    /// </summary>
    public void SetButtonEvents(UnityAction confirmAction)
    {
        onConfirm = confirmAction;
    }

    private void OnConfirmClick()
    {
        if (isClosing) return;
        isClosing = true;
        onConfirm?.Invoke();
    }

    private void OnDestroy()
    {
        if (confirmButton != null)
            confirmButton.onClick.RemoveAllListeners();
        onConfirm = null;
    }
}