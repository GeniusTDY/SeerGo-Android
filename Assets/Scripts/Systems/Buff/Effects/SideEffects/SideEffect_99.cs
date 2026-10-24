using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=99: 命中时，{0}%几率令对手混乱
    /// </summary>
    public class SideEffect_99 : ISkillSideEffectHandler
    {
        public override int EffectId => 99;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int probability))
            {
                SeerGoLogger.LogError($"[SideEffect_99] 参数解析失败: {string.Join(",", args)}");
                return;
            }

            SeerGoLogger.Log($"<color=cyan>[混乱效果] 概率:{probability}%</color>");

            // 概率判定
            bool success = (probability >= 100) || (Random.Range(1, 101) <= probability);

            if (!success)
            {
                SeerGoLogger.Log($"[SideEffect_99] 概率判定失败");
                return;
            }

            // 应用混乱Buff (ID=10)
            SeerGoLogger.Log($"<color=yellow>[混乱效果] 概率判定成功，应用混乱Buff</color>");
            buffSystem.ApplyBuff(caster, target, 10);
        }
    }
}
