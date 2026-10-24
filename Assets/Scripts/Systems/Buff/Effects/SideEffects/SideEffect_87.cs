using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=87: 恢复自身所有PP值
    /// </summary>
    public class SideEffect_87 : ISkillSideEffectHandler
    {
        public override int EffectId => 87;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (caster is BattleEntityController controller)
            {
                controller.GetCurrentPetData().Skills.NormalSkill.ForEach(skill =>
                {
                    skill.currentPP = skill.maxPP;
                });
            }
        }
    }
}
