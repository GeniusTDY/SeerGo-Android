using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=76:{0}%几率令对手在{1}回合内，每回合受到{2}点固定伤害
    /// </summary>
    public class SideEffect_76 : ISkillSideEffectHandler
    {
        public override int EffectId => 76;

        public override void OnAfterApplyDamage(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 3 || !int.TryParse(args[0], out int probability) || !float.TryParse(args[1], out float duration) || !float.TryParse(args[2], out float ExtraDamage))
            {
                SeerGoLogger.LogError($"[SideEffect_76] 参数解析失败");
                return;
            }

            bool success = (probability >= 100) || (Random.Range(1, 101) <= probability);

            if (!success)
            {
                SeerGoLogger.LogError($"[SideEffect_76] 判定失败");
                return;
            }

            SeerGoLogger.Log($"<color=cyan>[SideEffect_76] n:{ExtraDamage}</color>");

            buffSystem.ApplyBuff(caster, target, 20009, new Dictionary<string, object>
            {
                { "RemainingTurns", duration.ToString() },
                { "EffectValue", ExtraDamage }
            });

        }
    }
}
