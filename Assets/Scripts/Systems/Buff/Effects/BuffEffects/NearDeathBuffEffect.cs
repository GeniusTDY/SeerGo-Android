using SeerGoGame.Interfaces;
using SeerGoGame.Systems;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 受到致死攻击时则余下1点体力
    /// </summary>
    public class NearDeathBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType) 
        {
            return buffType == "NEAR_DEATH";
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            return -10001f;
        }

        /// <summary>
        /// 检查是否濒死
        /// </summary>
        public static bool IsNearDeath(IBuffable character)
        {
            return character.ActiveBuffs.ContainsKey(20023); 
        }
    }
}
