using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=118: 威力随机，随机范围150~220
    /// </summary>
    public class SideEffect_118 : ISkillSideEffectHandler
    {
        public override int EffectId => 118;

        public override void OnBeforeDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            int power = Random.Range(150, 221);

            if (caster is PlayerBattleController)
            {
                RoundContext.Current.PlayerSkillPower = power;
                SeerGoLogger.Log($"[SideEffect_118] 随机威力: {power}");
            }
            else if (caster is MonsterBattleController)
            {
                RoundContext.Current.EnemySkillPower = power;
                SeerGoLogger.Log($"[SideEffect_118] 随机威力: {power}");
            }
        }
    }
}
