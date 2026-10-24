using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=113: 个体越高，威力越大
    /// 公式: 威力倍率 = 1 + (IV/31) * 0.5，最高1.5倍
    /// </summary>
    public class SideEffect_113 : ISkillSideEffectHandler
    {
        public override int EffectId => 113;

        public override void OnBeforeDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (caster is BattleEntityController casterController)
            {
                var petData = casterController.GetCurrentPetData();
                int iv = petData.Talent.IV;
                float multiplier = 1f + (iv / 31f) * 0.5f;

                if (caster is PlayerBattleController)
                {
                    RoundContext.Current.PlayerSkillPower = (int)(RoundContext.Current.PlayerSkillPower * multiplier);
                    SeerGoLogger.Log($"[SideEffect_113] IV={iv}, 威力倍率={multiplier:F2}, 威力={RoundContext.Current.PlayerSkillPower}");
                }
                else if (caster is MonsterBattleController)
                {
                    RoundContext.Current.EnemySkillPower = (int)(RoundContext.Current.EnemySkillPower * multiplier);
                    SeerGoLogger.Log($"[SideEffect_113] IV={iv}, 威力倍率={multiplier:F2}, 威力={RoundContext.Current.EnemySkillPower}");
                }
            }
        }
    }
}
