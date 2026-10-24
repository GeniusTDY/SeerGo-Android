using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=112: 牺牲全部体力给对手造成250~300点伤害，造成致命伤害时，对手剩下1点体力
    /// </summary>
    public class SideEffect_112 : ISkillSideEffectHandler
    {
        public override int EffectId => 112;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (caster is BattleEntityController casterController)
            {
                var petData = casterController.GetCurrentPetData();

                // 牺牲全部体力（降到0）
                if (caster is PlayerBattleController player)
                {
                    int currentHp = petData.CurrentStatus.Hp;
                    player.TakeDamage(currentHp);

                    // 造成250~300随机伤害
                    int sacrificeDamage = Random.Range(250, 301);
                    RoundContext.Current.PlayerDamage = sacrificeDamage;

                    SeerGoLogger.Log($"[SideEffect_112] 牺牲体力({currentHp})，造成{sacrificeDamage}点伤害");
                }
                else if (caster is MonsterBattleController monster)
                {
                    int currentHp = petData.CurrentStatus.Hp;
                    monster.TakeDamage(currentHp);

                    int sacrificeDamage = Random.Range(250, 301);
                    RoundContext.Current.EnemyDamage = sacrificeDamage;

                    SeerGoLogger.Log($"[SideEffect_112] 牺牲体力({currentHp})，造成{sacrificeDamage}点伤害");
                }

                // 对手剩下1点体力（手下留情效果）
                buffSystem.ApplyBuff(caster, target, 20013);
            }
        }
    }
}
