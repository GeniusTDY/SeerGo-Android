using SeerGoGame.Interfaces;
using SeerGoGame.Systems;

namespace SeerGoGame.Buffs
{
    // 控制类型异常
    public class ControlTypeBuffEffect : IBuffEffectHandler
    {

        public override bool CanHandle(string buffType)
        {
            return buffType == "PARALYSIS" || buffType == "TIRED" || buffType == "FEAR" || buffType == "SLEEP" || buffType == "PETRIFY";
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            return -10001f;
        }

        /// <summary>
        /// 检查是否有控制效果
        /// </summary>
        public static bool HasControlTypeStatus(IBuffable character)
        {
            foreach (var buff in character.ActiveBuffs.Values)
            {
                if (buff.Config.type == "PARALYSIS" || buff.Config.type == "TIRED" || buff.Config.type == "FEAR" || buff.Config.type == "SLEEP" || buff.Config.type == "PETRIFY")
                {
                    return true;
                }
            }
            return false;
        }
    }
}
