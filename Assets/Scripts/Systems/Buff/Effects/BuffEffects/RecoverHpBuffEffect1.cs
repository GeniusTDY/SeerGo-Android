using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Systems;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 使用技能恢复生命值
    /// </summary>
    public class RecoverHpBuffEffect1 : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType)
        {
            return buffType == "ROUND_SKILL_HEAL";
        }

        public override void OnTurnEnd(IBuffable character, BattleEntityController opponent, BuffInstance buff)
        {
            if (character is PlayerBattleController player)
            {
                if(RoundContext.Current.PlayerSkill == null)
                    return;

                player.TakeDamage((int)buff.EffectValue, "+");
            }
            else if (character is MonsterBattleController monster)
            {
                if (RoundContext.Current.EnemySkill == null)
                    return;

                monster.TakeDamage((int)buff.EffectValue, "+");
            }
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            return -10001f;
        }
    }
}
