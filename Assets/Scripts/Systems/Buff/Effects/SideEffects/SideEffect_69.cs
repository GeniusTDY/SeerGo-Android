using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=69: 下{0}回合对手使用体力药剂时效果变成减少相应的体力
    /// </summary>
    public class SideEffect_69 : ISkillSideEffectHandler
    {
        public override int EffectId => 69;

        public override void OnAfterBuffTurnEnd(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int duration))
            {
                return;
            }

            buffSystem.ApplyBuff(caster, target, 20024, new Dictionary<string, object>
            {
               { "RemainingTurns", duration.ToString() }
            });

        }
    }
}
