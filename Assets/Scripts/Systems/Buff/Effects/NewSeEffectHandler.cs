using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Managers;
using SeerGoGame.Systems;
using System.Collections.Generic;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 特性效果处理器抽象类
    /// </summary>
    public abstract class INewSeEffectHandler
    {
        /// <summary>
        /// 特性ID
        /// </summary>
        public abstract int SeId { get; }

        /// <summary>
        /// 处理特性效果 技能使用后
        /// </summary>
        public virtual void OnAfterCastSkill(NewSeConfig seConfig, IBuffable caster, IBuffable target, SkillData skill,
                    string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        { }

        /// <summary>
        /// 处理特性效果 技能伤害计算前
        /// </summary>
        public virtual void OnBeforeDamageCalculation(NewSeConfig seConfig, IBuffable caster, IBuffable target, SkillData skill,
                    string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        { }

        /// <summary>
        /// 处理特性效果 技能伤害计算后
        /// </summary>
        public virtual void OnAfterDamageCalculation(NewSeConfig seConfig, IBuffable caster, IBuffable target, SkillData skill,
                    string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        { }

        /// <summary>
        /// 处理特性效果 技能伤害应用前
        /// </summary>
        public virtual void OnBeforeApplyDamage(NewSeConfig seConfig, IBuffable caster, IBuffable target, SkillData skill,
                    string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        { }

        /// <summary>
        /// 处理特性效果 技能伤害应用后
        /// </summary>
        public virtual void OnAfterApplyDamage(NewSeConfig seConfig, IBuffable caster, IBuffable target, SkillData skill,
                    string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        { }

        /// <summary>
        /// 处理特性效果 回合结束buff处理后
        /// </summary>
        public virtual void OnAfterBuffTurnEnd(NewSeConfig seConfig, IBuffable caster, IBuffable target, SkillData skill,
                    string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        { }

        /// <summary>
        /// 处理特性效果 技能未命中
        /// </summary>
        public virtual void OnSkillNotHit(NewSeConfig seConfig, IBuffable caster, IBuffable target, SkillData skill,
                    string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        { }
    }

    /// <summary>
    /// 特性效果管理器 - 负责根据特性ID调用对应的处理器
    /// </summary>
    public class NewSeEffectManager
    {
        private Dictionary<int, INewSeEffectHandler> handlers = new Dictionary<int, INewSeEffectHandler>();

        public NewSeEffectManager()
        {
            RegisterAllHandlers();
        }

        /// <summary>
        /// 注册所有特性处理器
        /// </summary>
        private void RegisterAllHandlers()
        {
            // 注册特性1001-1015的处理器 - 属性技能威力增加5%
            RegisterHandler(new NewSeEffect_SkillPowerUP(1001));// 草属性技能威力增加5%
            RegisterHandler(new NewSeEffect_SkillPowerUP(1002));// 水属性技能威力增加5%
            RegisterHandler(new NewSeEffect_SkillPowerUP(1003));// 火属性技能威力增加5%
            RegisterHandler(new NewSeEffect_SkillPowerUP(1004));// 飞行属性技能威力增加5%
            RegisterHandler(new NewSeEffect_SkillPowerUP(1005));// 电属性技能威力增加5%
            RegisterHandler(new NewSeEffect_SkillPowerUP(1006));// 机械属性技能威力增加5%
            RegisterHandler(new NewSeEffect_SkillPowerUP(1007));// 地面属性技能威力增加5%
            RegisterHandler(new NewSeEffect_SkillPowerUP(1008));// 普通属性技能威力增加5%
            RegisterHandler(new NewSeEffect_SkillPowerUP(1009));// 冰属性技能威力增加5%
            RegisterHandler(new NewSeEffect_SkillPowerUP(1010));// 超能属性技能威力增加5%
            RegisterHandler(new NewSeEffect_SkillPowerUP(1011));// 战斗属性技能威力增加5%
            RegisterHandler(new NewSeEffect_SkillPowerUP(1012));// 光属性技能威力增加5%
            RegisterHandler(new NewSeEffect_SkillPowerUP(1013));// 暗影属性技能威力增加5%
            RegisterHandler(new NewSeEffect_SkillPowerUP(1014));// 神秘属性技能威力增加5%
            RegisterHandler(new NewSeEffect_SkillPowerUP(1015));// 龙属性技能威力增加5%

            // 1016: 会心 - 所有技能致命一击率增加
            RegisterHandler(new NewSeEffect_1016());

            // 1017: 坚硬 - 受到任何伤害减少5%
            RegisterHandler(new NewSeEffect_1017());

            // 1018: 回避 - 被技能命中的几率减少5%
            RegisterHandler(new NewSeEffect_1018());

            // 1019: 顽强 - 受到致死攻击时有5%几率余下1点体力
            RegisterHandler(new NewSeEffect_1019());

            // 1020: 瞬杀 - 进攻类技能有5%几率秒杀对方
            RegisterHandler(new NewSeEffect_1020());

            // 1021: 回神 - 体力降低到1/8时有5%几率体力回满
            RegisterHandler(new NewSeEffect_1021());

            // 1022: 带电 - 受到普通攻击时有5%使对方麻痹
            // 1023: 高热 - 受到普通攻击时有5%使对方烧伤
            // 1024: 冰冷 - 受到普通攻击时有5%使对方冻伤
            // 1025: 阴森 - 受到普通攻击时有5%使对方害怕
            RegisterHandler(new NewSeEffect_CauseStatusOnHit(1022, 10)); // 麻痹
            RegisterHandler(new NewSeEffect_CauseStatusOnHit(1023, 12)); // 烧伤
            RegisterHandler(new NewSeEffect_CauseStatusOnHit(1024, 14)); // 冻伤
            RegisterHandler(new NewSeEffect_CauseStatusOnHit(1025, 15)); // 害怕

            // 1026: 反抗 - 受到特殊攻击时有5%几率使对方攻击降低1个等级
            // 1027: 忽略 - 受到特殊攻击时有5%几率使对方特攻降低1个等级
            // 1028: 草率 - 受到特殊攻击时有5%几率使对方特防降低1个等级
            // 1029: 慌张 - 受到特殊攻击时有5%几率使对方速度降低1个等级
            RegisterHandler(new NewSeEffect_StatDownOnHit(1026, 0)); // 攻击降低
            RegisterHandler(new NewSeEffect_StatDownOnHit(1027, 2)); // 特攻降低
            RegisterHandler(new NewSeEffect_StatDownOnHit(1028, 3)); // 特防降低
            RegisterHandler(new NewSeEffect_StatDownOnHit(1029, 4)); // 速度降低

            // 1030: 反击 - 受到任何攻击时有5%几率使自身攻击提升1个等级
            // 1031: 抵抗 - 受到任何攻击时有5%几率使自身防御提升1个等级
            // 1032: 反攻 - 受到任何攻击时有5%几率使自身特攻提升1个等级
            // 1033: 借风 - 受到任何攻击时有5%几率使自身速度提升1个等级
            RegisterHandler(new NewSeEffect_StatUpOnHit(1030, 0)); // 攻击提升
            RegisterHandler(new NewSeEffect_StatUpOnHit(1031, 1)); // 防御提升
            RegisterHandler(new NewSeEffect_StatUpOnHit(1032, 2)); // 特攻提升
            RegisterHandler(new NewSeEffect_StatUpOnHit(1033, 4)); // 速度提升

            // 1034: 虚无 - 5%几率完全抵挡一次伤害
            RegisterHandler(new NewSeEffect_1034());

        }

        /// <summary>
        /// 注册单个特性处理器
        /// </summary>
        private void RegisterHandler(INewSeEffectHandler handler)
        {
            if (handler != null)
            {
                handlers[handler.SeId] = handler;
            }
        }

        /// <summary>
        /// 处理特性效果 技能使用后
        /// </summary>
        public void OnAfterCastSkill(int seId, IBuffable caster, IBuffable target, SkillData skill,
                    string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            ApplyEffectAtTiming(seId, caster, target, skill, args, effectInfo, buffSystem,
                (handler, config) => handler.OnAfterCastSkill(config, caster, target, skill, args, effectInfo, buffSystem),
                "技能使用后");
        }

        /// <summary>
        /// 处理特性效果 技能伤害计算前
        /// </summary>
        public void OnBeforeDamageCalculation(int seId, IBuffable caster, IBuffable target, SkillData skill,
                    string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            ApplyEffectAtTiming(seId, caster, target, skill, args, effectInfo, buffSystem,
                (handler, config) => handler.OnBeforeDamageCalculation(config, caster, target, skill, args, effectInfo, buffSystem),
                "技能伤害计算前");
        }

        /// <summary>
        /// 处理特性效果 技能伤害计算后
        /// </summary>
        public void OnAfterDamageCalculation(int seId, IBuffable caster, IBuffable target, SkillData skill,
                    string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            ApplyEffectAtTiming(seId, caster, target, skill, args, effectInfo, buffSystem,
                (handler, config) => handler.OnAfterDamageCalculation(config, caster, target, skill, args, effectInfo, buffSystem),
                "技能伤害计算后");
        }

        /// <summary>
        /// 处理特性效果 技能伤害应用前
        /// </summary>
        public void OnBeforeApplyDamage(int seId, IBuffable caster, IBuffable target, SkillData skill,
                    string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            ApplyEffectAtTiming(seId, caster, target, skill, args, effectInfo, buffSystem,
                (handler, config) => handler.OnBeforeApplyDamage(config, caster, target, skill, args, effectInfo, buffSystem),
                "技能伤害应用前");
        }

        /// <summary>
        /// 处理特性效果 技能伤害应用后
        /// </summary>
        public void OnAfterApplyDamage(int seId, IBuffable caster, IBuffable target, SkillData skill,
                    string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            ApplyEffectAtTiming(seId, caster, target, skill, args, effectInfo, buffSystem,
                (handler, config) => handler.OnAfterApplyDamage(config, caster, target, skill, args, effectInfo, buffSystem),
                "技能伤害应用后");
        }

        /// <summary>
        /// 处理特性效果 回合结束buff处理后
        /// </summary>
        public void OnAfterBuffTurnEnd(int seId, IBuffable caster, IBuffable target, SkillData skill,
                    string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            ApplyEffectAtTiming(seId, caster, target, skill, args, effectInfo, buffSystem,
                (handler, config) => handler.OnAfterBuffTurnEnd(config, caster, target, skill, args, effectInfo, buffSystem),
                "回合结束buff处理后");
        }

        /// <summary>
        /// 处理特性效果 技能未命中
        /// </summary>
        public void OnSkillNotHit(int seId, IBuffable caster, IBuffable target, SkillData skill,
                    string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            ApplyEffectAtTiming(seId, caster, target, skill, args, effectInfo, buffSystem,
                (handler, config) => handler.OnSkillNotHit(config, caster, target, skill, args, effectInfo, buffSystem),
                "技能未命中");
        }

        /// <summary>
        /// 统一特性效果应用方法
        /// </summary>
        private void ApplyEffectAtTiming(int seId, IBuffable caster, IBuffable target, SkillData skill,
                    string[] args, EffectInfo effectInfo, BuffSystem buffSystem,
                    System.Action<INewSeEffectHandler, NewSeConfig> applyAction, string timingName)
        {
            NewSeConfig seConfig = NewSeManager.Instance.GetConfigById(seId);
            if (seConfig == null) return;

            if (handlers.TryGetValue(seId, out INewSeEffectHandler handler))
            {
                applyAction(handler, seConfig);
                SeerGoLogger.Log($"<color=cyan>[特性效果] {timingName} 应用特性 {seId} - {seConfig.Desc}</color>");
            }
            else
            {
                SeerGoLogger.LogWarning($"未找到特性 {seId} 的处理器");
            }
        }

        /// <summary>
        /// 清理所有处理器
        /// </summary>
        public void UnRegisterHandlers()
        {
            handlers.Clear();
        }
    }
}
