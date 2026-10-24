using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=70: 随机威力 - 威力随机，随机范围150~220
    /// </summary>
    public class SideEffect_70 : ISkillSideEffectHandler
    {
        public override int EffectId => 70;

        public override void OnBeforeDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            int power = Random.Range(150, 221);

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
