using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using System;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=81:"下{0}回合自身攻击技能必定命中"
    /// </summary>
    public class SideEffect_81 : ISkillSideEffectHandler
    {
        public override int EffectId => 81;

        public override void OnAfterBuffTurnEnd(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int duration))
            {
                return;
            }

            buffSystem.ApplyBuff(caster, target, 20029, new Dictionary<string, object>
            {
                { "RemainingTurns", duration.ToString() },
            });

        }
    }
}
