using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=50: 伤害变化效果
    /// ID=50: {0}回合自己受到物理攻击伤害减半
    /// </summary>
    public class SideEffect_50 : ISkillSideEffectHandler
    {
        public override int EffectId => 50;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1)
            {
                SeerGoLogger.LogError($"[SideEffect_50] 参数不足,需要1个参数");
                return;
            }

            if (!int.TryParse(args[0], out int duration))
            {
                SeerGoLogger.LogError($"[SideEffect_50] 参数解析失败: {string.Join(",", args)}");
                return;
            }

            // 应用buff
            buffSystem.ApplyBuff(caster, target, 20014, new Dictionary<string, object>
            {
                { "RemainingTurns",  duration.ToString()}
            });

        }

    }
}
