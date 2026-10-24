using QFramework;
using SeerGoGame.Commands;
using SeerGoGame.Controller;
using SeerGoGame.Interfaces;
using UnityEngine;
using UnityEngine.UI;

public class NoNoFunctionManager : BaseController
{
    [SerializeField] private Canvas uiCanvas;
    public SpriteRenderer[] spriteRenderers;

    [Header("UI 按钮引用")]
    [SerializeField] private Button recoveryButton;
    [SerializeField] private Button homeButton;
    [SerializeField] private Button expButton;
    [SerializeField] private Button moleculeButton;
    [SerializeField] private Button soulFormingButton;

    private IUISystem _UISystem;
    private IArchiveModel _archiveModel;
    private IArchiveSystem _archiveSystem;

    void Start()
    {
        _UISystem = this.GetSystem<IUISystem>();
        _archiveModel = this.GetModel<IArchiveModel>();
        _archiveSystem = this.GetSystem<IArchiveSystem>();

        // 设置按钮点击事件
        SetupButtonEvents();
    }

    private void SetupButtonEvents()
    {
        if (recoveryButton != null)
            recoveryButton.onClick.AddListener(OnRecoveryClicked);

        if (homeButton != null)
            homeButton.onClick.AddListener(OnHomeClicked);

        if (expButton != null)
            expButton.onClick.AddListener(OnExpClicked);

        if (moleculeButton != null)
            moleculeButton.onClick.AddListener(OnMoleculeClicked);

        if (soulFormingButton != null)
            soulFormingButton.onClick.AddListener(OnSoulFormingClicked);
    }

    private void OnMouseEnter()
    {
        SetSpritesState(true);
    }

    private void OnMouseExit()
    {
        SetSpritesState(false);
    }

    private void SetSpritesState(bool state)
    {
        if (spriteRenderers == null) return;

        foreach (SpriteRenderer sr in spriteRenderers)
        {
            if (sr != null)
            {
                sr.enabled = state;
            }
        }
    }

    // 按钮点击事件处理方法
    private void OnRecoveryClicked()
    {
        RecoveryPetMethod();
    }

    private void OnHomeClicked()
    {
        FunctionPanelLoader.Instance.LoadPanel("PetStroage/PetStroagePanel", (panel) =>
        {
            if (panel != null)
            {
                this.SendCommand(new ChangePetStroageCommand());
            }
        });
    }

    private void OnExpClicked()
    {
        _UISystem.SetUICanvasGroup("NoNoFuncCanvas", true);
        this.SendCommand(new ChangePetExpCommand());
    }

    private void OnMoleculeClicked()
    {
        FunctionPanelLoader.Instance.LoadPanel("Molecule/MoleculePanel", (panel) =>
        {
            if (panel != null)
            {
                this.SendCommand(new ChangeMoleculeCommand());
            }
        });
    }

    private void OnSoulFormingClicked()
    {
        FunctionPanelLoader.Instance.LoadPanel("Fusion/SoulFormingPanel", (panel) =>
        {
            if (panel != null)
            {
                this.SendCommand(new ChangeSoulFormingCommand());
            }
        });
    }

    // 公共方法，供外部调用
    public void RecoveryPetMethod()
    {
        _archiveModel.CurrentData.PetBag.ForEach(petData =>
        {
            // 恢复体力
            petData.CurrentStatus.Hp = petData.CurrentStatus.MaxHp;
            // 恢复PP
            petData.Skills.NormalSkill.ForEach(skill => skill.currentPP = skill.maxPP);
        });
        // 保存
        _archiveSystem.SaveGame("autosave0", _archiveModel.CurrentData);

        Dialog1Manager.Instance.ShowPopupOnTransform(uiCanvas.transform, "提示", "你的精灵已经完全恢复了!", null);
    }
}