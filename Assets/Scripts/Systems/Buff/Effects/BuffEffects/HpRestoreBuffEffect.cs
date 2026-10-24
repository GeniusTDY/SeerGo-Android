using SeerGoGame.Interfaces;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 血量低于100时恢复全部体力一次
    /// </summary>
    public class HpRestoreBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType)
        {
            return buffType == "HP_BELOW_RESTORE";
        }
        public override void OnTurnStart(IBuffable character, BattleEntityController opponent, BuffInstance buff)
        {
            if (character is BattleEntityController controller)
            {
                var petStatus = controller.GetCurrentPetData().CurrentStatus;
                SeerGoLogger.Log($"<color=orange>[血量低于100恢复一次] 玩家精灵当前HP: {petStatus.Hp}</color>");

                if (petStatus.Hp > 0 && petStatus.Hp <= (int)buff.EffectValue)
                {
                    SeerGoLogger.Log($"<color=orange>[血量低于100恢复一次] 恢复全部体力{(int)buff.EffectValue}</color>");
                    controller.TakeDamage(controller.GetMaxHp(), "+");
                }
            }
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            return -10001f;
        }
    }
}
