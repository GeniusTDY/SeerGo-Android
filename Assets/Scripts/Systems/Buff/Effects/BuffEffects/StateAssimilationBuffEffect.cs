using SeerGoGame.Interfaces;
using SeerGoGame.Systems;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 防御，攻击同化
    /// </summary>
    public class StateAssimilationBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType) 
        {
            return buffType == "DEFENSE_ASSIMILATION" || buffType == "ATTACK_ASSIMILATION";
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            return -10001f;
        }

        /// <summary>
        /// 检查是否防御同化
        /// </summary>
        public static bool IsDefAssimilation(IBuffable character)
        {
            return character.ActiveBuffs.ContainsKey(20017);
        }

        /// <summary>
        /// 检查是否攻击同化
        /// </summary>
        public static bool IsAtkAssimilation(IBuffable character)
        {
            return character.ActiveBuffs.ContainsKey(20018); 
        }
    }
}
