using SeerGoGame.Interfaces;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 冻伤效果处理器
    /// </summary>
    public class FreezeBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType)
        {
            return buffType == "FREEZE";
        }

        public override void OnTurnEnd(IBuffable character, BattleEntityController opponent, BuffInstance buff)
        {
            // 冻伤: 回合结束时损失最大体力的1/8
            if (character is PlayerBattleController player)
            {
                int damage = Mathf.RoundToInt(player.GetMaxHp() * buff.EffectValue);
                player.TakeDamage(damage);
                SeerGoLogger.Log($"<color=red>[冻伤效果] 玩家精灵受到冻伤伤害 {damage} 点</color>");
            }
            else if (character is MonsterBattleController monster)
            {
                int damage = Mathf.RoundToInt(monster.GetMaxHp() * buff.EffectValue);
                monster.TakeDamage(damage);
                SeerGoLogger.Log($"[冻伤效果] 敌方精灵受到冻伤伤害 {damage} 点");
            }
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            // 冻伤不影响属性
            return -10001f;
        }
    }
}
