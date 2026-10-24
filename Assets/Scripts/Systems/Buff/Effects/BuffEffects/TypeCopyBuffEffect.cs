using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 属性同化Buff效果处理器
    /// 使自身的属性和目标的属性相同
    /// </summary>
    public class TypeCopyBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType)
        {
            return buffType == "TYPE_COPY";
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            return -10001f; // 不影响属性值
        }

        /// <summary>
        /// 检查角色是否有属性同化buff
        /// </summary>
        public static bool HasTypeCopyBuff(IBuffable character)
        {
            foreach (var buff in character.ActiveBuffs.Values)
            {
                if (buff.Config.type == "TYPE_COPY")
                {
                    return true;
                }
            }
            return false;
        }
    }
}
