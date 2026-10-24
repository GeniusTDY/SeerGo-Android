using SeerGoGame.Interfaces;
using SeerGoGame.Systems;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 必定先手Buff效果处理器
    /// </summary>
    public class AttackMustFirstBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType)
        {
            return buffType == "ATTACK_MUST_FIRST";
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            return -10001f;
        }

        /// <summary>
        /// 检查是否有必定先手效果
        /// </summary>
        public static bool HasAttackMustFirst(IBuffable character)
        {
            foreach (var buff in character.ActiveBuffs.Values)
            {
                if (buff.Config.type == "ATTACK_MUST_FIRST")
                {
                    return true;
                }
            }
            return false;
        }
    }
}
