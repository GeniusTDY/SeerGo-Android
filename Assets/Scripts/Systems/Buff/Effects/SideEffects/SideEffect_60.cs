using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=60:{0}回合内，每回合都能附加{1}点固定伤害
    /// </summary>
    public class SideEffect_60 : ISkillSideEffectHandler
    {
        public override int EffectId => 60;

        public override void OnAfterApplyDamage(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 2 || !int.TryParse(args[0], out int duration) || !float.TryParse(args[1], out float ExtraDamage))
            {
                SeerGoLogger.LogError($"[SideEffect_60] 参数解析失败");
                return;
            }
            SeerGoLogger.Log($"<color=cyan>[SideEffect_60] n:{ExtraDamage}</color>");

            buffSystem.ApplyBuff(caster, target, 20009, new Dictionary<string, object>
            {
                { "RemainingTurns", duration.ToString() },
                { "EffectValue", ExtraDamage }
            });

        }
    }
}
