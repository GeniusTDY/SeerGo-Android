using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=30: 报复 - 后出手的话威力为2倍
    /// </summary>
    public class SideEffect_30 : ISkillSideEffectHandler
    {
        public override int EffectId => 30;

        public override void OnBeforeDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (caster is PlayerBattleController && RoundContext.Current.Priority == TurnPriority.EnemyFirst)
            {
                // 玩家后手，威力翻倍
                RoundContext.Current.PlayerSkillPower *= 2;
                SeerGoLogger.Log($"<color=yellow>[报复效果] 玩家后手，威力翻倍至{RoundContext.Current.PlayerSkillPower}!</color>");
            }
            else if (caster is MonsterBattleController && RoundContext.Current.Priority == TurnPriority.PlayerFirst)
            {
                // 敌方后手，威力翻倍
                RoundContext.Current.EnemySkillPower *= 2;
                SeerGoLogger.Log($"<color=yellow>[报复效果] 敌方后手，威力翻倍至{RoundContext.Current.EnemySkillPower}!</color>");
            }
        }
    }
}
