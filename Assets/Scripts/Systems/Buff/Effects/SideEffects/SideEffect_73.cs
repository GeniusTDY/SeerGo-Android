using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=73: 若先手攻击，则将所受伤害2倍反击给对手
    /// </summary>
    public class SideEffect_73 : ISkillSideEffectHandler
    {
        public override int EffectId => 73;

        public override void OnAfterApplyDamage(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (caster is PlayerBattleController playerBattle)
            {
                if (RoundContext.Current.Priority == TurnPriority.PlayerFirst)
                    RoundContext.Current.isPlayerFirstDoubleDamage = true;
            }
            else if (caster is MonsterBattleController monsterBattle)
            {
                if (RoundContext.Current.Priority == TurnPriority.EnemyFirst)
                    RoundContext.Current.isMonsterFirstDoubleDamage = true;
            }
        }
    }
}
