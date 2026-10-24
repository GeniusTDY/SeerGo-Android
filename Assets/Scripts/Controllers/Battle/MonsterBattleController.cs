using QFramework;
using SeerGoGame.Buffs;
using SeerGoGame.Commands;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static PetPrefabConfigManager;
using Random = UnityEngine.Random;

/// <summary>
/// 敌方战斗控制器 - 继承自BattleEntityController统一管理Buff和动画
/// </summary>
public class MonsterBattleController : BattleEntityController
{
    [Header("UI References")]
    public TextMeshProUGUI monsterName;
    public TextMeshProUGUI level;
    public TextMeshProUGUI monsterHpText;
    public Image hp;
    public Image MonsterPetImage;
    public PlayerBattleController playerBattleController;

    [Header("BuffUI")]
    public GameObject monsterBuffsPanel;
    public GameObject monsterStatePanel;

    [Header("伤害动画脚本")]
    public DamagePopup damagePopup;

    private IPetInfoSystem petInfoSystem;
    private IPetAttributeCalcSystem _petAttributeCalcSystem;
    private BuffSystem buffSystem;  // Buff系统
    private int _currentPetId;

    private static readonly string BasePath = ResourcePathBuilder.GetSpritesPetFightImagePath();

    // 使用列表存储精灵数据，避免相同ID导致覆盖
    private List<PetData> _monstersDataList = new List<PetData>();

    public PetData _currentMonsterData;

    private ISkillSystem skillSystem;

    private bool _isProcessingDamage = false;

    protected override void Start()
    {
        base.Start(); // 调用基类初始化
        petInfoSystem = this.GetSystem<IPetInfoSystem>();
        _petAttributeCalcSystem = this.GetSystem<IPetAttributeCalcSystem>();
        skillSystem = this.GetSystem<ISkillSystem>();
        buffSystem = this.GetSystem<BuffSystem>();

        this.RegisterEvent<UseItemSuccessEvent>(OnItemUseSuccess).UnRegisterWhenCurrentSceneUnloaded();
    }

    private void OnItemUseSuccess(UseItemSuccessEvent @event)
    {
        // 更新UI
        UpdateHpUI();
    }

    public void InitMonsterData(List<Pet> pets)
    {
        _monstersDataList.Clear();

        for (int i = 0; i < pets.Count; i++)
        {
            Pet pet = pets[i];
            PetData newMonsterData = new PetData();

            newMonsterData.Id = pet.Id.ToInt();
            MonsterData monsterBaseData = petInfoSystem.GetMonsterById(newMonsterData.Id);
            newMonsterData.Basic.Name = monsterBaseData.DefName;
            newMonsterData.Basic.Gender = monsterBaseData.Gender;
            newMonsterData.Basic.Personality = Random.Range(1, 25);
            newMonsterData.Basic.Attributes = monsterBaseData.Type;
            newMonsterData.Talent.IV = Random.Range(1, 33);
            newMonsterData.Exp.Level = pet.Level;

            string loopSkill = pet.LoopSkill;
            string[] skillArray = loopSkill.Split(',');

            foreach (string skill in skillArray)
            {
                // 技能信息
                SkillData skillData = skillSystem.GetSkillById(skill);
                EquippedSkill equippedSkill = new EquippedSkill(
                    skill.ToInt(),
                    skillData.MaxPP,
                    skillData.MaxPP);
                newMonsterData.Skills.NormalSkill.Add(equippedSkill);
            }

            _petAttributeCalcSystem.CalculateAllStats(newMonsterData);
            // 自定义数值
            if (pet.Status.Hp != 0)
            {
                newMonsterData.CurrentStatus.Hp = pet.Status.Hp;
                newMonsterData.CurrentStatus.MaxHp = pet.Status.Hp;
            }

            _monstersDataList.Add(newMonsterData);
        }
    }

    public void SetMonsterData(List<Pet> pet, Dictionary<string, GameObject> petsAction, int petIndex = 0)
    {
        if (petIndex < 0 || petIndex >= _monstersDataList.Count)
        {
            SeerGoLogger.LogError($"精灵索引{petIndex}无效，总精灵数: {_monstersDataList.Count}");
            return;
        }

        PetData currentMonsterData = _monstersDataList[petIndex];
        _currentMonsterData = currentMonsterData;

        monsterName.text = currentMonsterData.Basic.Name;
        level.text = currentMonsterData.Exp.Level > 100 ? "??" : currentMonsterData.Exp.Level.ToString();

        int maxHp = GetMaxHp();
        currentMonsterData.CurrentStatus.Hp = Mathf.Clamp(currentMonsterData.CurrentStatus.Hp, 0, maxHp);
        monsterHpText.text = $"{currentMonsterData.CurrentStatus.Hp}/{maxHp}";
        hp.fillAmount = (float)currentMonsterData.CurrentStatus.Hp / (float)maxHp;

        MonsterPetImage.sprite = ResourcesManager.Instance.Load<Sprite>($"{BasePath}/Pet_{currentMonsterData.Id}/1");

        base._currentPetsAction = petsAction; // 访问基类字段
        _currentPetId = currentMonsterData.Id;

        // 初始buff
        List<InitBuffData> buffList = pet[petIndex].GetInitBuffList();
        if (buffList.Count > 0)
        {
            foreach (var buffData in buffList)
            {
                var parameters = new Dictionary<string, object>();
                if (!string.IsNullOrEmpty(buffData.Duration))
                {
                    parameters["RemainingTurns"] = buffData.Duration;
                }
                if (!string.IsNullOrEmpty(buffData.EffectValue))
                {
                    parameters["EffectValue"] = float.Parse(buffData.EffectValue);
                }

                SeerGoLogger.Log($"初始化Buff: {buffData.Id}, Duration={buffData.Duration ?? "默认"}, EffectValue={buffData.EffectValue ?? "默认"}");
                buffSystem.ApplyBuff(this, playerBattleController, buffData.Id, parameters.Count > 0 ? parameters : null);
            }
        }
    }

