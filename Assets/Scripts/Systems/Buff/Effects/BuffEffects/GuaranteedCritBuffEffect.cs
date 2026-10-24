using SeerGoGame.Interfaces;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 必定暴击/牺牲暴击Buff效果处理器
    /// </summary>
    public class GuaranteedCritBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType)
        {
            return buffType == "GUARANTEED_CRIT" || buffType == "SACRIFICE_CRIT" || buffType == "HP_LOW_GUARANTEED_CRIT";
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            return -10001f;
        }

        /// <summary>
        /// 检查是否有必定暴击效果
        /// </summary>
        public static bool HasGuaranteedCrit(IBuffable character)
        {
            foreach (var buff in character.ActiveBuffs.Values)
            {
                if (buff.Config.type == "GUARANTEED_CRIT" || buff.Config.type == "SACRIFICE_CRIT")
                {
                    return true;
                }
                else if (buff.Config.type == "HP_LOW_GUARANTEED_CRIT")
                {
                    if (!(character is BattleEntityController controller))
                        return false;
                    // 血量低于指定值时触发
                    if (controller.GetCurrentPetData().CurrentStatus.Hp < (int)(controller.GetMaxHp() * buff.EffectValue))
                    {
                        SeerGoLogger.Log("有必定暴击效果");
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
