using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 受击反伤异常效果处理器（对应buff ID=20043, type=DAMAGE_THRESHOLD_STATUS）
    /// 每回合结束时，若本回合受到伤害高于阈值，则给对手施加固定伤害作为反击
    /// Args[0] = 异常状态类型码（麻痹0,中毒1,烧伤2,冻伤5,害怕6,疲惫7,睡眠8,石化9,冰封15）
    /// </summary>
    public class DamageThresholdStatusBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType)
        {
            return buffType == "DAMAGE_THRESHOLD_STATUS";
        }

        public override void OnTurnEnd(IBuffable character, BattleEntityController opponent, BuffInstance buff)
        {
            if (opponent == null || buff.Args == null || buff.Args.Length < 1)
                return;

            int threshold = (int)buff.EffectValue;
            if (!int.TryParse(buff.Args[0], out int statusType))
                return;

            int receivedDamage = 0;
            if (character is PlayerBattleController)
            {
                receivedDamage = RoundContext.Current.EnemyDamage;
            }
            else if (character is MonsterBattleController)
            {
                receivedDamage = RoundContext.Current.PlayerDamage;
            }

            if (receivedDamage > threshold)
            {
                // 受到伤害超过阈值，进行反击
                // 根据状态类型决定反击伤害量
                int retaliateDamage = threshold / 2; // 默认反击伤害为阈值的一半
                if (retaliateDamage > 0)
                {
                    opponent.TakeDamage(retaliateDamage);

                    string statusName = statusType switch
                    {
                        0 => "麻痹", 1 => "中毒", 2 => "烧伤", 5 => "冻伤",
                        6 => "害怕", 7 => "疲惫", 8 => "睡眠", 9 => "石化", 15 => "冰封",
                        _ => "未知"
                    };

                    SeerGoLogger.Log($"[反伤异常] 受到伤害{receivedDamage}>{threshold}，对手触发{statusName}效果，受到{retaliateDamage}点反击伤害");
                }
            }
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            return -10001f;
        }
    }
}
