using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=116: 3回合内，如果先出手，伤害的20%恢复体力
    /// </summary>
    public class SideEffect_116 : ISkillSideEffectHandler
    {
        public override int EffectId => 116;

        public override void OnAfterBuffTurnEnd(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            // 判断本方是否先出手
            bool isFirst = caster is PlayerBattleController
                ? RoundContext.Current.Priority == TurnPriority.PlayerFirst
                : RoundContext.Current.Priority == TurnPriority.EnemyFirst;

            if (!isFirst)
            {
                SeerGoLogger.Log($"[SideEffect_116] 本回合未先手，不触发");
                return;
            }

            // 恢复伤害的20%
            if (caster is PlayerBattleController player && RoundContext.Current.PlayerDamage > 0)
            {
                int heal = (int)(RoundContext.Current.PlayerDamage * 0.2f);
                player.TakeDamage(heal, "+");
                SeerGoLogger.Log($"[SideEffect_116] 先手回血: {heal}");
            }
            else if (caster is MonsterBattleController monster && RoundContext.Current.EnemyDamage > 0)
            {
                int heal = (int)(RoundContext.Current.EnemyDamage * 0.2f);
                monster.TakeDamage(heal, "+");
                SeerGoLogger.Log($"[SideEffect_116] 先手回血: {heal}");
            }
        }
    }
}
