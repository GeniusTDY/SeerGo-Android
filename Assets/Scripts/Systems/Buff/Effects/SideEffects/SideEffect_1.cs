using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=1: 吸取效果 - 给予对方损伤的一半会回复自己的体力
    /// </summary>
    public class SideEffect_1 : ISkillSideEffectHandler
    {
        public override int EffectId =>1;

        public override void OnAfterApplyDamage(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (caster is PlayerBattleController player)
            {
                if (RoundContext.Current.PlayerDamage <= 0)
                    return;
                player.TakeDamage((int)(RoundContext.Current.PlayerDamage * 0.5f), "+");
            }
            else if (caster is MonsterBattleController monster)
            {
                if (RoundContext.Current.EnemyDamage <= 0)
                    return;
                monster.TakeDamage((int)(RoundContext.Current.EnemyDamage * 0.5f), "+");
            }
        }
    }
}
