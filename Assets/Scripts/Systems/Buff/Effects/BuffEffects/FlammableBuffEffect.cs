using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 易燃效果处理器：命中降低30%，受到火系伤害则100%烧伤
    /// </summary>
    public class FlammableBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType)
        {
            return buffType == "FLAMMABLE";
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            // 易燃效果：命中率受到影响
            if (statType == "ACCURACY")
            {
                return buff.EffectValue; // -0.3 (降低30%命中)
            }
            return -10001f;
        }

        public override void OnTurnEnd(IBuffable character, BattleEntityController opponent, BuffInstance buff)
        {
            // 易燃无需每回合处理，通过GetStatMultiplier和战斗系统的火系伤害检测生效
        }
    }
}
