using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 每回合伤害减免效果处理器（对应buff ID=20039, type=DAMAGE_REDUCTION_PER_TURN）
    /// 每回合结束时，减少本回合受到的伤害值
    /// </summary>
    public class DamageReductionPerTurnBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType)
        {
            return buffType == "DAMAGE_REDUCTION_PER_TURN";
        }

        public override void OnTurnEnd(IBuffable character, BattleEntityController opponent, BuffInstance buff)
        {
            int reduceAmount = (int)buff.EffectValue;
            if (reduceAmount <= 0) return;

            if (character is PlayerBattleController player)
            {
                if (RoundContext.Current.EnemyDamage > 0)
                {
                    int reducedDamage = RoundContext.Current.EnemyDamage - reduceAmount;
                    if (reducedDamage < 0) reducedDamage = 0;

                    int actualReduction = RoundContext.Current.EnemyDamage - reducedDamage;
                    SeerGoLogger.Log($"[伤害减免] 玩家受到伤害{RoundContext.Current.EnemyDamage}，减免{actualReduction}点，剩余{reducedDamage}点");

                    // 返还减免的伤害到HP
                    if (actualReduction > 0)
                    {
                        player.TakeDamage(actualReduction, "+");
                    }
                }
            }
            else if (character is MonsterBattleController monster)
            {
                if (RoundContext.Current.PlayerDamage > 0)
                {
                    int reducedDamage = RoundContext.Current.PlayerDamage - reduceAmount;
                    if (reducedDamage < 0) reducedDamage = 0;

                    int actualReduction = RoundContext.Current.PlayerDamage - reducedDamage;
                    SeerGoLogger.Log($"[伤害减免] 对手受到伤害{RoundContext.Current.PlayerDamage}，减免{actualReduction}点，剩余{reducedDamage}点");

                    if (actualReduction > 0)
                    {
                        monster.TakeDamage(actualReduction, "+");
                    }
                }
            }
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            return -10001f;
        }
    }
}
