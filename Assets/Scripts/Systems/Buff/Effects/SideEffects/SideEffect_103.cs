using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=103: {0}%几率令对手增加一层衰弱
    /// 衰弱效果：每层降低造成伤害的20%，可叠加
    /// </summary>
    public class SideEffect_103 : ISkillSideEffectHandler
    {
        public override int EffectId => 103;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int probability))
            {
                SeerGoLogger.LogError($"[SideEffect_103] 参数解析失败: {string.Join(",", args)}");
                return;
            }

            bool success = (probability >= 100) || (Random.Range(1, 101) <= probability);
            if (!success)
            {
                SeerGoLogger.Log($"[SideEffect_103] 概率判定失败 ({probability}%)");
                return;
            }

            // 应用衰弱Buff (WEAKEN, buffId=20033)
            buffSystem.ApplyBuff(caster, target, 20033, new Dictionary<string, object>
            {
                { "EffectValue", 1f }
            });

            SeerGoLogger.Log($"[SideEffect_103] 衰弱效果生效");
        }
    }
}
