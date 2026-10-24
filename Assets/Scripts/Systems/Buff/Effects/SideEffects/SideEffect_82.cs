using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=82: 目标为雄性，伤害为200%；目标为雌性，伤害为50%
    /// </summary>
    public class SideEffect_82 : ISkillSideEffectHandler
    {
        public override int EffectId => 82;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (target is BattleEntityController battleController)
            {
                var petData = battleController.GetCurrentPetData();
                float multiplier = GetGenderMultiplier(petData.Basic.Gender);

                if (multiplier != 1f)
                {
                    bool isPlayerTarget = target is MonsterBattleController;
                    if (isPlayerTarget)
                    {
                        RoundContext.Current.PlayerDamage = (int)(RoundContext.Current.PlayerDamage * multiplier);
                    }
                    else
                    {
                        RoundContext.Current.EnemyDamage = (int)(RoundContext.Current.EnemyDamage * multiplier);
                    }
                }
            }
        }

        private float GetGenderMultiplier(int gender) => gender switch
        {
            0 => 0.5f,  // 雌
            1 => 2f,    // 雄
            _ => 1f     // 其他性别不调整
        };

    }
}
