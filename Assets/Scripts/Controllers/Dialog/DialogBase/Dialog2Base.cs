using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Dialog2Base : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] protected Button confirmButton;
    [SerializeField] protected Button cancelButton;

    protected UnityAction onConfirm;
    protected UnityAction onCancel;
    protected bool isClosing = false;

    protected virtual void Awake()
    {
        RemoveAllButtonListeners();

        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmClick);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelClick);
    }

    /// <summary>
    /// 设置按钮事件
    /// </summary>
    public void SetButtonEvents(UnityAction confirmAction, UnityAction cancelAction)
    {
        onConfirm = confirmAction;
        onCancel = cancelAction;
    }

    private void OnConfirmClick()
    {
        if (isClosing) return;

        isClosing = true;
        onConfirm?.Invoke();
    }

    private void OnCancelClick()
    {
        if (isClosing) return;

        isClosing = true;
        onCancel?.Invoke();
    }

    /// <summary>
    /// 移除所有按钮监听器
    /// </summary>
    protected void RemoveAllButtonListeners()
    {
        if (confirmButton != null)
            confirmButton.onClick.RemoveAllListeners();

        if (cancelButton != null)
            cancelButton.onClick.RemoveAllListeners();
    }

    protected virtual void OnDestroy()
    {
        RemoveAllButtonListeners();
        onConfirm = null;
        onCancel = null;
    }
}