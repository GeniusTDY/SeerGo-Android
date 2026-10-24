using SeerGoGame.Interfaces;
using SeerGoGame.Systems;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 技能闪避
    /// </summary>
    public class CaAttackMissBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType) 
        {
            return buffType == "CA_ATTACK_MISS";
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            return -10001f;
        }

        /// <summary>
        /// 检查是否闪避
        /// </summary>
        public static bool IsCaSkillMiss(IBuffable character)
        {
            return character.ActiveBuffs.ContainsKey(20028); 
        }

    }
}
