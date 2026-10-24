using PixeLadder.SimpleTooltip;
using QFramework;
using SeerGoGame.Commands;
using SeerGoGame.Controller;
using SeerGoGame.Events;
using SeerGoGame.Interfaces;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using UnityEngine;
using Button = UnityEngine.UI.Button;

public class PetBagController : BaseController
{
    [Header("左半背包")]
    public GameObject PetBagItemPrefab;
    public RectTransform content;
    public GameObject rightPetBagInfo;

    [Header("背包按钮")]
    public Button close; //关闭
    public Button Btn_engraving; //刻印
    public Button Btn_Stroage; //仓库
    public Button Btn_Skill; //技能学习
    public Button Btn_Skin; //皮肤
    public Button Btn_Follow; //精灵跟随
    public Button Btn_Shouxuan; //设置首选
    public Button Btn_IntoStroage; //放入仓库
    public Button Btn_Tujian; //图鉴
    public Button Btn_Prop; //道具
    public Button Btn_Recovery; //恢复

    [Header("按钮控制器")]
    public GameObject petItemPanel;//道具

    private IArchiveModel archiveModel;
    private IUISystem _UISystem;
    private IArchiveSystem _archiveSystem;
    private RightPetBagInfo _rightPetBagInfoScript;

    // 使用Queue实现简单对象池，避免频繁Instantiate/Destroy
    private Queue<PetBagItem> _petBagItemPool = new Queue<PetBagItem>();
    private List<PetBagItem> _activePetBagItems = new List<PetBagItem>();

    private Sprite _selectedBgSprite;
    private Sprite _normalBgSprite;
    private int _currentSelectedIndex = -1;
    private Canvas _uiCanvasCache;

    // 添加当前选中宠物的引用
    private PetData _currentSelectedPet;

    void Start()
    {
        _rightPetBagInfoScript = rightPetBagInfo.GetComponent<RightPetBagInfo>();
        archiveModel = this.GetModel<IArchiveModel>();
        _UISystem = this.GetSystem<IUISystem>();
        _archiveSystem = this.GetSystem<IArchiveSystem>();
        InitializeCanvas();
        PreloadResources();
        SetupEventListeners();
        InitButtonTooltips();
    }

    private void InitButtonTooltips()
    {
        var tooltipConfigs = new (Button button, string title)[]
        {
        (Btn_engraving, "精灵刻印(暂未开放)"),
        (Btn_Stroage, "精灵仓库"),
        (Btn_Skill, "技能学习"),
        (Btn_Skin, "皮肤(暂未开放)"),
        (Btn_Follow, "精灵跟随(暂未开放)"),
        (Btn_Shouxuan, "精灵首选"),
        (Btn_IntoStroage, "放入仓库"),
        (Btn_Tujian, "精灵图鉴"),
        (Btn_Prop, "精灵道具"),
        (Btn_Recovery, "精灵恢复")
        };

        foreach (var config in tooltipConfigs)
        {
            if (config.button != null)
            {
                TooltipTrigger.AddTooltip(
                    config.button.gameObject,
                    null,
                    config.title,
                    Color.yellow,
                    null,
                    _uiCanvasCache
                );
            }
        }
    }

    private void InitializeCanvas()
    {
        var canvasObj = GameObject.Find("PetBagCanvas");
        _uiCanvasCache = canvasObj?.GetComponent<Canvas>();

        if (_uiCanvasCache == null)
            SeerGoLogger.LogWarning("在场景中未找到名为'PetBagCanvas'的游戏对象或其上无Canvas组件。");

        _UISystem.SetUICanvasGroup("PetItemPanel", false);
    }

    private void PreloadResources()
    {
        _selectedBgSprite = ResourcesManager.Instance.Load<Sprite>($"Sprites/PetBag/0");
        _normalBgSprite = ResourcesManager.Instance.Load<Sprite>($"Sprites/PetBag/1");

        if (_selectedBgSprite == null || _normalBgSprite == null)
            SeerGoLogger.LogError("宠物背包背景图片加载失败！");
    }

    private void SetupEventListeners()
    {
        this.RegisterEvent<PetBagOpenEvent>(OnPetBagOpenEvent)
            .UnRegisterWhenGameObjectDestroyed(gameObject);
        close.onClick.AddListener(OnCloseButtonClick);
        Btn_Shouxuan.onClick.AddListener(OnShouxuanButtonClick);
        Btn_Prop.onClick.AddListener(OnPropButtonClick);
        Btn_Skill.onClick.AddListener(OnSkillButtonClick);
        Btn_Stroage.onClick.AddListener(OnStroageButtonClick);
        Btn_IntoStroage.onClick.AddListener(OnIntoStroageButtonClick);
        Btn_Recovery.onClick.AddListener(OnRecoveryButtonClick);
    }

