using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 属性同步效果处理器：每回合结束时移除自身属性变化，再复制对手的属性变化到自身
    /// </summary>
    public class StatMirrorBuffEffect : IBuffEffectHandler
    {
        private static readonly int[] StatChangeBuffIds =
        {
            10000, // 攻击
            10001, // 防御
            10002, // 特攻
            10003, // 特防
            10004, // 速度
            10005, // 命中
            10006, // 暴击率
        };

        public override bool CanHandle(string buffType)
        {
            return buffType == "STAT_MIRROR";
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            // 属性镜像自身不修改属性值，返回 -10001 表示不处理
            return -10001f;
        }

        public override void OnTurnEnd(IBuffable character, BattleEntityController opponent, BuffInstance buff)
        {
            // 1. 先移除自身现有的属性变化buff（上次同步的旧镜像）
            foreach (var statId in StatChangeBuffIds)
            {
                if (character.ActiveBuffs.ContainsKey(statId))
                    character.RemoveBuff(statId);
            }

            // 2. 复制对手当前所有属性变化buff到自身
            int remainingTurns = buff.RemainingTurns;
            foreach (var statId in StatChangeBuffIds)
            {
                if (opponent.ActiveBuffs.TryGetValue(statId, out var opponentStatBuff))
                {
                    var mirrorBuff = new BuffInstance
                    {
                        Config = opponentStatBuff.Config,
                        RemainingTurns = remainingTurns,
                        EffectValue = opponentStatBuff.EffectValue,
                        MaxStackCount = opponentStatBuff.MaxStackCount,
                        Keep = opponentStatBuff.Keep,
                        SwitchCount = opponentStatBuff.SwitchCount,
                        TargetType = BuffTargetType.Self,
                        Args = opponentStatBuff.Args
                    };
                    character.AddBuff(mirrorBuff);
                    SeerGoLogger.Log($"[属性同步] 复制对手 {opponentStatBuff.Config.name}({opponentStatBuff.EffectValue:+#;-#;0}) → 自身(剩余{remainingTurns}回合)");
                }
            }
        }
    }
}
