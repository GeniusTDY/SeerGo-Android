using PixeLadder.SimpleTooltip;
using QFramework;
using SeerGoGame.Controller;
using SeerGoGame.Events;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;
using Random = UnityEngine.Random;

public class MoleculeController : BaseController
{
    [SerializeField] private Button PreButton, NextButton, ConfirmButton, CloseButton;
    [SerializeField] private Image SoulImage;

    private IItemModel itemModel;
    private IArchiveModel archiveModel;
    private IPetInfoSystem peInfoSystem;
    private IPetAttributeCalcSystem peAttributeCalcSystem;
    private ISkillSystem skillSystem;
    private IArchiveSystem archiveSystem;
    private IExpConfigSystem expConfigSystem;

    private List<int> currentItems = new List<int>();
    private int currentPage, totalPages;

    void Start()
    {
        this.RegisterEvent<MoleculeChangeEventOpenEvent>(OnMoleculeOpenEvent)
            .UnRegisterWhenGameObjectDestroyed(gameObject);

        itemModel = this.GetModel<IItemModel>();
        archiveModel = this.GetModel<IArchiveModel>();
        peInfoSystem = this.GetSystem<IPetInfoSystem>();
        peAttributeCalcSystem = this.GetSystem<IPetAttributeCalcSystem>();
        skillSystem = this.GetSystem<ISkillSystem>();
        archiveSystem = this.GetSystem<IArchiveSystem>();
        expConfigSystem = this.GetSystem<IExpConfigSystem>();

        PreButton.onClick.AddListener(() => ChangePage(-1));
        NextButton.onClick.AddListener(() => ChangePage(1));
        ConfirmButton.onClick.AddListener(() => OnConfirmClick());
        CloseButton.onClick.AddListener(() => OnCloseClick());
    }

    private void OnCloseClick()
    {
        FunctionPanelLoader.Instance.ClosePanel("Molecule/MoleculePanel");
    }

    private void OnConfirmClick()
    {
        if (currentItems.Count == 0 || currentPage > currentItems.Count)
        {
            SeerGoLogger.LogWarning("当前没有可使用的精元");
            return;
        }

        int itemId = currentItems[currentPage - 1];
        int petId = MoleculeMapping.GetPetIdByItemId(itemId);

        if (petId <= 0)
        {
            SeerGoLogger.LogError($"未知的精元ID:{itemId}，无法获取对应的精灵");
            return;
        }

        // 检查精元数量
        var itemData = archiveModel.CurrentData.ItemStorage.Find(item => item.Id == itemId);
        if (itemData == null || itemData.Num <= 0)
        {
            SeerGoLogger.LogWarning("精元数量不足");
            return;
        }

        ProcessGetPetReward(petId, itemId);
    }

    private void OnMoleculeOpenEvent(MoleculeChangeEventOpenEvent @event) => LoadData();

    private void LoadData()
    {
        currentItems.Clear();
        foreach (var item in archiveModel.CurrentData.ItemStorage)
        {
            var config = itemModel.GetById(item.Id);
            if (config != null && itemModel.IsContainScope(config, "INMOLECULE") && item.Num > 0)
                currentItems.Add(item.Id);
        }

        totalPages = Mathf.Max(1, currentItems.Count);
        currentPage = totalPages > 0 ? 1 : 0;
        UpdatePage();
    }

    private void UpdatePage()
    {
        PreButton.interactable = currentPage > 1;
        NextButton.interactable = currentPage < totalPages;
        ConfirmButton.interactable = currentItems.Count > 0;

        if (currentItems.Count == 0 || currentPage > currentItems.Count)
        {
            SoulImage.sprite = ResourcesManager.Instance.Load<Sprite>($"Sprites/Function/Molecule/Slot");
            ShowTooltip("空");
            return;
        }

        int itemId = currentItems[currentPage - 1];
        var sprite = ResourcesManager.Instance.Load<Sprite>($"Sprites/Item/{itemId}");

        if (sprite != null)
        {
            SoulImage.sprite = sprite;
        }
        else
        {
            SeerGoLogger.LogError($"无法加载物品图片:Sprites/Item/{itemId}");
            SoulImage.sprite = ResourcesManager.Instance.Load<Sprite>($"Sprites/Function/Molecule/Slot");
            ShowTooltip("空");
        }

        var config = itemModel.GetById(itemId);
        if (config != null)
        {
            int num = archiveModel.CurrentData.ItemStorage.Find(item => item.Id == itemId)?.Num ?? 0;
            ShowTooltip(config.description + "\n数量:" + num);
        }
        else
        {
            ShowTooltip("未知物品");
        }
    }

