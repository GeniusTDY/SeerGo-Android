using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 性别伤害倍数处理器：对雄性精灵伤害翻倍（对有GENDER_DAMAGE_MOD buff的一方生效）
    /// </summary>
    public class GenderDamageBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType)
        {
            return buffType == "GENDER_DAMAGE_MOD";
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            if (buff.Config.type != statType)
                return -10001f;

            // 获取当前回合的对手
            BattleEntityController opponent = null;
            if (character is PlayerBattleController)
            {
                opponent = BattleManageController.Instance?.monsterBattleController;
            }
            else if (character is MonsterBattleController)
            {
                opponent = BattleManageController.Instance?.playerBattleController;
            }

            if (opponent == null) return -10001f;

            // 检查对手精灵性别：1=雄性
            var petData = opponent.GetCurrentPetData();
            if (petData != null && petData.Basic.Gender == 1)
            {
                SeerGoLogger.Log($"[性别伤害] 对手雄性，伤害倍率 x{buff.EffectValue}");
                return buff.EffectValue;
            }

            return -10001f;
        }
    }
}
