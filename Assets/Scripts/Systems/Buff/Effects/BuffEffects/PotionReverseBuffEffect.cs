using SeerGoGame.Interfaces;
using SeerGoGame.Systems;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 对手使用体力药剂时效果变成减少相应的体力
    /// </summary>
    public class PotionReverseBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType) 
        {
            return buffType == "POTION_REVERSE";
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            return -10001f;
        }

        /// <summary>
        /// 检查是否药剂反噬
        /// </summary>
        public static bool IsPotionReverse(IBuffable character)
        {
            return character.ActiveBuffs.ContainsKey(20024); 
        }
    }
}
