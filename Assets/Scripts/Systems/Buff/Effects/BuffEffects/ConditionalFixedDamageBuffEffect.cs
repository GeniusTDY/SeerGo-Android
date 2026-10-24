using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 条件固伤效果处理器（对应buff ID=20040, type=CONDITIONAL_FIXED_DAMAGE）
    /// 每回合结束时，若自身处于能力下降或异常状态，则对对手造成固定伤害
    /// </summary>
    public class ConditionalFixedDamageBuffEffect : IBuffEffectHandler
    {
        // 能力下降buff ID列表
        private static readonly int[] DebuffBuffIds =
        {
            10000, 10001, 10002, 10003, 10004, 10005, // 属性buff
        };

        // 异常状态buff ID列表
        private static readonly int[] StatusBuffIds =
        {
            1, 2, 3, 4, 5, 6, 7, 8, 9, // 中毒、冻伤、烧伤、流血、麻痹、疲惫、害怕、睡眠、石化
        };

        public override bool CanHandle(string buffType)
        {
            return buffType == "CONDITIONAL_FIXED_DAMAGE";
        }

        public override void OnTurnEnd(IBuffable character, BattleEntityController opponent, BuffInstance buff)
        {
            if (opponent == null) return;

            bool hasDebuff = false;
            bool hasStatus = false;

            // 检查是否有能力下降（EffectValue < 0）
            foreach (var debuffId in DebuffBuffIds)
            {
                if (character.ActiveBuffs.TryGetValue(debuffId, out var statBuff) && statBuff.EffectValue < 0)
                {
                    hasDebuff = true;
                    break;
                }
            }

            // 检查是否有异常状态
            foreach (var statusId in StatusBuffIds)
            {
                if (character.ActiveBuffs.ContainsKey(statusId))
                {
                    hasStatus = true;
                    break;
                }
            }

            if (hasDebuff || hasStatus)
            {
                int fixedDamage = Mathf.RoundToInt(buff.EffectValue);
                opponent.TakeDamage(fixedDamage);
                string reason = hasDebuff ? "能力下降" : "";
                reason += hasStatus ? (string.IsNullOrEmpty(reason) ? "异常状态" : "或异常状态") : "";
                SeerGoLogger.Log($"[条件固伤] 处于{reason}，对对手造成{fixedDamage}点固定伤害");
            }
            else
            {
                SeerGoLogger.Log("[条件固伤] 未处于能力下降或异常状态，不触发");
            }
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            return -10001f;
        }
    }
}
