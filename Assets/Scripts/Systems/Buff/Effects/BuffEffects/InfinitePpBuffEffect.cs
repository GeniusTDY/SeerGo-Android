using SeerGoGame.Interfaces;
using SeerGoGame.Systems;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 无限PP
    /// </summary>
    public class InfinitePpBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType) 
        {
            return buffType == "INFINITE_PP";
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            return -10001f;
        }

        /// <summary>
        /// 检查是否无限PP
        /// </summary>
        public static bool IsInfinitePp(IBuffable character)
        {
            return character.ActiveBuffs.ContainsKey(30006); 
        }

    }
}