    private void OnRecoveryButtonClick()
    {
        archiveModel.CurrentData.PetBag.ForEach(petData =>
        {
            // 恢复体力
            petData.CurrentStatus.Hp = petData.CurrentStatus.MaxHp;
            // 恢复PP
            petData.Skills.NormalSkill.ForEach(skill => skill.currentPP = skill.maxPP);
        });
        // 保存
        _archiveSystem.SaveGame("autosave0", archiveModel.CurrentData);
        this.SendCommand(new ChangePetBagCommand());

        Dialog1Manager.Instance.ShowPopupOnTransform(_uiCanvasCache.transform, "提示", "你的精灵已经完全恢复了!", null);
    }

    // 放入仓库
    private void OnIntoStroageButtonClick()
    {
        if (_currentSelectedIndex == -1 || _currentSelectedPet == null)
        {
            Dialog1Manager.Instance.ShowPopupOnTransform(_uiCanvasCache.transform, "提示", "请选择一个精灵!", null);
            return;
        }
        if (archiveModel.CurrentData.PetBag.Count <= 1)
        {
            Dialog1Manager.Instance.ShowPopupOnTransform(_uiCanvasCache.transform, "提示", "背包至少要保留一只精灵!", null);
            return;
        }
        archiveModel.CurrentData.PetStorage.Add(_currentSelectedPet);
        archiveModel.CurrentData.PetBag.Remove(_currentSelectedPet);
        // 保存
        _archiveSystem.SaveGame("autosave0", archiveModel.CurrentData);

        // 移除选中精灵后，重置选中状态，避免后续LoadPetBag使用过期索引
        var removedPet = _currentSelectedPet;
        _currentSelectedIndex = -1;
        _currentSelectedPet = null;

        Dialog1GetPetManager.Instance.ShowPopupOnTransform(FunctionPanelLoader.Instance.transform,
                $"<color=red>{removedPet.Basic.Name}</color>已经放入你的仓库",
                removedPet,
                true,
                () =>
                {
                    this.SendCommand<ChangePetBagCommand>();
                });

    }
    // 精灵仓库
    private void OnStroageButtonClick()
    {
        FunctionPanelLoader.Instance.LoadPanel("PetStroage/PetStroagePanel", (panel) =>
        {
            if (panel != null)
            {
                this.SendCommand(new ChangePetStroageCommand());
            }
        });
    }

    private void OnSkillButtonClick()
    {
        FunctionPanelLoader.Instance.LoadPanel("SkillRelace/SkillReplacePanel", (panel) =>
        {
            if (panel != null)
            {
                this.SendCommand(new ChangeSkillReplaceCommand());
            }
        });
    }

    private void OnCloseButtonClick()
    {
        _UISystem.SetUICanvasGroup("PetBagCanvas", false);
        _UISystem.SetUICanvasGroup("PetItemPanel", false);
    }

    private void OnPetBagOpenEvent(PetBagOpenEvent @event) => LoadPetBag();

    private void OnPropButtonClick()
    {
        //道具
        _UISystem.SetUICanvasGroup("PetItemPanel", true);
        this.SendCommand(new ChangePetBagItemCommand());
    }


    private void OnShouxuanButtonClick()
    {
        List<PetData> petBag = archiveModel.CurrentData.PetBag;

        if (petBag.Count <= 1)
            return;

        if (_currentSelectedIndex < 0 || _currentSelectedIndex >= petBag.Count)
            return;

        if (_currentSelectedIndex > 0)
        {
            // 交换首选和当前选中位置
            PetData temp = petBag[0];
            petBag[0] = petBag[_currentSelectedIndex];
            petBag[_currentSelectedIndex] = temp;

            _currentSelectedIndex = 0;

            // 存档
            this.SendCommand(new SaveArchiveDataCommand(archiveModel.CurrentData));

            // 完全重建背包UI，确保数据与显示一致
            LoadPetBag();
        }
        else
        {
            // 如果已经是首选，只需要确保右侧信息正确显示
            _currentSelectedPet = petBag[0];
            UpdateRightPetBagInfo(_currentSelectedPet, 0);
        }
    }


    private void LoadPetBag()
    {
        PreloadResources();
        ClearPetBagItems();

        List<PetData> petBag = archiveModel.CurrentData.PetBag;
        for (int i = 0; i < petBag.Count; i++)
        {
            CreatePetBagItem(petBag[i], i);
        }

        // 校验当前选中状态，防止外部操作（如仓库替换）导致索引过期
        ValidateSelection(petBag);

        // 选中第一个
        if (_activePetBagItems.Count > 0)
        {
            int indexToSelect = 0;
            if (_currentSelectedIndex >= 0 && _currentSelectedIndex < petBag.Count)
            {
                // 如果之前有选中的宠物，尝试保持选中
                indexToSelect = _currentSelectedIndex;
            }

            // 确保索引有效
            if (indexToSelect >= petBag.Count)
            {
                indexToSelect = 0;
            }

            SelectPet(petBag[indexToSelect], indexToSelect);
        }
    }

