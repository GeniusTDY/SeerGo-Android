using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 恢复伤害百分比体力
    /// </summary>
    public class DamagePercentRecoverHpBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType)
        {
            return buffType == "DAMAGE_PERCENT_RECOVER";
        }

        public override void OnTurnEnd(IBuffable character, BattleEntityController opponent, BuffInstance buff)
        {
            if (character is PlayerBattleController player)
            {
                if(RoundContext.Current.PlayerDamage == 0)
                    return;

                int damage = Mathf.RoundToInt(RoundContext.Current.PlayerDamage * (1/buff.EffectValue));
                player.TakeDamage(damage, "+");
                SeerGoLogger.Log($"<color=red>[恢复效果] 玩家精灵恢复 {damage} 点</color>");
            }
            else if (character is MonsterBattleController monster)
            {
                if (RoundContext.Current.EnemyDamage == 0)
                    return;

                int damage = Mathf.RoundToInt(RoundContext.Current.EnemyDamage * buff.EffectValue);
                monster.TakeDamage(damage, "+");
                SeerGoLogger.Log($"<color=red>[恢复效果] 敌方精灵恢复 {damage} 点</color>");
            }
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            return -10001f;
        }
    }
}
