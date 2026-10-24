using SeerGoGame.Interfaces;
using SeerGoGame.Systems;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 固伤效果处理器
    /// </summary>
    public class FixedDamageBonusBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType)
        {
            return buffType == "FIXED_DAMAGE_BONUS";
        }

        public override void OnTurnEnd(IBuffable character, BattleEntityController opponent, BuffInstance buff)
        {
            if (character is PlayerBattleController player)
            {
                player.TakeDamage(Mathf.RoundToInt(buff.EffectValue));
            }
            else if (character is MonsterBattleController monster)
            {
                monster.TakeDamage(Mathf.RoundToInt(buff.EffectValue));
            }
        }
        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            return -10001f;
        }
    }
}
