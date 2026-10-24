using SeerGoGame.Interfaces;
using SeerGoGame.Systems;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 技能失效
    /// </summary>
    public class SkillBlockBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType) 
        {
            return buffType == "SKILL_BLOCK";
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            return -10001f;
        }

        /// <summary>
        /// 检查是否技能失效
        /// </summary>
        public static bool IsSkillBlocked(IBuffable character)
        {
            return character.ActiveBuffs.ContainsKey(20005); 
        }

    }
}
