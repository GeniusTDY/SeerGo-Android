using QFramework;
using SeerGoGame.Buffs;
using SeerGoGame.Commands;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Utilities;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

/// <summary>
/// 玩家战斗控制器 - 继承自BattleEntityController统一管理Buff和动画
/// </summary>
public class PlayerBattleController : BattleEntityController
{
    [Header("UI References")]
    public TextMeshProUGUI playerName;
    public TextMeshProUGUI level;
    public TextMeshProUGUI playerHpText;
    public Image hp;
    public GameObject petSkillPanle;
    public GameObject skillPrefab;
    public Image PlayerPetImage;
    public TextMeshProUGUI petName;
    public Image PetImage;
    public Canvas uiCanvasCache;

    [Header("BuffUI")]
    public GameObject playerBuffsPanel;
    public GameObject playerStatePanel;

    [Header("伤害动画脚本")]
    public DamagePopup damagePopup;

    private ISkillSystem skillSystem;
    private IItemModel itemModel;

    private static readonly string BasePath = ResourcePathBuilder.GetSpritesPetFightImagePath();

    // 技能项缓存
    private Dictionary<int, PlayerSkillsPrefabItem> _skillItemCache = new Dictionary<int, PlayerSkillsPrefabItem>();

    public PetData _currentPetData;

    private bool _isProcessingDamage = false;

    protected override void Start()
    {
        base.Start(); // 调用基类初始化
        this.RegisterEvent<UseItemSuccessEvent>(OnItemUseSuccess).UnRegisterWhenCurrentSceneUnloaded();
        skillSystem = this.GetSystem<ISkillSystem>();
        itemModel = this.GetModel<IItemModel>();
    }

    //  物品使用成功事件处理
    private void OnItemUseSuccess(UseItemSuccessEvent @event)
    {
        /*  // 更新UI
          RefreshSkillButtons();
          UpdateHpUI();
          ItemConfig itemConfig = itemModel.GetById(@event.ItemId);
          if (itemConfig.type == "healHP")
          {
              damagePopup.SetupPlayer((int)itemConfig.effectValue, DamagePopup.HealColor, DamagePopup.HealOutlineColor, "+")
                  .PlayPlayer();
          }*/
    }

    public void SetPetsData(ArchiveData archiveData, Dictionary<string, GameObject> petsAction, int petIndex = 0)
    {
        playerName.text = archiveData.Nickname;
        var petBags = archiveData.PetBag;

        if (petIndex < 0 || petIndex >= petBags.Count)
        {
            SeerGoLogger.LogError($"宠物索引{petIndex}无效，总宠物数: {petBags.Count}");
            return;
        }

        var currentPet = petBags[petIndex];
        _currentPetData = currentPet;
        level.text = currentPet.Exp.Level.ToString();
        int maxHp = GetMaxHp();
        currentPet.CurrentStatus.Hp = Mathf.Clamp(currentPet.CurrentStatus.Hp, 0, maxHp);
        hp.fillAmount = (float)currentPet.CurrentStatus.Hp / (float)maxHp;
        playerHpText.text = $"{currentPet.CurrentStatus.Hp}/{maxHp}";

        PlayerPetImage.sprite = ResourcesManager.Instance.Load<Sprite>($"{BasePath}/Pet_{currentPet.Id}/1");
        petName.text = currentPet.Basic.Name;
        PetImage.sprite = ResourcesManager.Instance.Load<Sprite>($"{BasePath}/Pet_{currentPet.Id}/1");

        base._currentPetsAction = petsAction; // 访问基类字段
        ClearSkillItems();
        CreateSkillItems(currentPet.Skills.NormalSkill, currentPet.Id, _currentPetData);
    }

