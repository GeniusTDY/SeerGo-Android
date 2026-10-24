using QFramework;
using SeerGoGame.Buffs;
using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using UnityEngine;

/**
 *  战斗相关计算帮助类
 */
public class BattleCalcUtilsHelper : AbstractSystem
{
    private IAttributeSystem attributeSystem;
    private ITypeRelationSystem typeRelationSystem;

    private const float DEFAULT_SELF_DAMAGE_BUFF = 1f;   // 己方默认增伤百分比
    private const float DEFAULT_TARGET_DAMAGE_DEBUFF = 0f; // 对方默认减伤百分比

    protected override void OnInit()
    {
        attributeSystem = this.GetSystem<IAttributeSystem>();
        typeRelationSystem = this.GetSystem<ITypeRelationSystem>();
    }

    // 战斗伤害计算
    public int GetPlayerPetDamage(PlayerBattleController caster, MonsterBattleController target, SkillData casterSkillData, int category)
    {
        (int attack, int defense) = category switch
        {
            1 when StateAssimilationBuffEffect.IsDefAssimilation(target) => (caster.GetAtk(), caster.GetDef()),// 防御同化
            2 when StateAssimilationBuffEffect.IsAtkAssimilation(target) => (target.GetAtk(), target.GetDef()),// 物攻同化
            1 => (caster.GetAtk(), target.GetDef()), // 物攻：普通攻击 + 普通防御
            2 => (caster.GetSpAtk(), target.GetSpDef()),// 特攻：特殊攻击 + 特殊防御
            _ => (caster.GetSpAtk(), target.GetSpDef()) // 特攻：特殊攻击 + 特殊防御（默认分支）
        };

        RoundContext.Current.PlayerType = caster.GetCurrentPetData().Basic.Attributes;
        RoundContext.Current.EnemyType = target.GetCurrentMonsterData().Basic.Attributes;

        return CalculateDamage(
            caster.GetCurrentPetData(),
            attack,
            defense,
            target.GetCurrentMonsterData().Basic.Attributes,
            casterSkillData,
            RoundContext.Current.PlayerSkillPower,
            DEFAULT_SELF_DAMAGE_BUFF,
            DEFAULT_TARGET_DAMAGE_DEBUFF,
            caster,
            target
        );
    }

    public int GetMonsterPetDamage(MonsterBattleController caster, PlayerBattleController target, SkillData casterSkillData, int category)
    {
        (int attack, int defense) = category switch
        {
            1 when StateAssimilationBuffEffect.IsDefAssimilation(target) => (caster.GetAtk(), caster.GetDef()),// 防御同化
            2 when StateAssimilationBuffEffect.IsAtkAssimilation(target) => (target.GetAtk(), target.GetDef()),// 物攻同化
            1 => (caster.GetAtk(), target.GetDef()), // 物攻：普通攻击 + 普通防御
            2 => (caster.GetSpAtk(), target.GetSpDef()),// 特攻：特殊攻击 + 特殊防御
            _ => (caster.GetSpAtk(), target.GetSpDef()) // 特攻：特殊攻击 + 特殊防御（默认分支）
        };

        RoundContext.Current.EnemyType = caster.GetCurrentMonsterData().Basic.Attributes;
        RoundContext.Current.PlayerType = target.GetCurrentPetData().Basic.Attributes;

        return CalculateDamage(
            caster.GetCurrentMonsterData(),
            attack,
            defense,
            target.GetCurrentPetData().Basic.Attributes,
            casterSkillData,
            RoundContext.Current.EnemySkillPower,
            DEFAULT_SELF_DAMAGE_BUFF,
            DEFAULT_TARGET_DAMAGE_DEBUFF,
            caster,
            target
        );
    }

