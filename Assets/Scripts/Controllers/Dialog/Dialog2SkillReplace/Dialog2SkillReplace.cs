using QFramework;
using SeerGoGame.Controller;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Button = UnityEngine.UI.Button;

public class Dialog2SkillReplace : BaseController
{
    [Header("UI组件")]
    [SerializeField] private Button ReplaceButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private GameObject ReplaceSkill;
    [SerializeField] private GameObject MySkill1;
    [SerializeField] private GameObject MySkill2;
    [SerializeField] private GameObject MySkill3;
    [SerializeField] private GameObject MySkill4;

    // 回调定义：参数为选中的技能下标
    private UnityAction<int> onConfirmWithIndex;
    private UnityAction onCancel;
    private Action onCloseCallback;
    private bool isClosing = false;
    private bool isActive = false;

    private DialogSkillItem replaceSkillItem;
    private DialogSkillItem MySkill1Item;
    private DialogSkillItem MySkill2Item;
    private DialogSkillItem MySkill3lItem;
    private DialogSkillItem MySkill4Item;
    private List<DialogSkillItem> skillItemBg;

    // 当前选中的技能下标
    private int currentSelectedIndex = -1;

    private ISkillSystem _skillSystem;

    private const string selectBgPath = "Sprites/Dialog/selectBg";
    private const string skillBgPath = "Sprites/Dialog/skillBg";

    private Sprite skillBgSprite;
    private Sprite selectBgSprite;

    private void Awake()
    {
        // 初始化列表
        skillItemBg = new List<DialogSkillItem>();

        RemoveAllButtonListeners();

        // 获取技能项组件
        replaceSkillItem = ReplaceSkill.GetComponent<DialogSkillItem>();

        MySkill1Item = MySkill1.GetComponent<DialogSkillItem>();
        MySkill2Item = MySkill2.GetComponent<DialogSkillItem>();
        MySkill3lItem = MySkill3.GetComponent<DialogSkillItem>();
        MySkill4Item = MySkill4.GetComponent<DialogSkillItem>();

        skillItemBg.Add(MySkill1Item);
        skillItemBg.Add(MySkill2Item);
        skillItemBg.Add(MySkill3lItem);
        skillItemBg.Add(MySkill4Item);

        skillBgSprite = Resources.Load<Sprite>(skillBgPath);
        selectBgSprite = Resources.Load<Sprite>(selectBgPath);

        _skillSystem = this.GetSystem<ISkillSystem>();

        // 绑定按钮事件
        if (ReplaceButton != null)
            ReplaceButton.onClick.AddListener(OnConfirmClick);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelClick);

        // 为每个技能槽位添加点击事件
        SetupSkillItemClickEvents();

        // 初始状态
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
        }
    }

    /// <summary>
    /// 为技能槽位设置点击事件
    /// </summary>
    private void SetupSkillItemClickEvents()
    {
        for (int i = 0; i < skillItemBg.Count; i++)
        {
            int index = i; // 避免闭包问题
            Button skillButton = skillItemBg[i].gameObject.GetComponent<Button>();
            if (skillButton != null)
            {
                skillButton.onClick.RemoveAllListeners();
                skillButton.onClick.AddListener(() => OnSkillItemClicked(index));
            }
        }
    }

    /// <summary>
    /// 初始化弹窗内容
    /// </summary>
    public void Initialize(PetData petData, SkillData newSkillData, Transform uiCanvasCache)
    {
        if (petData == null || newSkillData == null)
        {
            SeerGoLogger.LogError("PetData or SkillData is null!");
            return;
        }

        // 查询4个技能
        List<EquippedSkill> normalSkills = petData.Skills?.NormalSkill;
        if (normalSkills == null || normalSkills.Count < 4)
        {
            SeerGoLogger.LogError("NormalSkillId is null or count less than 4!");
            return;
        }

        Canvas canvas = uiCanvasCache.GetComponent<Canvas>();
        // 赋值
        if (replaceSkillItem != null)
            replaceSkillItem.SetDialogSkillItem(newSkillData, ReplaceSkill, canvas);

        if (MySkill1Item != null)
            MySkill1Item.SetDialogSkillItem(_skillSystem.GetSkillById(normalSkills[0].id.ToString()), MySkill1, canvas);
        if (MySkill2Item != null)
            MySkill2Item.SetDialogSkillItem(_skillSystem.GetSkillById(normalSkills[1].id.ToString()), MySkill2, canvas);
        if (MySkill3lItem != null)
            MySkill3lItem.SetDialogSkillItem(_skillSystem.GetSkillById(normalSkills[2].id.ToString()), MySkill3, canvas);
        if (MySkill4Item != null)
            MySkill4Item.SetDialogSkillItem(_skillSystem.GetSkillById(normalSkills[3].id.ToString()), MySkill4, canvas);

        // 默认选中第一个技能
        SelectSkillItem(0);

        isClosing = false;
        isActive = true;

        // 显示动画
        StartCoroutine(ShowAnimation());
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
    /// 技能项被点击
    /// </summary>
    private void OnSkillItemClicked(int index)
    {
        if (isClosing || index < 0 || index >= skillItemBg.Count)
            return;

        SelectSkillItem(index);
    }

    /// <summary>
    /// 选中技能项
    /// </summary>
    private void SelectSkillItem(int index)
    {
        if (index >= 0 && index < skillItemBg.Count)
        {
            currentSelectedIndex = index;

            // 设置选中状态（背景图）
            if (skillItemBg[index] != null)
                skillItemBg[index].SetDialogSkillBg(selectBgSprite);
        }

        for (int i = 0; i < skillItemBg.Count; i++)
        {
            if (i == index || skillItemBg[i] == null)
                continue;
            skillItemBg[i].SetDialogSkillBg(skillBgSprite);
        }
    }

    /// <summary>
    /// 设置按钮事件（带选中下标回调）- 修复：使用UnityAction类型
    /// </summary>
    public void SetButtonEvents(UnityAction<int> confirmActionWithIndex, UnityAction cancelAction)
    {
        onConfirmWithIndex = confirmActionWithIndex;
        onCancel = cancelAction;
    }

    /// <summary>
    /// 设置按钮事件（兼容旧版本）- 修复：使用UnityAction类型
    /// </summary>
    public void SetButtonEvents(UnityAction confirmAction, UnityAction cancelAction)
    {
        onConfirmWithIndex = (index) => confirmAction?.Invoke();
        onCancel = cancelAction;
    }

    private void OnConfirmClick()
    {
        if (isClosing || !isActive) return;

        // 检查是否有选中的技能
        if (currentSelectedIndex == -1)
        {
            SeerGoLogger.LogWarning("请先选择一个技能进行替换！");
            return;
        }

        isClosing = true;

        // 将选中的技能下标传递给回调
        onConfirmWithIndex?.Invoke(currentSelectedIndex);
    }

    private void OnCancelClick()
    {
        if (isClosing || !isActive) return;

        isClosing = true;
        onCancel?.Invoke();
    }

    /// <summary>
    /// 移除所有按钮监听器
    /// </summary>
    private void RemoveAllButtonListeners()
    {
        if (ReplaceButton != null)
            ReplaceButton.onClick.RemoveAllListeners();

        if (cancelButton != null)
            cancelButton.onClick.RemoveAllListeners();
    }

    private void OnDestroy()
    {
        RemoveAllButtonListeners();
        onConfirmWithIndex = null;
        onCancel = null;
        onCloseCallback = null;
    }
}