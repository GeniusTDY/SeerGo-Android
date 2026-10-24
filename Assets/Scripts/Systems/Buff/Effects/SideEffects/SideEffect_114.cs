using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=114: 命中后有{0}%几率令对方易燃
    /// 易燃效果：命中降低30%，受到火系伤害则100%烧伤
    /// </summary>
    public class SideEffect_114 : ISkillSideEffectHandler
    {
        public override int EffectId => 114;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int probability))
            {
                SeerGoLogger.LogError($"[SideEffect_114] 参数解析失败: {string.Join(",", args)}");
                return;
            }

            bool success = (probability >= 100) || (Random.Range(1, 101) <= probability);
            if (!success)
            {
                SeerGoLogger.Log($"[SideEffect_114] 概率判定失败 ({probability}%)");
                return;
            }

            // 应用易燃Buff (FLAMMABLE, buffId=20038)
            buffSystem.ApplyBuff(caster, target, 20038);

            SeerGoLogger.Log($"[SideEffect_114] 易燃效果生效");
        }
    }
}
