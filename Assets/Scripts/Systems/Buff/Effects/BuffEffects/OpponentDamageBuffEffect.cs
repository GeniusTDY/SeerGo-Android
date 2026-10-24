using SeerGoGame.Interfaces;
using SeerGoGame.Systems;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 魔师迪露对方伤害百倍
    /// </summary>
    public class OpponentDamageBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType) 
        {
            return buffType == "OPPONENT_DAMAGE_100X";
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            return -10001f;
        }

        /// <summary>
        /// 检查是否令对方伤害100倍
        /// </summary>
        public static bool IsDamage100X(IBuffable character)
        {
            return character.ActiveBuffs.ContainsKey(30011); 
        }

    }
}
