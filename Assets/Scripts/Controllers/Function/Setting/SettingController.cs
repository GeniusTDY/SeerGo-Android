using QFramework;
using SeerGoGame.Controller;
using SeerGoGame.Events;
using SeerGoGame.Interfaces;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

public class SettingController : BaseController
{
    [Header("音量设置")]
    [SerializeField] private Button CloseButton;
    [SerializeField] private TextMeshProUGUI AudioNumText;
    [SerializeField] private Button SaveButton;
    [SerializeField] private Slider AudioSlider;

    [SerializeField] private TextMeshProUGUI BattleAudioNumText;
    [SerializeField] private Slider BattleAudioSlider;

    [Header("对战倍速")]
    [SerializeField] private ToggleGroup toggleGroup;
    // 存放所有单选框
    private List<Toggle> toggleList = new List<Toggle>();

    private IArchiveModel archiveModel;
    private IArchiveSystem archiveSystem;

    void Start()
    {
        archiveModel = this.GetModel<IArchiveModel>();
        archiveSystem = this.GetSystem<IArchiveSystem>();

        // 滑动条变化时更新文字显示
        AudioSlider.onValueChanged.AddListener(OnSliderValueChanged);
        BattleAudioSlider.onValueChanged.AddListener(OnBattleSliderValueChanged);

        // 初始化倍速Toggle列表（收集子Toggle并绑定ToggleGroup实现互斥）
        if (toggleGroup != null)
        {
            toggleList.AddRange(toggleGroup.GetComponentsInChildren<Toggle>(true));
            foreach (var toggle in toggleList)
            {
                toggle.group = toggleGroup; // 显式绑定组，确保互斥
            }
        }

        // 保存按钮
        SaveButton.onClick.AddListener(OnSave);

        // 关闭按钮
        CloseButton.onClick.AddListener(OnClose);

        this.RegisterEvent<SettingOpenEvent>(OnSettingOpenEvent)
            .UnRegisterWhenGameObjectDestroyed(gameObject);
    }


    /// <summary>
    /// 打开设置面板时，从存档读取当前音量并刷新UI
    /// </summary>
    private void OnSettingOpenEvent(SettingOpenEvent @event)
    {
        RefreshUIFromArchive();
    }

    /// <summary>
    /// 从存档读取音量、倍速等设置并刷新UI（存档存1-10，Slider用0-1）
    /// </summary>
    private void RefreshUIFromArchive()
    {
        int bgmVolume = Mathf.Clamp(archiveModel.CurrentData.SettingsData.BGMVolume, 1, 10);
        float sliderValue = (bgmVolume - 1) / 9f;
        AudioSlider.SetValueWithoutNotify(sliderValue);
        AudioNumText.text = bgmVolume.ToString();

        int btBgmVolume = Mathf.Clamp(archiveModel.CurrentData.SettingsData.BattleBGMVolume, 1, 10);
        float btSliderValue = (btBgmVolume - 1) / 9f;
        BattleAudioSlider.SetValueWithoutNotify(btSliderValue);
        BattleAudioNumText.text = btBgmVolume.ToString();

        // 刷新倍速Toggle
        int speed = archiveModel.CurrentData.SettingsData.BattleAnimSpeed;
        if (speed < 1 || speed > toggleList.Count) speed = 1;
        for (int i = 0; i < toggleList.Count; i++)
            toggleList[i].SetIsOnWithoutNotify(i == speed - 1);
    }

    /// <summary>
    /// 滑动条回调：实时显示1-10
    /// </summary>
    private void OnSliderValueChanged(float value)
    {
        AudioNumText.text = (Mathf.RoundToInt(value * 9f) + 1).ToString();
    }
    private void OnBattleSliderValueChanged(float value)
    {
        BattleAudioNumText.text = (Mathf.RoundToInt(value * 9f) + 1).ToString();
    }

    /// <summary>
    /// 保存按钮：将音量(1-10)、倍速写入存档并实时应用
    /// </summary>
    private void OnSave()
    {
        int volumeInt = Mathf.RoundToInt(AudioSlider.value * 9f) + 1;
        int battleVolumeInt = Mathf.RoundToInt(BattleAudioSlider.value * 9f) + 1;

        // 读取倍速选择（1/2/3）
        int selectedSpeed = 1;
        for (int i = 0; i < toggleList.Count; i++)
        {
            if (toggleList[i].isOn) { selectedSpeed = i + 1; break; }
        }

        // 写入存档
        archiveModel.CurrentData.SettingsData.BGMVolume = volumeInt;
        archiveModel.CurrentData.SettingsData.BattleBGMVolume = battleVolumeInt;
        archiveModel.CurrentData.SettingsData.BattleAnimSpeed = selectedSpeed;

        // 持久化保存
        archiveSystem.SaveGame("autosave0", archiveModel.CurrentData);

        // 实时应用
        AudioManager.Instance.SetVolume(AudioSlider.value);
        BattleEntityController.BattleAnimationSpeed = selectedSpeed;

        Dialog1Manager.Instance.ShowPopupOnTransform(
           FunctionPanelLoader.Instance.GetCanvas().transform,
          "提示",
          "保存成功!",
          onConfirm: () => OnClose()
      );
    }

    private void OnClose()
    {
        FunctionPanelLoader.Instance.ClosePanel("Setting/SettingPanel");
    }
}