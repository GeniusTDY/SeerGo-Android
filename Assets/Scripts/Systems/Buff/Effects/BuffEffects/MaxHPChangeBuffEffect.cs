using SeerGoGame.Interfaces;
using SeerGoGame.Systems;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 最大生命值降低
    /// </summary>
    public class MaxHPChangeBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType)
        {
            return buffType == "MAXHP" || buffType == "MAXHP_MULTIPLE";
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            if (buff.Config.type != statType)
            {
                return -10001f;
            }
            if (buff.Config.type == "MAXHP")
            {
                return -buff.EffectValue;
            }

            if (buff.Config.type == "MAXHP_MULTIPLE")
            {
                return buff.EffectValue;
            }
            return -10001f;
        }
    }
}
