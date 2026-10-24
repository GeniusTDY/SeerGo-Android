using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=10002: 自身体力/2+个体值
    ///艾尔伊洛“幻化利刃”
    /// 公式: 自身体力/2+个体值
    /// </summary>
    public class SideEffect_10002 : ISkillSideEffectHandler
    {
        public override int EffectId => 10002;

        public override void OnBeforeDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (caster is BattleEntityController casterController)
            {
                var petData = casterController.GetCurrentPetData();
                int iv = petData.Talent.IV;
                int hp = petData.CurrentStatus.Hp;

                if (caster is PlayerBattleController)
                {
                    RoundContext.Current.PlayerSkillPower = (int)Math.Floor(hp / 2f + iv);
                }
                else if (caster is MonsterBattleController)
                {
                    RoundContext.Current.EnemySkillPower = (int)Math.Floor(hp / 2f + iv);
                }
            }
        }
    }
}
