using SeerGoGame.Interfaces;
using SeerGoGame.Systems;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 免疫类Buff效果处理器 (能力防护、异常免疫)
    /// </summary>
    public class ImmuneBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType) 
        {
            return buffType == "STAT_IMMUNE" || buffType == "STATUS_IMMUNE" || buffType == "STATUS_CONTROL_IMMUNE" || buffType == "STATUS_WEAK_IMMUNE";
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            return -10001f;
        }

        /// <summary>
        /// 检查是否免疫能力下降
        /// </summary>
        public static bool IsImmuneToStatDown(IBuffable character)
        {
            return character.ActiveBuffs.ContainsKey(20003); // 能力防护Buff
        }

        /// <summary>
        /// 检查是否免疫异常状态
        /// </summary>
        public static bool IsImmuneToStatus(IBuffable character)
        {
            return character.ActiveBuffs.ContainsKey(20004); // 异常免疫Buff
        }

        /// <summary>
        /// 检查是否免疫控制类异常状态
        /// </summary>
        public static bool IsImmuneToControlStatus(IBuffable character)
        {
            return character.ActiveBuffs.ContainsKey(30003); // 控制类异常免疫Buff
        }

        /// <summary>
        /// 检查是否免疫弱化类异常状态
        /// </summary>
        public static bool IsImmuneToWeakStatus(IBuffable character)
        {
            return character.ActiveBuffs.ContainsKey(30004); // 弱化类异常免疫Buff
        }
    }
}
