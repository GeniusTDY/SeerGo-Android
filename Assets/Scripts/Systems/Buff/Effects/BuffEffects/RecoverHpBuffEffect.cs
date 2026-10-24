using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 使用技能恢复生命值
    /// </summary>
    public class RecoverHpBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType)
        {
            return buffType == "SKILL_RECOVER_HP";
        }

        public override void OnTurnEnd(IBuffable character, BattleEntityController opponent, BuffInstance buff)
        {
            if (character is PlayerBattleController player)
            {
                if(RoundContext.Current.PlayerSkill == null)
                    return;

                int damage = Mathf.RoundToInt(player.GetMaxHp() * (1/buff.EffectValue));
                player.TakeDamage(damage, "+");
                SeerGoLogger.Log($"<color=red>[恢复效果] 玩家精灵恢复 {damage} 点</color>");
            }
            else if (character is MonsterBattleController monster)
            {
                if (RoundContext.Current.EnemySkill == null)
                    return;

                int damage = Mathf.RoundToInt(monster.GetMaxHp() * buff.EffectValue);
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
