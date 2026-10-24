using SeerGoGame.Interfaces;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 属性变化效果处理器 (攻击/防御/速度 上升/下降)
    /// </summary>
    public class StatChangeBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType)
        {
            return buffType == "ATK" ||
                   buffType == "DEF" ||
                   buffType == "SPEED" ||
                   buffType == "SPATK" ||
                   buffType == "SPDEF" ||
                   buffType == "ACCURACY" ||
                   buffType == "CRITRATE" ||
                   buffType == "BOSS_ATK" ||
                   buffType == "BOSS_DEF" ||
                   buffType == "BOSS_SPDEF" ||
                   buffType == "BOSS_SPATK" ||
                   buffType == "BOSS_SPEED"
                   ;
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            if (buff.Config.type != statType)
            {
                return -10001f;
            }

            if (buff.Config.type == "ACCURACY")
            {
                return LevelEffectSystem.GetAccuracyLevelMultiplier((int)buff.EffectValue);
            }

            if (buff.Config.type == "CRITRATE")
            {
                return buff.EffectValue;
            }

            if (buff.Config.type == "ATK" || buff.Config.type == "DEF" || buff.Config.type == "SPEED" || buff.Config.type == "SPATK" || buff.Config.type == "SPDEF")
            {
                return LevelEffectSystem.GetActualMultiplier((int)buff.EffectValue);
            }

            if (buff.Config.type == "BOSS_ATK" ||
                buff.Config.type == "BOSS_DEF" ||
                buff.Config.type == "BOSS_SPDEF" ||
                buff.Config.type == "BOSS_SPATK" ||
                buff.Config.type == "BOSS_SPEED")
            {
                return LevelEffectSystem.GetActualMultiplier((int)buff.EffectValue);
            }

            SeerGoLogger.LogError($"未知的属性类型: {buff.Config.type}");
            return -10001f;
        }
    }
}
