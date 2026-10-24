using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=41/42: 伤害变化效果 立即生效
    /// ID=41: {0}~{1}回合本方受到的火系攻击伤害减半
    /// </summary>
    public class SideEffect_41 : ISkillSideEffectHandler
    {

        public override int EffectId => 41;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 2)
            {
                SeerGoLogger.LogError($"[SideEffect_41] 参数不足,需要2个参数");
                return;
            }

            if (!int.TryParse(args[0], out int duration1) ||
                !int.TryParse(args[1], out int duration2))
            {
                SeerGoLogger.LogError($"[SideEffect_41] 参数解析失败: {string.Join(",", args)}");
                return;
            }

            int duration = Random.Range(duration1, duration2 + 1);
            // 应用buff
            buffSystem.ApplyBuff(caster, target, 20013, new Dictionary<string, object>
            {
                { "RemainingTurns",  duration.ToString()}
            });

        }

    }
}
