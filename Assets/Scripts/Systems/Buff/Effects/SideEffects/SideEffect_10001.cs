using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=10001: 个体越高，威力越大  
    /// 卡鲁耶克“异界幻象”、宿主岩“本能一击”
    /// 公式: 威力倍率 = IV * 31，最高155
    /// </summary>
    public class SideEffect_10001 : ISkillSideEffectHandler
    {
        public override int EffectId => 10001;

        public override void OnBeforeDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (caster is BattleEntityController casterController)
            {
                var petData = casterController.GetCurrentPetData();
                int iv = petData.Talent.IV;

                if (caster is PlayerBattleController)
                {
                    RoundContext.Current.PlayerSkillPower = iv * 31;
                }
                else if (caster is MonsterBattleController)
                {
                    RoundContext.Current.EnemySkillPower = iv * 31;
                }
            }
        }
    }
}
