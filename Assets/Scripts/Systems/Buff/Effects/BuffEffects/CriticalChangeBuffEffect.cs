using SeerGoGame.Interfaces;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 要害，暴击率带回合数
    /// </summary>
    public class CriticalChangeBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType)
        {
            return buffType == "CRITICAL" || buffType == "BOSS_CRITICAL";
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            if (buff.Config.type != statType)
            {
                return -10001f;
            }
            if (buff.Config.type == "CRITICAL")
            {
                return buff.EffectValue;
            }
            if (buff.Config.type == "BOSS_CRITICAL")
            {
                return buff.EffectValue;
            }

            return -10001f;
        }
    }
}
