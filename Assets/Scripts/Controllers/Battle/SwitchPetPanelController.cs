using QFramework;
using SeerGoGame.Commands;
using SeerGoGame.Interfaces;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SwitchPetPanelController : BaseBattleController
{
    [Header("UI组件引用")]
    public GameObject skillPanel;
    [Header("精灵头像框")]
    public Button[] PetsButtons;
    public Image[] ButtonsImages;
    public Image[] PetsImages;

    public TextMeshProUGUI Level;
    public TextMeshProUGUI Name;
    public TextMeshProUGUI HpText;
    public Image HpBar;
    public Button BattleButton;
    public GameObject SkillItemPrefab;

    [Header("按钮不可点击颜色")]
    public Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 1f); // 灰色半透明
    public Color normalColor = Color.white; // 正常颜色

    private const string BUTTON_NORMAL_PATH = "Sprites/Fight/FuncButton/SwitchPet/NormalBg";
    private const string BUTTON_SELECTED_PATH = "Sprites/Fight/FuncButton/SwitchPet/SelectBg";
    private static readonly string BasePath = ResourcePathBuilder.GetSpritesPetFightImagePath();

    private ISkillSystem skillSystem;
    private ArchiveData _archiveData;
    // 当前选中的精灵索引
    private int _currentSelectedPetIndex = 0;
    // 当前上场的精灵索引
    private int _currentActivePetIndex = -1;
    // 当前宠物数量
    private int _currentPetCount = 0;

    // 技能项缓存
    private Dictionary<int, PlayerSwitchPetSkillItem> _skillItemCache = new Dictionary<int, PlayerSwitchPetSkillItem>();

    // 记录按钮是否已初始化
    private bool _buttonsInitialized = false;

    private void Start()
    {
        skillSystem = this.GetSystem<ISkillSystem>();
        InitializePetButtons();
    }

    public void SetData(ArchiveData archiveData, int petIndex = 0)
    {
        if (_currentActivePetIndex == -1)
            _currentActivePetIndex = petIndex;

        _archiveData = archiveData;
        _currentPetCount = archiveData.PetBag.Count;

        ClearSkillItems();
        ResetPetsImage();
        UpdateButtonsInteractable();
        ClickSetData(archiveData, petIndex);
    }

    private void InitializePetButtons()
    {
        if (_buttonsInitialized) return;

        for (int i = 0; i < PetsButtons.Length; i++)
        {
            int index = i;
            PetsButtons[i].onClick.AddListener(() => OnPetSelected(index));
        }
        BattleButton.onClick.AddListener(() => OnBattleButtonClick());

        _buttonsInitialized = true;
    }

    private void UpdateButtonsInteractable()
    {
        for (int i = 0; i < PetsButtons.Length; i++)
        {
            PetsButtons[i].interactable = i < _currentPetCount;
        }
    }

    private void OnBattleButtonClick()
    {
        if (IsSelectedPetAlive() && !IsActivePetIndex())
        {
            _currentActivePetIndex = -1;
            this.SendCommand(new BattleSwitchPetCommand(_currentSelectedPetIndex));
        }
    }

    private void OnPetSelected(int petIndex)
    {
        // 检查索引是否有效
        if (petIndex >= _currentPetCount) return;

        ClearSkillItems();
        ResetPetsImage();
        ClickSetData(_archiveData, petIndex);
    }

    private void ClickSetData(ArchiveData archiveData, int petIndex)
    {
        if (petIndex < 0 || petIndex >= archiveData.PetBag.Count)
        {
            SeerGoLogger.LogError($"宠物索引{petIndex}无效，总宠物数: {archiveData.PetBag.Count}");
            return;
        }

        var selectedPet = archiveData.PetBag[petIndex];

        // 设置选中状态
        ButtonsImages[petIndex].sprite = ResourcesManager.Instance.Load<Sprite>(BUTTON_SELECTED_PATH);
        ButtonsImages[petIndex].gameObject.transform.localScale = new Vector3(1.2f, 1.2f, 1f);

        // 设置所有宠物的头像
        for (int i = 0; i < PetsImages.Length; i++)
        {
            if (i < archiveData.PetBag.Count)
            {
                PetData item = archiveData.PetBag[i];
                PetsImages[i].sprite = ResourcesManager.Instance.Load<Sprite>($"{BasePath}/Pet_{item.Id}/1");
                PetsImages[i].gameObject.SetActive(true);

                if (item.CurrentStatus.Hp <= 0)
                {
                    PetsImages[i].color = disabledColor;
                }
                else
                {
                    PetsImages[i].color = normalColor;
                }
            }
            else
            {
                PetsImages[i].gameObject.SetActive(false);
            }
        }

        CreateSkillItems(selectedPet.Skills.NormalSkill, selectedPet.Id, archiveData.PetBag[petIndex]);

        // 设置宠物信息
        Level.text = "Lv:" + selectedPet.Exp.Level.ToString();
        Name.text = selectedPet.Basic.Name;
        HpText.text = $"{selectedPet.CurrentStatus.Hp}/{selectedPet.CurrentStatus.MaxHp}";

        float amount = (float)selectedPet.CurrentStatus.Hp / (float)selectedPet.CurrentStatus.MaxHp;
        HpBar.fillAmount = amount;

        _currentSelectedPetIndex = petIndex;
        UpdateBattleButtonState(selectedPet);
    }

    private bool IsSelectedPetAlive()
    {
        if (_archiveData == null ||
            _currentSelectedPetIndex < 0 ||
            _currentSelectedPetIndex >= _archiveData.PetBag.Count)
        {
            return false;
        }

        var selectedPet = _archiveData.PetBag[_currentSelectedPetIndex];
        return selectedPet.CurrentStatus.Hp > 0;
    }

    private bool IsActivePetIndex()
    {
        return _currentSelectedPetIndex == _currentActivePetIndex;
    }

    private void UpdateBattleButtonState(PetData selectedPet)
    {
        bool canSwitch = selectedPet.CurrentStatus.Hp > 0 &&
                       _currentActivePetIndex != _currentSelectedPetIndex;

        BattleButton.interactable = canSwitch;

        var buttonImage = BattleButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = canSwitch ? normalColor : disabledColor;
        }

        var buttonText = BattleButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.color = canSwitch ? normalColor : disabledColor;
        }
    }

    private void ResetPetsImage()
    {
        foreach (var item in PetsImages)
        {
            item.gameObject.SetActive(false);
        }

        foreach (var item in ButtonsImages)
        {
            item.sprite = ResourcesManager.Instance.Load<Sprite>(BUTTON_NORMAL_PATH);
            item.gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
        }
    }

    private void ClearSkillItems()
    {
        foreach (Transform child in skillPanel.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    private void CreateSkillItems(List<EquippedSkill> skills, int petId, PetData petData)
    {
        for (int i = 0; i < skills.Count; i++)
        {
            CreateSkillItem(skills[i], i, petData);
        }
    }

    private void CreateSkillItem(EquippedSkill skill, int index, PetData petData)
    {
        PlayerSwitchPetSkillItem skillItem = null;

        if (_skillItemCache.ContainsKey(skill.id))
        {
            skillItem = _skillItemCache[skill.id];
            skillItem.gameObject.SetActive(true);
        }
        else
        {
            var skillGameObject = Instantiate(SkillItemPrefab, skillPanel.transform);
            skillItem = skillGameObject.GetComponent<PlayerSwitchPetSkillItem>();
            _skillItemCache[skill.id] = skillItem;
        }

        var skillData = skillSystem.GetSkillById(skill.id.ToString());
        skillItem.SetSkillData(skillData, petData);
    }

    private void OnDestroy()
    {
        foreach (Button btn in PetsButtons)
        {
            if (btn != null) btn.onClick.RemoveAllListeners();
        }
        if (BattleButton != null) BattleButton.onClick.RemoveAllListeners();

        foreach (var item in _skillItemCache.Values)
        {
            if (item != null) Destroy(item);
        }
    }
}