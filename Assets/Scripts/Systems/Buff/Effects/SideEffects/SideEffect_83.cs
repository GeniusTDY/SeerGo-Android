using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=83:自身雄性，下两回合必定先手；自身雌性，下两回合必定致命一击
    /// </summary>
    public class SideEffect_83 : ISkillSideEffectHandler
    {
        public override int EffectId => 83;

        public override void OnAfterBuffTurnEnd(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (caster is BattleEntityController casterController)
            {
                PetData petData = casterController.GetCurrentPetData();
                if (petData.Basic.Gender == 0)
                    buffSystem.ApplyBuff(caster, target, 20008, new Dictionary<string, object>
                    {
                        { "RemainingTurns", "2" }
                    });
                else if (petData.Basic.Gender == 1)
                    buffSystem.ApplyBuff(caster, target, 20030, new Dictionary<string, object>
                    {
                        { "RemainingTurns", "2"}
                    });
            }
        }
    }
}
