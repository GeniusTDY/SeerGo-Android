using SeerGoGame.Interfaces;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 每回合回血效果处理器（对应buff ID=20046, type=TURN_HEAL）
    /// 每回合行动开始时恢复自身固定体力值
    /// </summary>
    public class TurnHealBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType)
        {
            return buffType == "TURN_HEAL";
        }

        public override void OnTurnEnd(IBuffable character, BattleEntityController opponent, BuffInstance buff)
        {
            if (character is BattleEntityController controller)
            {
                int healAmount = (int)buff.EffectValue;
                controller.TakeDamage(healAmount, "+");
                SeerGoLogger.Log($"<color=green>[每回合回血] {controller.GetEntityName()} 恢复 {healAmount} 点体力</color>");
            }
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            return -10001f;
        }
    }
}
