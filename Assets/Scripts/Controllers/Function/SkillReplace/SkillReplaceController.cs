using QFramework;
using SeerGoGame.Commands;
using SeerGoGame.Controller;
using SeerGoGame.Events;
using SeerGoGame.Interfaces;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

public class SkillReplaceController : BaseController
{
    [Header("UI")]
    [SerializeField] private GameObject PetsPanel;
    [SerializeField] private Button ConfirmButton;
    [SerializeField] private Button CloseButton;

    [Header("颜色设置")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;

    [Header("预制体")]
    [SerializeField] private GameObject ItemPrefab;

    private IArchiveModel archiveModel;
    private PetData currentSelectPet = null;
    private int currentSelectPetIndex = -1;
    private List<Image> petItemImages = new List<Image>();

    void Start()
    {
        this.RegisterEvent<SkillReplaceOpenEvent>(OnSkillReplaceOpen)
                  .UnRegisterWhenGameObjectDestroyed(gameObject);

        archiveModel = this.GetModel<IArchiveModel>();
        ConfirmButton.onClick.AddListener(OnConfirmButtonClick);
        CloseButton.onClick.AddListener(OnCloseButtonClick);
    }

    private void OnSkillReplaceOpen(SkillReplaceOpenEvent @event)
    {
        LoadPetsData();
    }

    private void LoadPetsData()
    {
        // 清空现有内容
        foreach (Transform child in PetsPanel.transform)
        {
            Destroy(child.gameObject);
        }
        petItemImages.Clear();

        List<PetData> petBag = archiveModel.CurrentData.PetBag;
        for (int i = 0; i < petBag.Count; i++)
        {
            CreateItem(petBag[i], i);
        }
    }

    private void CreateItem(PetData petData, int index)
    {
        GameObject petItem = Instantiate(ItemPrefab, PetsPanel.transform);

        // 获取Image组件
        Image itemImage = petItem.GetComponent<Image>();
        if (itemImage == null)
        {
            itemImage = petItem.AddComponent<Image>();
        }
        petItemImages.Add(itemImage);

        // 设置UI
        PetBagItem petUI = petItem.GetComponent<PetBagItem>();
        if (petUI != null)
        {
            petUI.SetPetBagItem(petData);
        }

        // 添加点击事件
        Button petButton = petItem.GetComponentInChildren<Button>();
        if (petButton != null)
        {
            petButton.onClick.AddListener(() => OnPetSelected(petData, index));
        }

        // 设置默认颜色
        itemImage.color = normalColor;
    }

    private void OnPetSelected(PetData petData, int index)
    {
        // 重置所有item为默认颜色
        for (int i = 0; i < petItemImages.Count; i++)
        {
            if (petItemImages[i] != null)
            {
                petItemImages[i].color = normalColor;
            }
        }

        // 设置选中item为金色
        if (index >= 0 && index < petItemImages.Count && petItemImages[index] != null)
        {
            petItemImages[index].color = selectedColor;
        }

        currentSelectPet = petData;
        currentSelectPetIndex = index;
    }

    private void OnCloseButtonClick()
    {
        // 重置所有item为默认颜色
        for (int i = 0; i < petItemImages.Count; i++)
        {
            if (petItemImages[i] != null)
            {
                petItemImages[i].color = normalColor;
            }
        }

        currentSelectPet = null;
        currentSelectPetIndex = -1;
        FunctionPanelLoader.Instance.ClosePanel("SkillRelace/SkillReplacePanel");
    }

    private void OnConfirmButtonClick()
    {
        if (currentSelectPet == null)
        {
            Dialog1Manager.Instance.ShowPopupOnTransform(FunctionPanelLoader.Instance.transform, "", "请先选择要替换技能的宠物");
            return;
        }
        if (currentSelectPet.Skills.OwnSkillId.Count <= 4)
        {
            Dialog1Manager.Instance.ShowPopupOnTransform(FunctionPanelLoader.Instance.transform, "", "你的这只精灵没有可以唤醒的技能");
            return;
        }

        FunctionPanelLoader.Instance.ClosePanel("SkillRelace/SkillReplacePanel");
        FunctionPanelLoader.Instance.LoadPanel("SkillRelace/SkillReplaceMainPanel", (panel) =>
        {
            if (panel != null)
            {
                this.SendCommand(new ChangeSkillReplaceMainCommand(currentSelectPet,currentSelectPetIndex));
                currentSelectPet = null;
                currentSelectPetIndex = -1;
            }
        });

    }
}