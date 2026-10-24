using SeerGoGame.Buffs;
using SeerGoGame.Interfaces;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

/// <summary>
/// Buff效果管理器 - 统一管理所有buff效果处理器
/// </summary>
public class BuffEffectManager
{
    private List<IBuffEffectHandler> handlers = new List<IBuffEffectHandler>();

    public BuffEffectManager()
    {
        // 注册所有buff效果处理器
        RegisterHandler(new PoisonBuffEffect());// 中毒
        RegisterHandler(new BurnBuffEffect());// 灼烧
        RegisterHandler(new FreezeBuffEffect());// 冻结
        RegisterHandler(new ParasiticBuffEffect());// 寄生
        RegisterHandler(new StatChangeBuffEffect());// 能力属性变化
        RegisterHandler(new PerishBuffEffect());// 濒死
        RegisterHandler(new GuaranteedCritBuffEffect());// 必暴击
        RegisterHandler(new BlockBuffEffect());// 格挡
        RegisterHandler(new ImmuneBuffEffect());// 免疫
        RegisterHandler(new TypeReverseBuffEffect());// 属性反转
        RegisterHandler(new TypeCopyBuffEffect());// 属性复制
        RegisterHandler(new ControlTypeBuffEffect());// 控制类型异常
        RegisterHandler(new ReflectionBuffEffect());// 反弹伤害
        RegisterHandler(new CriticalChangeBuffEffect());// 暴击带回合数
        RegisterHandler(new MaxHPChangeBuffEffect());// 降低最大生命值
        RegisterHandler(new NearDeathBuffEffect());// 手下留情
        RegisterHandler(new DamageChangeBuffEffect());// 伤害变化减半或者翻倍
        RegisterHandler(new ShieldBuffEffect());// 护盾值抵消伤害
        RegisterHandler(new SkillBlockBuffEffect());// 技能失效
        RegisterHandler(new RecoverHpBuffEffect());// 恢复体力百分比
        RegisterHandler(new RecoverHpBuffEffect1());// 恢复体力
        RegisterHandler(new FixedDamageBonusBuffEffect());// 固伤
        RegisterHandler(new PotionReverseBuffEffect());// 药剂反转
        RegisterHandler(new PhyAttackMissBuffEffect());// 物攻闪避
        RegisterHandler(new CaAttackMissBuffEffect());// 属攻闪避
        RegisterHandler(new AttackMustHitBuffEffect());// 必定命中
        RegisterHandler(new AttackMustFirstBuffEffect());// 必定先手
        RegisterHandler(new DamagePercentRecoverHpBuffEffect());//  恢复伤害百分比体力
        RegisterHandler(new ImmuneSkillBuffEffect());//  技能免疫
        RegisterHandler(new InfinitePpBuffEffect());//  无限PP
        RegisterHandler(new HpRestoreBuffEffect());//  血量低于100时恢复全部体力一次
        RegisterHandler(new StateAssimilationBuffEffect());//  防御，攻击同化
        RegisterHandler(new ShadowBarrierBuffEffect());//  尤纳斯免疫伤害
        RegisterHandler(new SkillOnlyDamageBuffEffect());//  最后一滴血需要指定技能造成伤害
        RegisterHandler(new PrioritySkillBuffEffect());//  先制
        RegisterHandler(new OpponentDamageBuffEffect());//  令对方伤害100倍
        RegisterHandler(new LowHpInstantKillBuffEffect());//  低血秒杀
        RegisterHandler(new LoopTypeSkillDamageBuffEffect());//  水火草技能循环
        RegisterHandler(new StatMirrorBuffEffect());//  属性同步（对手状态变化同步到自己身上）
        RegisterHandler(new GenderDamageBuffEffect());//  性别伤害倍数（对雄性精灵伤害倍数）
        RegisterHandler(new WeakBuffEffect());//  衰弱（每层降低造成伤害的20%）
        RegisterHandler(new FlammableBuffEffect());//  易燃（命中降低30%）
        RegisterHandler(new DamageReductionPerTurnBuffEffect());//  每回合伤害减免
        RegisterHandler(new ConditionalFixedDamageBuffEffect());//  条件固伤（能力下降或异常时对对手造成固伤）
        RegisterHandler(new AttrSkillBlockBuffEffect());//  属性技能封锁
        RegisterHandler(new DamageThresholdStatusBuffEffect());//  受击反伤异常
        RegisterHandler(new TurnHealBuffEffect());//  每回合回血
    }

    public void RegisterHandler(IBuffEffectHandler handler)
    {
        handlers.Add(handler);
        SeerGoLogger.Log($"[BuffEffectManager] 注册处理器: {handler.GetType().Name}");
    }

    /// <summary>
    /// 处理回合开始时的buff效果
    /// </summary>
    public void ProcessTurnStart(IBuffable character, BattleEntityController opponent, BuffInstance buff)
    {
        foreach (var handler in handlers)
        {
            if (handler.CanHandle(buff.Config.type))
            {
                handler.OnTurnStart(character, opponent, buff);
                return;
            }
        }

        // 如果没有找到处理器,记录日志
        SeerGoLogger.Log($"[BuffEffectManager] 没有找到处理 {buff.Config.type} 的回合开始效果处理器");
    }

    /// <summary>
    /// 处理回合结束时的buff效果
    /// </summary>
    public void ProcessTurnEnd(IBuffable character,BattleEntityController opponent, BuffInstance buff)
    {
        foreach (var handler in handlers)
        {
            if (handler.CanHandle(buff.Config.type))
            {
                handler.OnTurnEnd(character,opponent, buff);
                return;
            }
        }
    }

    /// <summary>
    /// 获取属性加成
    /// </summary>
    public float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
    {
        foreach (var handler in handlers)
        {
            if (handler.CanHandle(buff.Config.type))
            {
                return handler.GetStatMultiplier(character, buff, statType);
            }
        }

        return -10001f;
    }
    public void UnRegisterHandler()
    {
        handlers.Clear();
    }
}
