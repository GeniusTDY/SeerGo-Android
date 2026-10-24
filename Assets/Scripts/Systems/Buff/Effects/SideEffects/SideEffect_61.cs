using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=61: 随机威力 - 威力随机，随机范围50~150
    /// </summary>
    public class SideEffect_61 : ISkillSideEffectHandler
    {
        public override int EffectId => 61;

        public override void OnBeforeDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            int power = Random.Range(50, 151);

            if (caster is PlayerBattleController)
            {
                RoundContext.Current.PlayerSkillPower = power;
            }
            else if (caster is MonsterBattleController)
            {
                RoundContext.Current.EnemySkillPower = power;
            }
        }
    }
}
