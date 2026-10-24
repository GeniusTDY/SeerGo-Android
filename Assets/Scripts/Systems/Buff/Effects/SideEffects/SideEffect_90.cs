using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=90: {0}回合内，自身造成的伤害为{1}倍
    /// </summary>
    public class SideEffect_90 : ISkillSideEffectHandler
    {
        public override int EffectId => 90;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 2 || !int.TryParse(args[0], out int duration) || !float.TryParse(args[1], out float multiple))
            {
                SeerGoLogger.LogError($"[SideEffect_90] 参数解析失败");
                return;
            }

           buffSystem.ApplyBuff(caster, target, 20019, new Dictionary<string, object>()
           {
               { "RemainingTurns", duration.ToString() },
               { "EffectValue", multiple }
           });
        }
    }
}