    private void ChangePage(int direction)
    {
        if (currentItems.Count == 0) return;

        int newPage = currentPage + direction;
        if (newPage >= 1 && newPage <= totalPages)
        {
            currentPage = newPage;
            UpdatePage();
        }
    }

    private void ShowTooltip(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            TooltipTrigger.AddTooltip(
                SoulImage.gameObject,
                null,
                text,
                Color.yellow,
                null,
                FunctionPanelLoader.Instance.GetCanvas());
        }
    }

    public void ProcessGetPetReward(int petId, int itemId)
    {
        MonsterData monsterBaseData = peInfoSystem.GetMonsterById(petId);
        if (monsterBaseData == null)
        {
            SeerGoLogger.LogWarning($"无法获取怪物数据，petId:{petId}");
            return;
        }

        PetData newMonsterData = new PetData();

        newMonsterData.Id = petId;
        newMonsterData.Basic.Id = petId;
        newMonsterData.Basic.Name = monsterBaseData.DefName;
        newMonsterData.Basic.Gender = monsterBaseData.Gender;
        newMonsterData.Basic.Personality = Random.Range(1, 25);
        newMonsterData.Basic.Attributes = monsterBaseData.Type;
        newMonsterData.Basic.GetPetDate = DateTime.Now.ToString("yyyy-MM-dd");

        newMonsterData.Feature.Id = petId;
        newMonsterData.Exp.Id = petId;
        newMonsterData.Exp.Level = 1;
        newMonsterData.Exp.CurrentExp = expConfigSystem.GetExpValue(1, monsterBaseData.GrowthType);

        newMonsterData.Talent.Id = petId;
        newMonsterData.Talent.IV = Random.Range(1, 32);

        newMonsterData.Skills.Id = petId;
        List<LearnableMove> learnableMoves = peInfoSystem.GetLearnableMoves(petId);
        if (learnableMoves != null)
        {
            for (int i = 0; i < learnableMoves.Count; i++)
            {
                if (learnableMoves[i].LearningLv == 1)
                {
                    newMonsterData.Skills.OwnSkillId.Add(learnableMoves[i].ID);
                    EquippedSkill equippedSkill = new EquippedSkill();
                    equippedSkill.id = learnableMoves[i].ID;
                    equippedSkill.maxPP = skillSystem.GetSkillById(learnableMoves[i].ID.ToString()).MaxPP;
                    equippedSkill.currentPP = equippedSkill.maxPP;
                    newMonsterData.Skills.NormalSkill.Add(equippedSkill);
                }
            }
        }

        // 计算宠物属性
        peAttributeCalcSystem.CalculateAllStats(newMonsterData);

        // 减少精元数量
        var itemData = archiveModel.CurrentData.ItemStorage.Find(item => item.Id == itemId);
        if (itemData != null)
        {
            itemData.Num--;
            if (itemData.Num <= 0)
            {
                archiveModel.CurrentData.ItemStorage.Remove(itemData);
            }
        }

        bool isBagFull = archiveModel.CurrentData.PetBag.Count >= 6;

        if (archiveModel.CurrentData.PetBag.Count < 6)
        {
            archiveModel.CurrentData.PetBag.Add(newMonsterData);
        }
        else
        {
            archiveModel.CurrentData.PetStorage.Add(newMonsterData);
        }

        // 保存存档
        archiveSystem.SaveGame("autosave0", archiveModel.CurrentData);

        // 弹窗提示
        Dialog1GetPetManager.Instance.ShowPopupOnTransform(
            FunctionPanelLoader.Instance.GetCanvas().transform,
            isBagFull ?
                $"恭喜你获得了<color=green>{newMonsterData.Basic.Name}</color>，背包已满，已放入仓库" :
                $"恭喜你获得了<color=green>{newMonsterData.Basic.Name}</color>，你可以点击右下方的精灵按钮",
            newMonsterData,
            isBagFull
        );

        // 重新加载数据
        LoadData();
    }

    private void OnDestroy()
    {
        PreButton.onClick.RemoveAllListeners();
        NextButton.onClick.RemoveAllListeners();
        ConfirmButton.onClick.RemoveAllListeners();
        CloseButton.onClick.RemoveAllListeners();
    }
}