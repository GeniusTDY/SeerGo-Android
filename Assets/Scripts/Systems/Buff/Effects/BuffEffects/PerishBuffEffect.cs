using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 延时反杀Buff效果处理器
    /// </summary>
    public class PerishBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType) => buffType == "PERISH";

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            return -10001f;
        }

        public override void OnTurnEnd(IBuffable character, BattleEntityController opponent, BuffInstance buff)
        {
            // 回合结束时检查倒计时
            if (buff.RemainingTurns == 1) // 下回合结束时会变成0
            {
                if (character is PlayerBattleController player)
                {
                    PetData petData = player.GetCurrentPetData();
                    if (petData != null)
                    {
                        player.TakeDamage(petData.CurrentStatus.Hp); // 更新UI
                        SeerGoLogger.Log($"<color=red>[延时反杀] {petData.Basic.Name}倒计时结束，死亡！</color>");
                    }
                }
                else if (character is MonsterBattleController enemy)
                {
                    PetData petData = enemy.GetCurrentPetData();
                    if (petData != null)
                    {
                        enemy.TakeDamage(petData.CurrentStatus.Hp); // 更新UI
                        SeerGoLogger.Log($"<color=red>[延时反杀] {petData.Basic.Name}倒计时结束，死亡！</color>");
                    }
                }
            }
        }
    }
}
