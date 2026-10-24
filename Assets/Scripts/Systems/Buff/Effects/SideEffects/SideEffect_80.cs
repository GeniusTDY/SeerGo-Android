using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using System;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=80: 损失1/2的当前体力，给予对手同等的伤害
    /// </summary>
    public class SideEffect_80 : ISkillSideEffectHandler
    {
        public override int EffectId => 80;

        public override void OnAfterApplyDamage(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (caster is PlayerBattleController player)
            {
                MonsterBattleController monsterBattle = target as MonsterBattleController;
                var petData = player.GetCurrentPetData();
                int damage = (int)Math.Max(petData.CurrentStatus.Hp / 2f, 1);
                player.TakeDamage(damage);
                monsterBattle.TakeDamage(damage);
            }
            else if (caster is MonsterBattleController monster)
            {
                PlayerBattleController playerBattle = target as PlayerBattleController;
                var petData = monster.GetCurrentPetData();
                int damage = (int)Math.Max(petData.CurrentStatus.Hp / 2f, 1);

                monster.TakeDamage(damage);
                playerBattle.TakeDamage(damage);

            }
        }
    }
}
