using QFramework;
using SeerGoGame.Models;
using SeerGoGame.Systems;

namespace SeerGoGame.Interfaces
{
    /// <summary>
    /// 技能副作用处理器接口
    /// 每种副作用实现自己的逻辑
    /// </summary>
    public abstract class ISkillSideEffectHandler
    {
        /// <summary>
        /// 该处理器能处理的副作用ID
        /// </summary>
        public abstract int EffectId { get; }

        /// <summary>
        /// 处理副作用效果 技能使用后
        /// </summary>
        /// <param name="caster">施法者</param>
        /// <param name="target">目标</param>
        /// <param name="skill">技能数据</param>
        /// <param name="args">参数数组</param>
        /// <param name="effectInfo">效果信息</param>
        /// <param name="buffSystem">Buff系统引用</param>
        public virtual void OnAfterCastSkill(IBuffable caster, IBuffable target, SkillData skill,
                    string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        { }


        /// <summary>
        /// 处理副作用效果 技能伤害计算前
        /// </summary>
        public virtual void OnBeforeDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                    string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        { }
        /// <summary>
        /// 处理副作用效果 技能伤害计算后
        /// </summary>
        public virtual void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                    string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        { }
        /// <summary>
        /// 处理副作用效果 技能伤害应用前
        /// </summary>
        public virtual void OnBeforeApplyDamage(IBuffable caster, IBuffable target, SkillData skill,
                    string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        { }
        /// <summary>
        /// 处理副作用效果 技能伤害应用后
        /// </summary>
        public virtual void OnAfterApplyDamage(IBuffable caster, IBuffable target, SkillData skill,
                    string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        { }

        /// <summary>
        /// 处理副作用效果 回合结束buff处理后
        /// </summary>
        public virtual void OnAfterBuffTurnEnd(IBuffable caster, IBuffable target, SkillData skill,
                    string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        { }
        /// <summary>
        /// 处理副作用效果 技能未命中
        /// </summary>
        public virtual void OnSkillNotHit(IBuffable caster, IBuffable target, SkillData skill,
                    string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        { }

    }
}
