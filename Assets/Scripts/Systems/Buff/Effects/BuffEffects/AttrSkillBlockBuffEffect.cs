using SeerGoGame.Interfaces;
using SeerGoGame.Systems;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 属性技能封锁效果处理器（对应buff ID=20041, type=ATTR_SKILL_BLOCK）
    /// 使对手的属性技能（Category=0的属性变化类技能）失效
    /// </summary>
    public class AttrSkillBlockBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType)
        {
            return buffType == "ATTR_SKILL_BLOCK";
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            return -10001f;
        }

        /// <summary>
        /// 检查属性技能是否被封锁
        /// </summary>
        public static bool IsAttrSkillBlocked(IBuffable character)
        {
            return character.ActiveBuffs.ContainsKey(20041);
        }
    }
}
