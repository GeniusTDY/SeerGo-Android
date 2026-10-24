using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=89: {0}回合内，每次造成伤害的1/{1}会恢复自己的HP
    /// </summary>
    public class SideEffect_89 : ISkillSideEffectHandler
    {
        public override int EffectId => 89;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 2 || !int.TryParse(args[0], out int duration) || !float.TryParse(args[1], out float multiple))
            {
                SeerGoLogger.LogError($"[SideEffect_89] 参数解析失败");
                return;
            }

           buffSystem.ApplyBuff(caster, target, 20031, new Dictionary<string, object>()
           {
               { "RemainingTurns", duration.ToString() },
               { "EffectValue", multiple }
           });
        }
    }
}
