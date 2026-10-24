using SeerGoGame.Interfaces;
using SeerGoGame.Systems;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 免疫类Buff效果处理器 (技能)
    /// </summary>
    public class ImmuneSkillBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType) 
        {
            return buffType == "IMMUNE_SKILL" || buffType == "IMMUNE_LIFE_BOND_SKILL"|| buffType == "IMMUNE_ZHG_SKILL";
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            return -10001f;
        }

        /// <summary>
        /// 检查是否免疫百分比伤害技能
        /// </summary>
        public static bool IsImmuneToSkill(IBuffable character)
        {
            return character.ActiveBuffs.ContainsKey(30001); // 技能免疫
        }
        /// <summary>
        /// 检查是否免疫同生共死技能
        /// </summary>
        public static bool IsImmuneToLifeBondSkill(IBuffable character)
        {
            return character.ActiveBuffs.ContainsKey(30009); // 技能免疫
        }
        /// <summary>
        /// 检查是否免疫镇魂歌
        /// </summary>
        public static bool IsImmuneToZHGSkill(IBuffable character)
        {
            return character.ActiveBuffs.ContainsKey(300016); // 技能免疫
        }
    }
}
