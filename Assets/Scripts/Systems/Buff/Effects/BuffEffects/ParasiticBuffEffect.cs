using SeerGoGame.Interfaces;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 寄生效果处理器
    /// </summary>
    public class ParasiticBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType)
        {
            return buffType == "PARASITIC";
        }

        public override void OnTurnEnd(IBuffable character, BattleEntityController opponent, BuffInstance buff)
        {
            // 寄生: 回合结束时损失最大体力的1/8
            if (character is PlayerBattleController player)
            {
                int damage = Mathf.RoundToInt(player.GetMaxHp() * buff.EffectValue);
                //  损失
                player.TakeDamage(damage);
                opponent.TakeDamage(damage, "+");
                SeerGoLogger.Log($"[寄生效果] 玩家精灵受到寄生伤害 {damage} 点");
            }
            else if (character is MonsterBattleController monster)
            {
                int damage = Mathf.RoundToInt(monster.GetMaxHp() * buff.EffectValue);
                monster.TakeDamage(damage);
                opponent.TakeDamage(damage, "+");
                SeerGoLogger.Log($"[寄生效果] 敌方精灵受到寄生伤害 {damage} 点");
            }
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            // 寄生不影响属性
            return -10001f;
        }
    }
}
