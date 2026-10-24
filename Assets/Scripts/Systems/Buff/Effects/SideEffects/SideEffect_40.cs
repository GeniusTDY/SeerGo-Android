using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=40: 电光火石 - 先出手的话威力为2倍
    /// </summary>
    public class SideEffect_40 : ISkillSideEffectHandler
    {
        public override int EffectId => 40;

        public override void OnBeforeDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (caster is PlayerBattleController && RoundContext.Current.Priority == TurnPriority.PlayerFirst)
            {
                RoundContext.Current.PlayerSkillPower = skill.Power * 2;
            }
            else if (caster is MonsterBattleController && RoundContext.Current.Priority == TurnPriority.EnemyFirst)
            {
                RoundContext.Current.EnemySkillPower = skill.Power * 2;
            }
        }
    }
}
