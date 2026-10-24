using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Systems;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 先制效果处理器
    /// </summary>
    public class PrioritySkillBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType)
        {
            return buffType == "PRIORITY_SKILL";
        }

        public static bool IsSkillPriority(IBuffable character)
        {
            return character.ActiveBuffs.ContainsKey(30010);
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            return -10001f;
        }
    }
}
