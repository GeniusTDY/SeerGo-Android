using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 反弹效果处理器
    /// </summary>
    public class ReflectionBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType)
        {
            return buffType == "REFLECTION";
        }

        public override void OnTurnEnd(IBuffable character, BattleEntityController opponent, BuffInstance buff)
        {
            // 反弹: 作用{0}~{1}回合, 每回合反弹对手1/{2}伤害
            if (character is PlayerBattleController player)
            {
                if (RoundContext.Current.PlayerDamage <= 0)
                {
                    SeerGoLogger.Log($"<color=bule>[反弹效果] 玩家精灵没有受到伤害</color>");
                    return;
                }
                var petData = player.GetCurrentPetData();
                if (petData != null)
                {
                    int damage = Mathf.RoundToInt(RoundContext.Current.PlayerDamage * 1 / buff.EffectValue);
                    player.TakeDamage(damage);
                    SeerGoLogger.Log($"<color=bule>[反弹效果] 玩家精灵受到反弹伤害 {damage} 点</color>");
                }
            }
            else if (character is MonsterBattleController monster)
            {
                if (RoundContext.Current.EnemyDamage <= 0)
                {
                    SeerGoLogger.Log($"<color=bule>[反弹效果] 敌方精灵没有受到伤害</color>");
                    return;
                }
                var petData = monster.GetCurrentMonsterData();
                if (petData != null)
                {
                    int damage = Mathf.RoundToInt(RoundContext.Current.EnemyDamage * 1 / buff.EffectValue);
                    monster.TakeDamage(damage);
                    SeerGoLogger.Log($"<color=bule>[反弹效果] 敌方精灵受到反弹伤害 {damage} 点</color>");
                }
            }
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            // 反弹不影响属性
            return -10001f;
        }
    }
}
