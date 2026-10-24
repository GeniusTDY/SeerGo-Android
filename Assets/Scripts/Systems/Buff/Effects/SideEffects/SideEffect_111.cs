using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=111: 附加额外伤害，自身等级越高，附加的伤害越高
    /// 公式: 附加伤害 = 等级 * 1.5
    /// </summary>
    public class SideEffect_111 : ISkillSideEffectHandler
    {
        public override int EffectId => 111;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (caster is BattleEntityController casterController)
            {
                var petData = casterController.GetCurrentPetData();
                int level = petData.Exp.Level;
                int bonusDamage = (int)(level * 1.5f);

                if (caster is PlayerBattleController)
                {
                    RoundContext.Current.PlayerDamage += bonusDamage;
                    SeerGoLogger.Log($"[SideEffect_111] 等级加成伤害: Lv{level} => +{bonusDamage}");
                }
                else if (caster is MonsterBattleController)
                {
                    RoundContext.Current.EnemyDamage += bonusDamage;
                    SeerGoLogger.Log($"[SideEffect_111] 等级加成伤害: Lv{level} => +{bonusDamage}");
                }
            }
        }
    }
}