    private void ClearSkillItems()
    {
        foreach (Transform child in petSkillPanle.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    private void CreateSkillItems(List<EquippedSkill> skills, int petId, PetData petData)
    {
        for (int i = 0; i < skills.Count; i++)
        {
            CreateSkillItem(skills[i], i, petId, petData);
        }
    }

    private void CreateSkillItem(EquippedSkill skill, int index, int petId, PetData petData)
    {
        PlayerSkillsPrefabItem skillItem = null;

        // 检查缓存
        if (_skillItemCache.ContainsKey(skill.id))
        {
            skillItem = _skillItemCache[skill.id];
            skillItem.gameObject.SetActive(true);
        }
        else
        {
            var skillGameObject = Instantiate(skillPrefab, petSkillPanle.transform);
            skillItem = skillGameObject.GetComponent<PlayerSkillsPrefabItem>();
            _skillItemCache[skill.id] = skillItem;
        }

        var skillData = skillSystem.GetSkillById(skill.id.ToString());
        var button = skillItem.gameObject.GetComponent<Button>();

        // 检查PP值，设置按钮是否可用
        int currentPP = petData.Skills.NormalSkill[index].currentPP;
        bool canUse = currentPP > 0;
        button.interactable = canUse;

        // 清除旧的事件监听
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => OnSkillSelected(skillData, index, petId));

        skillItem.SetSkillData(skillData, petData, uiCanvasCache);
    }

    private void OnSkillSelected(SkillData skillData, int index, int petId)
    {
        // 再次检查PP值（双重保险）
        if (_currentPetData.Skills.NormalSkill[index].currentPP <= 0)
        {
            SeerGoLogger.LogWarning($"技能 {skillData.Name} PP值不足，无法使用！");
            return;
        }

        this.SendCommand(new BattleUseSkillCommand("Player", petId, "USE_SKILL", skillData, index));
    }

    public IEnumerator PlaySkillAnimationCoroutine(SkillData skillData, int petId)
    {
        if (!_skillAnimationMap.TryGetValue(skillData.Category, out var animationSuffix))
            yield break;

        string animationKey = $"{petId}_{animationSuffix}";
        _hitFrameTriggered = false;

        if (_currentAnimationCoroutine != null)
        {
            StopCoroutine(_currentAnimationCoroutine);
        }

        _currentAnimationCoroutine = StartCoroutine(PlayAnimationOnce(_currentPetsAction, petId, animationKey, animationSuffix));
        yield return _currentAnimationCoroutine;
    }

    protected override void OnHitFrameTriggered()
    {
        this.SendCommand(new BattleAttackHitedCommand("Player"));
    }

    // 刷新技能按钮的可用状态（用于PP值变化后更新UI）
    public void RefreshSkillButtons()
    {
        if (_currentPetData == null) return;

        for (int i = 0; i < _currentPetData.Skills.NormalSkill.Count; i++)
        {
            int skillId = _currentPetData.Skills.NormalSkill[i].id;
            if (_skillItemCache.TryGetValue(skillId, out var skillItem))
            {
                var button = skillItem.gameObject.GetComponent<Button>();
                int currentPP = _currentPetData.Skills.NormalSkill[i].currentPP;
                button.interactable = currentPP > 0;

                // 刷新PP显示
                var skillData = skillSystem.GetSkillById(skillId.ToString());
                skillItem.SetSkillData(skillData, _currentPetData, uiCanvasCache);
            }
        }
    }

    #region 抽象方法实现
    public override void UpdateHpUI()
    {
        if (_currentPetData == null || hp == null) return;
        int maxHp = GetMaxHp();
        // 当最大生命被降低时， 确保当前生命值不会超过最大生命值
        int currentHp = Mathf.Clamp(_currentPetData.CurrentStatus.Hp, 0, maxHp);

        float hpRatio = (float)currentHp / (float)maxHp;
        hp.fillAmount = Mathf.Clamp01(hpRatio);
        playerHpText.text = $"{currentHp}/{maxHp}";
    }

    public override string GetEntityName() => "玩家精灵";

    public override PetData GetCurrentPetData() => _currentPetData;
    //  获取技能缓存
    public Dictionary<int, PlayerSkillsPrefabItem> GetPlyaerSkillCache() => _skillItemCache;

    protected override Transform GetBuffPanelTransform() => playerBuffsPanel.transform;

    protected override Transform GetStatePanelTransform() => playerStatePanel.transform;

    protected override void OnDefeat()
    {
        this.SendCommand(new PlayerPetDefeatedCommand(0));
    }
    #endregion

    public override void TakeDamage(int damage, string prefix = "-")
    {
        ApplyHpChange(_currentPetData, damage, true, prefix);
    }

    public void SetPlayerHp(int damage, PetData petData)
    {
        // 未命中时不修改HP，只显示MISS
        if (damage == -1)
        {
            damagePopup.SetupPlayer(damage, DamagePopup.SkillDamageInColor, DamagePopup.SkillDamageOutlineColor, "-").PlayPlayer();
            return;
        }
        ApplyHpChange(petData, damage, false);
    }

    private void ApplyHpChange(PetData petData, int damage, bool isBuffDamage, string prefix = "-")
    {
        // 基础校验
        if (petData?.CurrentStatus == null || hp == null || petData.CurrentStatus.Hp <= 0)
            return;

        if (_isProcessingDamage)
            return;

        // 标记开始处理伤害
        _isProcessingDamage = true;
        try
        {
            int oldHp = petData.CurrentStatus.Hp;

            // 计算新HP
            damage = Mathf.Clamp(damage, int.MinValue, int.MaxValue);


            int hpChange = prefix == "-" ? -damage : damage;
            int newHp = Mathf.Clamp(petData.CurrentStatus.Hp + hpChange, 0, GetMaxHp());

            // 判断自身是否锁血，指定技能才能造成伤害
            if (SkillOnlyDamageBuffEffect.IsImmuneToSkillOnlyDamage(this) && damage >= oldHp)
            {
                newHp = 1;
            }

            // 显示伤害数字
            if (damage >= 0 || damage == -1)
            {
                var colors = isBuffDamage
                    ? (DamagePopup.BuffDamageInColor, DamagePopup.BuffDamageOutlineColor)
                    : (DamagePopup.SkillDamageInColor, DamagePopup.SkillDamageOutlineColor);
                if (prefix == "+")
                {
                    colors = (DamagePopup.HealColor, DamagePopup.HealOutlineColor);
                }
                damagePopup.SetupPlayer(damage, colors.Item1, colors.Item2, prefix).PlayPlayer();
            }

            // 更新数据
            petData.CurrentStatus.Hp = newHp;
            // 更新UI
            float hpRatio = (float)newHp / GetMaxHp();
            hp.fillAmount = Mathf.Clamp01(hpRatio);
            playerHpText.text = $"{newHp}/{GetMaxHp()}";

            // 死亡处理
            if (newHp <= 0)
            {
                OnDefeat();
            }
        }
        finally
        {
            // 确保无论成功与否，最后都重置标志位
            _isProcessingDamage = false;
        }
    }

}