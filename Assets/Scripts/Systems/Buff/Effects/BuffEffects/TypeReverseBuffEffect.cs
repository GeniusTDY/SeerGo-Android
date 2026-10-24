using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using System.Linq;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 属性反转Buff效果处理器
    /// 在伤害计算时，攻守双方的属性互换
    /// </summary>
    public class TypeReverseBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType)
        {
            return buffType == "TYPE_REVERSE";
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            return -10001f; // 不影响属性值
        }

        /// <summary>
        /// 获取反转后的属性ID
        /// 如果施法者和目标都有TYPE_REVERSE buff，则属性互换
        /// </summary>
        /// <param name="attacker">攻击方</param>
        /// <param name="defender">防御方</param>
        /// <returns>是否发生了属性反转</returns>
        public static int GetReversedTypesCounts(IBuffable attacker, IBuffable defender)
        {
            int counts = 0;
            if (attacker.ActiveBuffs.Values.Any(buff => buff.Config.type == "TYPE_REVERSE")) counts++;
            if (defender.ActiveBuffs.Values.Any(buff => buff.Config.type == "TYPE_REVERSE")) counts++;
            return counts;
        }

    }
}