    private int CalculateDamage(
        PetData attackerData,
        int attackerAttack,
        int defenderDefense,
        int defenderAttributeId,
        SkillData skillData,
        int skillPower,
        float casterAddition,
        float targetReduction,
        IBuffable caster,
        IBuffable target)
    {
        if (skillPower == 0) { return 0; }

        // 1. 等级系数
        float levelCoefficient = (0.4f * attackerData.Exp.Level + 2) / 50;
        // 2. 浮动范围
        float range = Random.Range(0.85f, 1f);
        // 3. 攻击/防御比值
        float attackDefenseRatio = (float)attackerAttack / defenderDefense;
        // 4. 属性buff处理(属性反转)
        int counts = TypeReverseBuffEffect.GetReversedTypesCounts(caster, target);
        // 双方初始属性id
        int attackerTypeId = attackerData.Basic.Attributes;
        int defenderTypeId = defenderAttributeId;
        // 交换属性
        if (counts == 1)
            (attackerTypeId, defenderTypeId) = (defenderTypeId, attackerTypeId);

        // 5.属性同化处理
        if (TypeCopyBuffEffect.HasTypeCopyBuff(caster))
            attackerTypeId = defenderTypeId;
        else if (TypeCopyBuffEffect.HasTypeCopyBuff(target))
            defenderTypeId = attackerTypeId;

        // 全局记录属性
        (RoundContext.Current.PlayerType, RoundContext.Current.EnemyType) = caster switch
        {
            PlayerBattleController => (attackerTypeId, defenderTypeId),
            MonsterBattleController => (defenderTypeId, attackerTypeId),
            _ => (RoundContext.Current.PlayerType, RoundContext.Current.EnemyType) // 保持原值
        };

        // 6. 属性加成（STAB）：攻击方自身属性包含技能属性时触发，兼容双属性
        float attributeAddition = IsSameTypeIncludingDouble(attackerTypeId, skillData.Type) ? 1.5f : 1f;
        // 7. 克制倍数：按技能属性（攻击方）→ 防守方属性计算，支持单/双属性
        float typeRelationMultiplier = CalcMultiAttributeMultiplier(skillData.Type, defenderTypeId);

        // 8. 伤害计算
        float damage = (levelCoefficient * skillPower * attackDefenseRatio + 2) *
                      range * attributeAddition * typeRelationMultiplier *
                      casterAddition * (1 - targetReduction);

        return Mathf.FloorToInt(damage);
    }

    #region 多属性克制系数计算

    /// <summary>
    /// 计算多属性克制系数（支持单→单、双→单、单→双、双→双）
    /// 用技能属性（攻击方）→ 防守方属性查克制关系
    /// </summary>
    private float CalcMultiAttributeMultiplier(int skillTypeId, int defenderTypeId)
    {
        var attackerComponents = GetSingleAttrIds(skillTypeId);
        var defenderComponents = GetSingleAttrIds(defenderTypeId);

        int atkCount = attackerComponents.Count;
        int defCount = defenderComponents.Count;

        if (atkCount == 1 && defCount == 1)
        {
            // 单 → 单
            return GetSingleToSingleMult(attackerComponents[0], defenderComponents[0]);
        }
        else if (atkCount == 2 && defCount == 1)
        {
            // 双 → 单
            return CalcDoubleToSingle(attackerComponents, defenderComponents[0]);
        }
        else if (atkCount == 1 && defCount == 2)
        {
            // 单 → 双
            return CalcSingleToDouble(attackerComponents[0], defenderComponents);
        }
        else
        {
            // 双 → 双
            return CalcDoubleToDouble(attackerComponents, defenderComponents);
        }
    }

    /// <summary>将属性ID拆成单属性ID列表（双属性拆为2个单属性ID，单属性直接返回1个）</summary>
    private List<int> GetSingleAttrIds(int attrId)
    {
        if (attributeSystem.IsDoubleAttribute(attrId))
            return attributeSystem.GetAttributeComponents(attrId);
        return new List<int> { attrId };
    }

