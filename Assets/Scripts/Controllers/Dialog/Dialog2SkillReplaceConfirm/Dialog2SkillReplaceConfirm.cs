using QFramework;
using SeerGoGame.Controller;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using System;
using UnityEngine;
using UnityEngine.Events;
using Button = UnityEngine.UI.Button;

public class Dialog2SkillReplaceConfirm : BaseController
{
    [Header("UI组件")]
    [SerializeField] private GameObject OringinSkill;
    [SerializeField] private GameObject ReplaceSkill;
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private Action onCloseCallback;
    private UnityAction onConfirm;
    private UnityAction onCancel;
    private bool isClosing = false;
    private bool isActive = false;

    private DialogSkillItem item1;
    private DialogSkillItem item2;

    private ISkillSystem _skillSystem;

    private void Awake()
    {
        // 确保清除之前的事件监听
        RemoveAllButtonListeners();

        _skillSystem = this.GetSystem<ISkillSystem>();

        item1 = OringinSkill?.GetComponent<DialogSkillItem>();
        item2 = ReplaceSkill?.GetComponent<DialogSkillItem>();

        // 绑定按钮事件
        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmClick);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelClick);

        // 初始状态
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
        }
    }

    /// <summary>
    /// 初始化弹窗内容 - 修复参数类型为 UnityAction
    /// </summary>
    public void Initialize(int originId, int replaceId,Transform canvas, UnityAction confirmAction = null, UnityAction cancelAction = null)
    {
        SkillData originSkillData = _skillSystem?.GetSkillById(originId.ToString());
        SkillData replaceSkillData1 = _skillSystem?.GetSkillById(replaceId.ToString());
        Canvas uiCanvas = canvas.GetComponent<Canvas>();
        if (item1 != null && originSkillData != null)
            item1.SetDialogSkillItem(originSkillData, OringinSkill, uiCanvas);
        if (item2 != null && replaceSkillData1 != null)
            item2.SetDialogSkillItem(replaceSkillData1, ReplaceSkill, uiCanvas);

        onConfirm = confirmAction;
        onCancel = cancelAction;
        isClosing = false;
        isActive = true;

        // 显示动画
        StartCoroutine(ShowAnimation());
    }

    /// <summary>
    /// 设置按钮事件 - 修复参数类型为 UnityAction
    /// </summary>
    public void SetButtonEvents(UnityAction confirmAction, UnityAction cancelAction)
    {
        onConfirm = confirmAction;
        onCancel = cancelAction;
    }

    /// <summary>
    /// 设置关闭回调（实现IPopupController接口）
    /// </summary>
    public void SetCloseCallback(Action onClose)
    {
        onCloseCallback = onClose;
    }

    /// <summary>
    /// 关闭弹窗（实现IPopupController接口）
    /// </summary>
    public void Close()
    {
        if (isClosing || !isActive) return;

        isClosing = true;
        StartCoroutine(CloseCoroutine());
    }

    private void OnConfirmClick()
    {
        if (isClosing || !isActive) return;

        isClosing = true;
        onConfirm?.Invoke();
    }

    private void OnCancelClick()
    {
        if (isClosing || !isActive) return;

        isClosing = true;
        onCancel?.Invoke();
    }

    /// <summary>
    /// 显示动画
    /// </summary>
    private System.Collections.IEnumerator ShowAnimation()
    {
        if (canvasGroup != null)
        {
            canvasGroup.interactable = true;

            float elapsed = 0;
            float duration = 0.3f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }
            canvasGroup.alpha = 1;
        }
    }

    /// <summary>
    /// 关闭动画
    /// </summary>
    private System.Collections.IEnumerator CloseCoroutine()
    {
        isActive = false;

        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;

            float elapsed = 0;
            float duration = 0.2f;
            float startAlpha = canvasGroup.alpha;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0, elapsed / duration);
                yield return null;
            }
        }

        // 销毁对象并回调
        Destroy(gameObject);
        onCloseCallback?.Invoke();
    }

    /// <summary>
    /// 移除所有按钮监听器
    /// </summary>
    private void RemoveAllButtonListeners()
    {
        if (confirmButton != null)
            confirmButton.onClick.RemoveAllListeners();

        if (cancelButton != null)
            cancelButton.onClick.RemoveAllListeners();
    }

    private void OnDestroy()
    {
        RemoveAllButtonListeners();
        onConfirm = null;
        onCancel = null;
        onCloseCallback = null;
    }
}