    // 播放技能动画
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
        this.SendCommand(new BattleAttackHitedCommand("Monster"));
    }

    // 提供给BattleManager的公共方法
    public List<EquippedSkill> GetCurrentMonsterSkills()
    {
        if (_currentMonsterData != null)
        {
            return _currentMonsterData.Skills.NormalSkill;
        }
        return new List<EquippedSkill>();
    }

    // 获取当前怪物数据（用于PP扣减）
    public PetData GetCurrentMonsterData()
    {
        return _currentMonsterData;
    }

    // 获取有PP的技能列表（返回技能ID和对应索引的元组）
    public List<(int skillId, int index)> GetAvailableSkills()
    {
        var availableSkills = new List<(int, int)>();

        if (_currentMonsterData == null) return availableSkills;

        for (int i = 0; i < _currentMonsterData.Skills.NormalSkill.Count; i++)
        {
            // 检查PP值
            if (i < _currentMonsterData.Skills.NormalSkill.Count &&
                _currentMonsterData.Skills.NormalSkill[i].currentPP > 0)
            {
                availableSkills.Add((_currentMonsterData.Skills.NormalSkill[i].id, i));
            }
        }

        return availableSkills;
    }

    public int GetCurrentPetId()
    {
        return _currentPetId;
    }

    #region 抽象方法实现
    public override void UpdateHpUI()
    {
        if (_currentMonsterData == null || hp == null) return;

        int maxHp = GetMaxHp();
        // 当最大生命被降低时， 确保当前生命值不会超过最大生命值
        int currentHp = Mathf.Clamp(_currentMonsterData.CurrentStatus.Hp, 0, maxHp);

        float hpRatio = (float)currentHp / (float)maxHp;
        hp.fillAmount = Mathf.Clamp01(hpRatio);
        monsterHpText.text = $"{currentHp}/{maxHp}";
    }

    public override string GetEntityName() => "敌方精灵";

    public override PetData GetCurrentPetData() => _currentMonsterData;

    protected override Transform GetBuffPanelTransform() => monsterBuffsPanel.transform;

    protected override Transform GetStatePanelTransform() => monsterStatePanel.transform;

    protected override void OnDefeat()
    {
        this.SendCommand(new MonsterPetDefeatedCommand(0));
    }
    #endregion

    // 统一处理伤害的私有方法
    private void ApplyDamage(int damage, PetData targetMonster, Color inColor, Color outlineColor, bool isSkillDamage = false, string prefix = "-")
    {
        if (targetMonster.CurrentStatus.Hp <= 0) return;
        if (_isProcessingDamage)
            return;

        // 标记开始处理伤害
        _isProcessingDamage = true;
        try
        {
            // SetMonsterHp中特有的clamp处理
            int processedDamage = isSkillDamage
            ? Mathf.Clamp(damage, int.MinValue, int.MaxValue)
            : damage;

            // 判断回血或者扣血
            int hpChange = prefix == "-" ? -processedDamage : processedDamage;
            int newHp = Mathf.Clamp(targetMonster.CurrentStatus.Hp + hpChange, 0, GetMaxHp());

            // 判断自身是否锁血，指定技能才能造成伤害
            if (SkillOnlyDamageBuffEffect.IsImmuneToSkillOnlyDamage(this) && damage >= targetMonster.CurrentStatus.Hp)
            {
                newHp = 1;
            }

            targetMonster.CurrentStatus.Hp = newHp;

            // 统一的伤害显示逻辑
            if (damage >= 0 || damage == -1)
            {
                if (prefix == "+")
                {
                    inColor = DamagePopup.HealColor;
                    outlineColor = DamagePopup.HealOutlineColor;
                }
                damagePopup.SetupMonster(processedDamage, inColor, outlineColor, prefix).PlayMonster();
            }

            UpdateHpUI();

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

    // 原TakeDamage方法
    public override void TakeDamage(int damage, string prefix = "-")
    {
        ApplyDamage(
            damage,
            _currentMonsterData,
            DamagePopup.BuffDamageInColor,
            DamagePopup.BuffDamageOutlineColor,
            false,
            prefix
        );
    }

    // 原SetMonsterHp方法
    public void SetMonsterHp(int damage, PetData petData, string prefix = "-")
    {
        // 未命中时不修改HP，只显示MISS
        if (damage == -1)
        {
            damagePopup.SetupMonster(damage, DamagePopup.SkillDamageInColor, DamagePopup.SkillDamageOutlineColor, prefix).PlayMonster();
            return;
        }
        ApplyDamage(
            damage,
            petData,
            DamagePopup.SkillDamageInColor,
            DamagePopup.SkillDamageOutlineColor,
            true,
            prefix
        );
    }

    // 获取所有怪物数据列表
    public List<PetData> GetAllMonsterDataList()
    {
        return _monstersDataList;
    }

    public PetData GetMonsterDataListByIndex(int index)
    {
        return _monstersDataList[index];
    }

}