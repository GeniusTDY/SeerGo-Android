using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=72: 若没有命中对手则自己失去全部体力
    /// </summary>
    public class SideEffect_72 : ISkillSideEffectHandler
    {
        public override int EffectId => 72;

        public override void OnSkillNotHit(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (caster is PlayerBattleController playerBattle)
            {
                playerBattle.TakeDamage(playerBattle.GetCurrentPetData().CurrentStatus.Hp);
            }
            else if (caster is MonsterBattleController monsterBattle)
            {
                monsterBattle.TakeDamage(monsterBattle.GetCurrentPetData().CurrentStatus.Hp);
            }
        }
    }
}