    /// <summary>
    /// 校验当前选中状态，处理外部数据变更导致索引过期的情况
    /// </summary>
    private void ValidateSelection(List<PetData> petBag)
    {
        if (_currentSelectedPet != null)
        {
            int actualIndex = petBag.IndexOf(_currentSelectedPet);
            if (actualIndex >= 0)
            {
                // 精灵仍在背包中，更新为正确索引
                _currentSelectedIndex = actualIndex;
            }
            else
            {
                // 精灵已被移除（如放入仓库），清除选中状态
                _currentSelectedIndex = -1;
                _currentSelectedPet = null;
            }
        }
        else if (_currentSelectedIndex >= petBag.Count)
        {
            // 没有选中精灵引用但索引越界，重置
            _currentSelectedIndex = -1;
        }
    }

    private void UpdatePetBagDisplay()
    {
        List<PetData> petBag = archiveModel.CurrentData.PetBag;

        // 重新设置所有活动项的宠物数据
        for (int i = 0; i < _activePetBagItems.Count; i++)
        {
            if (i < petBag.Count)
            {
                _activePetBagItems[i].SetPetBagItem(petBag[i]);

                // 更新点击事件，直接使用petBgButton字段避免找错按钮
                _activePetBagItems[i].petBgButton.onClick.RemoveAllListeners();
                PetData pet = petBag[i];
                int index = i;
                _activePetBagItems[i].petBgButton.onClick.AddListener(() => OnPetSelected(pet, index));
            }
        }

        // 重新设置选中状态
        if (_activePetBagItems.Count > 0 && _currentSelectedIndex >= 0 && _currentSelectedIndex < petBag.Count)
        {
            SelectPet(petBag[_currentSelectedIndex], _currentSelectedIndex);
        }
    }

    private void CreatePetBagItem(PetData petData, int index)
    {
        var itemUI = GetPetBagItemFromPool();
        itemUI.SetPetBagItem(petData);
        _activePetBagItems.Add(itemUI);

        // 强制矫正Transform层级顺序，确保布局与实际数据索引一致
        itemUI.transform.SetSiblingIndex(index);

        // 清理旧监听器并添加新监听器，避免重复注册
        itemUI.petBgButton.onClick.RemoveAllListeners();
        itemUI.petBgButton.onClick.AddListener(() => OnPetSelected(petData, index));
    }

    private PetBagItem GetPetBagItemFromPool()
    {
        PetBagItem item;

        if (_petBagItemPool.Count > 0)
        {
            item = _petBagItemPool.Dequeue();
            item.gameObject.SetActive(true);
        }
        else
        {
            var itemObj = Instantiate(PetBagItemPrefab, content);
            item = itemObj.GetComponent<PetBagItem>();
        }

        return item;
    }

    private void ReturnPetBagItemToPool(PetBagItem item)
    {
        item.gameObject.SetActive(false);
        _petBagItemPool.Enqueue(item);
    }

    private void ClearPetBagItems()
    {
        foreach (var item in _activePetBagItems)
        {
            ReturnPetBagItemToPool(item);
        }
        _activePetBagItems.Clear();
    }

    private void OnPetSelected(PetData petData, int index)
    {
        if (index == _currentSelectedIndex) return;
        SelectPet(petData, index);
    }

    private void SelectPet(PetData petData, int index)
    {
        // 更新所有项的背景
        for (int i = 0; i < _activePetBagItems.Count; i++)
        {
            if (_activePetBagItems[i]?.petBgImg != null)
            {
                _activePetBagItems[i].petBgImg.sprite = (i == index) ? _selectedBgSprite : _normalBgSprite;
            }
        }

        _currentSelectedIndex = index;
        _currentSelectedPet = petData;
        UpdateRightPetBagInfo(petData, index);
    }

    private void UpdateRightPetBagInfo(PetData petData, int index)
    {
        // 清空技能区域
        for (int i = 0; i < 4; i++)
        {
            var skillBg = _rightPetBagInfoScript.skillBgs[i];
            if (skillBg != null && skillBg.transform != null)
            {
                foreach (Transform child in skillBg.transform)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        _rightPetBagInfoScript.SetRightBagInfo(petData, index, _uiCanvasCache);
    }

    // 公共方法
    public void Refresh() => LoadPetBag();

    public PetData GetSelectedPet()
    {
        var petBag = archiveModel.CurrentData.PetBag;
        return (_currentSelectedIndex >= 0 && _currentSelectedIndex < petBag.Count) ?
            petBag[_currentSelectedIndex] : null;
    }

    public int GetSelectedIndex() => _currentSelectedIndex;

    private void OnDestroy()
    {
        // 清理对象池
        foreach (var item in _petBagItemPool)
        {
            if (item != null && item.gameObject != null)
                Destroy(item.gameObject);
        }
        _petBagItemPool.Clear();
        _activePetBagItems.Clear();
    }
}