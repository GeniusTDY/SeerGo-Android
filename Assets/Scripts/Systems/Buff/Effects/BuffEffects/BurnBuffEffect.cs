using SeerGoGame.Interfaces;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 烧伤效果处理器
    /// </summary>
    public class BurnBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType)
        {
            return buffType == "BURN";
        }

        public override void OnTurnEnd(IBuffable character, BattleEntityController opponent, BuffInstance buff)
        {
            SeerGoLogger.Log("<color=orange>[烧伤效果] 开始处理...</color>");
            // 烧伤: 回合结束时损失最大体力的1/8
            if (character is PlayerBattleController player)
            {
                int damage = Mathf.RoundToInt(player.GetMaxHp() * buff.EffectValue);
                player.TakeDamage(damage);
                SeerGoLogger.Log($"<color=red>[烧伤效果] 玩家精灵受到烧伤伤害 {damage} 点</color>");
            }
            else if (character is MonsterBattleController monster)
            {
                int damage = Mathf.RoundToInt(monster.GetMaxHp() * buff.EffectValue);
                monster.TakeDamage(damage);
                SeerGoLogger.Log($"<color=red>[烧伤效果] 敌方精灵受到烧伤伤害 {damage} 点</color>");
            }
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            if (buff.Config.type != statType)
            {
                return -10001f;
            }

            // 烧伤会降低攻击技能威力50%
            if (buff.Config.type == "ATK")
            {
                return LevelEffectSystem.GetActualMultiplier(-2); ; // 攻击力减少50%
            }
            return -10001f;
        }
    }
}
