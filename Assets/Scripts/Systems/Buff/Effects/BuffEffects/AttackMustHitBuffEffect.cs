using SeerGoGame.Interfaces;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 必定命中Buff效果处理器
    /// </summary>
    public class AttackMustHitBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType) 
        {
            return buffType == "ATTACK_MUST_HIT";
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            return -10001f;
        }

        /// <summary>
        /// 检查是否有必定命中效果
        /// </summary>
        public static bool HasAttackMustHit(IBuffable character)
        {
            foreach (var buff in character.ActiveBuffs.Values)
            {
                if (buff.Config.type == "ATTACK_MUST_HIT")
                {
                    return true;
                }
            }
            return false;
        }
    }
}
