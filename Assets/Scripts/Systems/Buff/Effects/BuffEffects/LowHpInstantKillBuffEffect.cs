using SeerGoGame.Interfaces;
using SeerGoGame.Systems;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 魔师迪露低血秒杀
    /// </summary>
    public class LowHpInstantKillBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType)
        {
            return buffType == "LOW_HP_INSTANT_KILL";
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            return -10001f;
        }

        /// <summary>
        /// 检查是否低血秒杀
        /// </summary>
        public static bool IsLowHpInstantKill(IBuffable character)
        {
            if (character.ActiveBuffs.TryGetValue(30012, out var buffValue) &&
                character is BattleEntityController battle)
            {
                int hp = (int)(battle.GetMaxHp() * (1f / buffValue.EffectValue));
                return battle.GetCurrentPetData().CurrentStatus.Hp < hp;
            }
            return false;
        }

    }
}