    /// <summary>单 → 单：直接查克制表</summary>
    private float GetSingleToSingleMult(int attackerSingleId, int defenderSingleId)
    {
        var atkType = attributeSystem.GetAttributeById(attackerSingleId);
        var defType = attributeSystem.GetAttributeById(defenderSingleId);
        return atkType != null && defType != null
            ? typeRelationSystem.GetDamageMultiplier(atkType.en, defType.en)
            : 1f;
    }

    /// <summary>双 → 单：A・B → C</summary>
    private float CalcDoubleToSingle(List<int> atkComps, int defSingle)
    {
        float x1 = GetSingleToSingleMult(atkComps[0], defSingle);
        float x2 = GetSingleToSingleMult(atkComps[1], defSingle);
        return CombineMultipliers(x1, x2);
    }

    /// <summary>单 → 双：A → B・C</summary>
    private float CalcSingleToDouble(int atkSingle, List<int> defComps)
    {
        float x1 = GetSingleToSingleMult(atkSingle, defComps[0]);
        float x2 = GetSingleToSingleMult(atkSingle, defComps[1]);
        return CombineMultipliers(x1, x2);
    }

    /// <summary>双 → 双：A・B → C・D = (f(A,B→C) + f(A,B→D)) / 2</summary>
    private float CalcDoubleToDouble(List<int> atkComps, List<int> defComps)
    {
        float d1 = CalcDoubleToSingle(atkComps, defComps[0]);
        float d2 = CalcDoubleToSingle(atkComps, defComps[1]);
        return (d1 + d2) / 2f;
    }

    /// <summary>
    /// 双属性克制公式：
    /// 两个都是2 → 4；
    /// 至少一个是0 → (x₁+x₂)/4；
    /// 其他 → (x₁+x₂)/2
    /// </summary>
    private static float CombineMultipliers(float x1, float x2)
    {
        if (x1 == 2f && x2 == 2f) return 4f;
        if (x1 == 0f || x2 == 0f) return (x1 + x2) / 4f;
        return (x1 + x2) / 2f;
    }

    /// <summary>判断 petAttrId（可能是双属性）是否包含 targetSingleId</summary>
    private bool IsSameTypeIncludingDouble(int petAttrId, int targetSingleId)
    {
        if (!attributeSystem.IsDoubleAttribute(petAttrId))
            return petAttrId == targetSingleId;
        var comps = attributeSystem.GetAttributeComponents(petAttrId);
        return comps.Count > 0 && comps.Contains(targetSingleId);
    }

    #endregion

    // 精灵捕捉概率计算

    public bool CalcCatchRate(PetData petData, float itemCoefficient, int baseCatchRate, float statusCorrection = 1.0f)
    {
        SeerGoLogger.Log("计算精灵捕捉概率" + itemCoefficient);
        // 如果基础捕获率为0，直接返回失败
        if (baseCatchRate == 0) return false;

        // 无敌胶囊
        if (itemCoefficient >= 255) return true;

        // 计算HP系数
        float hpFactor = (3f * petData.CurrentStatus.MaxHp - 2f * petData.CurrentStatus.Hp)
                 / (3f * petData.CurrentStatus.MaxHp);
        SeerGoLogger.Log("计算精灵捕捉概率" + hpFactor);
        // 计算修正捕获率 = HP因子 × 基础捕获率 × 道具修正 × 状态修正
        float modifiedCatchRate = hpFactor * baseCatchRate * itemCoefficient * statusCorrection;
        SeerGoLogger.Log("计算精灵捕捉概率" + modifiedCatchRate);
        // 关键判定1：如果修正捕获率MCR ≥ 255，则必定捕获成功
        if (modifiedCatchRate >= 255) return true;

        // finalRate = min(MCR / 255 × 100 %, 100 %)
        int finalRate = Mathf.Min((int)(modifiedCatchRate / 255 * 100), 100);

        // 生成1-100的随机数，如果随机数小于等于finalRate则捕获成功
        int randomValue = Random.Range(1, 101);
        bool isSuccess = randomValue <= finalRate;

        return isSuccess;
    }

}