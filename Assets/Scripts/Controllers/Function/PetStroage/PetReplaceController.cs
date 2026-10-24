using QFramework;
using SeerGoGame.Commands;
using SeerGoGame.Controller;
using SeerGoGame.Events;
using SeerGoGame.Interfaces;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PetReplaceController : BaseController
{
    [Header("UI")]
    [SerializeField] private GameObject PetItemPanel; // 精灵面板容器
    [SerializeField] private Button ReplaceButton, CloseButton; // 替换按钮和关闭按钮
    [SerializeField] private GameObject PetItem; // 精灵UI项预制体

    // 模型和系统接口
    private IArchiveModel archiveModel;
    private IArchiveSystem archiveSystem;

    // 宠物项列表和选择状态
    private List<PetBagItem> petItems = new List<PetBagItem>();
    private int selectedStroagePetIndex = -1; // 仓库选中的精灵索引
    private int selectedBagPetIndex = -1;     // 背包选中的精灵索引

    // 颜色常量定义
    private Color NormalColor = Color.white;    // 默认背景色
    private Color SelectedColor = Color.yellow; // 选中背景色

    void Start()
    {
        // 注册打开替换面板事件
        this.RegisterEvent<PetReplaceOpenEvent>(OnPetReplaceOpen).UnRegisterWhenGameObjectDestroyed(gameObject);

        // 获取存档模型和系统
        archiveModel = this.GetModel<IArchiveModel>();
        archiveSystem = this.GetSystem<IArchiveSystem>();

        // 设置按钮点击事件
        CloseButton.onClick.AddListener(() => FunctionPanelLoader.Instance.ClosePanel("PetStroage/ReplacePetPanel"));
        ReplaceButton.onClick.AddListener(OnReplaceButtonClick);
    }

    // 替换按钮点击事件处理
    private void OnReplaceButtonClick()
    {
        // 检查是否选择了仓库和背包的宠物
        if (selectedStroagePetIndex == -1 || selectedBagPetIndex == -1)
        {
            Dialog1Manager.Instance.ShowPopupOnTransform(FunctionPanelLoader.Instance.transform, "提示", "请选择要替换的精灵");
            return;
        }

        // 获取选中的仓库宠物信息
        var petName = archiveModel.CurrentData.PetStorage[selectedStroagePetIndex].Basic.Name;

        // 交换背包和仓库中的宠物数据
        (archiveModel.CurrentData.PetBag[selectedBagPetIndex],
         archiveModel.CurrentData.PetStorage[selectedStroagePetIndex]) =
        (archiveModel.CurrentData.PetStorage[selectedStroagePetIndex],
         archiveModel.CurrentData.PetBag[selectedBagPetIndex]);

        // 保存存档
        archiveSystem.SaveGame("autosave0", archiveModel.CurrentData);

        // 关闭替换面板
        FunctionPanelLoader.Instance.ClosePanel("PetStroage/ReplacePetPanel");

        // 显示获取宠物提示
        Dialog1GetPetManager.Instance.ShowPopupOnTransform(FunctionPanelLoader.Instance.transform,
            $"<color=red>{petName}</color>已经放入你的精灵背包",
            archiveModel.CurrentData.PetBag[selectedBagPetIndex],
            false,
            () =>
            {
                // 刷新宠物仓库和背包显示
                this.SendCommand(new ChangePetStroageCommand());
                this.SendCommand<ChangePetBagCommand>();
            });
    }

    // 宠物替换面板打开事件
    private void OnPetReplaceOpen(PetReplaceOpenEvent e)
    {
        if (e.petIndex == -1) return;

        // 记录仓库中选中的宠物索引
        selectedStroagePetIndex = e.petIndex;
        selectedBagPetIndex = -1; // 重置背包选择状态

        // 加载背包宠物数据
        LoadData();
    }

    // 加载背包宠物数据
    private void LoadData()
    {
        // 清理现有UI项
        foreach (Transform child in PetItemPanel.transform) Destroy(child.gameObject);
        petItems.Clear();

        // 获取当前背包中的宠物列表
        var petBag = archiveModel.CurrentData.PetBag;

        // 为每个背包宠物创建UI项
        for (int i = 0; i < petBag.Count; i++)
        {
            CreateItem(petBag[i], i);
        }
    }

    // 创建单个宠物UI项
    private void CreateItem(PetData petData, int index)
    {
        // 实例化宠物项预制体
        var petItem = Instantiate(PetItem, PetItemPanel.transform);
        var petUI = petItem.GetComponent<PetBagItem>();
        petUI.SetPetBagItem(petData);

        // 添加或获取按钮组件
        if (!petItem.TryGetComponent<Button>(out var itemButton))
        {
            itemButton = petItem.AddComponent<Button>();
        }

        // 设置点击事件
        itemButton.onClick.AddListener(() => OnPetItemClicked(index));

        // 保存宠物项引用
        petItems.Add(petUI);

        // 设置初始背景色
        SetItemColor(petItem, NormalColor);
    }

    // 宠物项点击事件处理
    private void OnPetItemClicked(int index)
    {
        // 记录选中的背包宠物索引
        selectedBagPetIndex = index;

        // 更新所有宠物项的显示状态
        for (int i = 0; i < petItems.Count; i++)
        {
            SetItemColor(petItems[i].gameObject, i == index ? SelectedColor : NormalColor);
        }
    }

    // 设置宠物项背景颜色
    private void SetItemColor(GameObject item, Color color)
    {
        if (item.TryGetComponent<Image>(out var image))
        {
            image.color = color;
        }
    }
}