using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=100: 自身体力越少则威力越大
    /// 体力比例越低，威力倍率越高（最低1倍，最高2倍）
    /// 公式: 倍率 = 2 - (当前HP / 最大HP)
    /// </summary>
    public class SideEffect_100 : ISkillSideEffectHandler
    {
        public override int EffectId => 100;

        public override void OnBeforeDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (caster is BattleEntityController casterController)
            {
                var petData = casterController.GetCurrentPetData();
                float hpRatio = (float)petData.CurrentStatus.Hp / casterController.GetMaxHp();
                float multiplier = 2f - hpRatio;
                multiplier = UnityEngine.Mathf.Max(multiplier, 1f);

                if (caster is PlayerBattleController)
                {
                    RoundContext.Current.PlayerSkillPower = (int)(RoundContext.Current.PlayerSkillPower * multiplier);
                    SeerGoLogger.Log($"[SideEffect_100] HP比例={hpRatio:F2}, 威力倍率={multiplier:F2}, 威力={RoundContext.Current.PlayerSkillPower}");
                }
                else if (caster is MonsterBattleController)
                {
                    RoundContext.Current.EnemySkillPower = (int)(RoundContext.Current.EnemySkillPower * multiplier);
                    SeerGoLogger.Log($"[SideEffect_100] HP比例={hpRatio:F2}, 威力倍率={multiplier:F2}, 威力={RoundContext.Current.EnemySkillPower}");
                }
            }
        }
    }
}
