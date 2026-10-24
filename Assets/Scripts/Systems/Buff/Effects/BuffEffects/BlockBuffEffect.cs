using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 抵挡Buff效果处理器
    /// </summary>
    public class BlockBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType) => buffType == "BLOCK";

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            return -10001f;
        }

        /// <summary>
        /// 尝试抵挡攻击
        /// </summary>
        public static bool TryBlock(IBuffable character)
        {
            if (character.ActiveBuffs.TryGetValue(20002, out var blockBuff))
            {
                return true;
            }
            return false;
        }
    }
}
