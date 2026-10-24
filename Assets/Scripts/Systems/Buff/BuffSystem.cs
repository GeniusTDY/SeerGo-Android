using QFramework;
using SeerGoGame.Buffs;
using SeerGoGame.Helper;
using SeerGoGame.Models;
using SeerGoGame.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Systems
{
    public class BuffInstance
    {
        public BuffData Config;
        public int RemainingTurns;
        public float EffectValue;
        public int MaxStackCount;
        public bool Keep;
        public int SwitchCount;
        public BuffTargetType TargetType;
        public string[] Args; // 参数数组，用于复杂效果
    }

    public interface IBuffable
    {
        Dictionary<int, BuffInstance> ActiveBuffs { get; }
        void AddBuff(BuffInstance buff);
        void RemoveBuff(int buffId);
        void OnTurnStart(BattleEntityController opponent);
        void OnTurnEnd(BattleEntityController opponent);
    }

    public class BuffSystem : AbstractSystem
    {
        protected override void OnInit()
        {
            // 系统初始化
        }

        // 核心方法：支持指定施法者和目标
        public void ApplyBuff(IBuffable caster, IBuffable target, int buffId, Dictionary<string, object> parameters = null)
        {
            var buffModel = this.GetModel<IBuffModel>();

            if (buffModel.BuffConfigs.TryGetValue(buffId, out var buffConfig))
            {
                var buffInstance = new BuffInstance
                {
                    Config = buffConfig,
                    RemainingTurns = DurationParser.Parse(buffConfig.duration),
                    EffectValue = buffConfig.effectValue,
                    MaxStackCount = buffConfig.maxStackCount,
                    Keep = buffConfig.keep,
                    SwitchCount = buffConfig.switchCount,
                    TargetType = buffConfig.GetTargetType()
                };

                if (parameters != null)
                {
                    if (parameters.TryGetValue("RemainingTurns", out var rtObj) && rtObj is string remainingTurns)
                        buffInstance.RemainingTurns = DurationParser.Parse(remainingTurns);

                    if (parameters.TryGetValue("EffectValue", out var evObj) && evObj is float effectValue)
                        buffInstance.EffectValue = effectValue;

                    if (parameters.TryGetValue("MaxStackCount", out var mscObj) && mscObj is int maxStackCount)
                        buffInstance.MaxStackCount = maxStackCount;

                    if (parameters.TryGetValue("Keep", out var kpObj) && kpObj is bool keep)
                        buffInstance.Keep = keep;

                    if (parameters.TryGetValue("SwitchCount", out var scObj) && scObj is int SwitchCount)
                        buffInstance.SwitchCount = SwitchCount;

                    if (parameters.TryGetValue("TargetType", out var ttObj) && ttObj is BuffTargetType TargetType)
                        buffInstance.TargetType = TargetType;

                    if (parameters.TryGetValue("Args", out var argsObj) && argsObj is string[] args)
                        buffInstance.Args = args;
                }

                // 根据buff配置中的targetType确定实际应用buff的对象
                IBuffable actualTarget = DetermineActualTarget(caster, target, buffInstance.TargetType);

                // 判断对方是否有免疫能力下降buff
                if (IsImmuneToStatDownBuff(caster, target, buffInstance))
                    return;
                // 判断对方是否有免疫所有异常buff
                if (IsImmuneToStatusBuff(caster, target, buffInstance))
                    return;

                // 判断对方是否免疫控制类异常
                if(IsImmuneToStatusControlBuff(caster, target, buffInstance))
                    return;
                // 判断对方是否免疫弱化类异常
                if(IsImmuneToWeakStatus(caster, target, buffInstance))
                    return;
                SeerGoLogger.Log($"<color=magenta>[ApplyBuff] Buff:{buffConfig.name}(ID:{buffId}), 施法者:{caster.GetType().Name}, 传入目标:{target.GetType().Name}, TargetType:{buffInstance.TargetType}, 实际目标:{actualTarget?.GetType().Name}</color>");

                if (actualTarget != null)
                {
                    actualTarget.AddBuff(buffInstance);
                }
            }
        }

        // 确定是否免疫弱化类异常
        private bool IsImmuneToWeakStatus(IBuffable caster, IBuffable target, BuffInstance buffInstance)
        {
            if (buffInstance.Config.tab != 2)
                return false;

            IBuffable immuneTarget = buffInstance.TargetType switch
            {
                BuffTargetType.Enemy => target,
                BuffTargetType.Self => caster,
                _ => null
            };

            return immuneTarget != null && ImmuneBuffEffect.IsImmuneToWeakStatus(immuneTarget);
        }

        // 确定是否免疫控制类异常
        private bool IsImmuneToStatusControlBuff(IBuffable caster, IBuffable target, BuffInstance buffInstance)
        {
            if (buffInstance.Config.tab != 1)
                return false;

            IBuffable immuneTarget = buffInstance.TargetType switch
            {
                BuffTargetType.Enemy => target,
                BuffTargetType.Self => caster,
                _ => null
            };

            return immuneTarget != null && ImmuneBuffEffect.IsImmuneToControlStatus(immuneTarget);
        }

        // 判断是否免疫状态下降buff
        private bool IsImmuneToStatDownBuff(IBuffable caster, IBuffable target, BuffInstance buffInstance)
        {
            if (buffInstance.Config.tab != 9 || buffInstance.EffectValue >= 0)
                return false;

            IBuffable immuneTarget = buffInstance.TargetType switch
            {
                BuffTargetType.Enemy => target,
                BuffTargetType.Self => caster,
                _ => null
            };

            return immuneTarget != null && ImmuneBuffEffect.IsImmuneToStatDown(immuneTarget);
        }

        // 确定是否免疫所有异常buff
        private bool IsImmuneToStatusBuff(IBuffable caster, IBuffable target, BuffInstance buffInstance)
        {
            if (buffInstance.Config.tab != 1 && buffInstance.Config.tab != 2)
                return false;

            IBuffable immuneTarget = buffInstance.TargetType switch
            {
                BuffTargetType.Enemy => target,
                BuffTargetType.Self => caster,
                _ => null
            };

            return immuneTarget != null && ImmuneBuffEffect.IsImmuneToStatus(immuneTarget);
        }

        // 根据buff配置的目标类型确定实际应用对象
        private IBuffable DetermineActualTarget(IBuffable caster, IBuffable target, BuffTargetType targetType)
        {
            switch (targetType)
            {
                case BuffTargetType.Self:
                    return caster;  // Buff作用于施法者自己
                case BuffTargetType.Enemy:
                    return target;  // Buff作用于目标（通常为敌人）
                case BuffTargetType.Ally:
                    return target;  // Buff作用于盟友（可根据需要扩展阵营判断）
                default:
                    return caster;
            }
        }

        public void ProcessTurnStart(IBuffable target, BattleEntityController opponent)
        {
            target.OnTurnStart(opponent);
        }

        public void ProcessTurnEnd(IBuffable target, BattleEntityController opponent)
        {
            target.OnTurnEnd(opponent);
        }
    }
}