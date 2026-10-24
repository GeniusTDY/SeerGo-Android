using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=117: 5回合内，如果先出手，50%几率对手害怕1到3回合
    /// </summary>
    public class SideEffect_117 : ISkillSideEffectHandler
    {
        public override int EffectId => 117;

        public override void OnAfterBuffTurnEnd(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            // 判断本方是否先出手
            bool isFirst = caster is PlayerBattleController
                ? RoundContext.Current.Priority == TurnPriority.PlayerFirst
                : RoundContext.Current.Priority == TurnPriority.EnemyFirst;

            if (!isFirst)
            {
                SeerGoLogger.Log($"[SideEffect_117] 本回合未先手，不触发");
                return;
            }

            // 50%概率判定
            if (Random.Range(1, 101) > 50)
            {
                SeerGoLogger.Log($"[SideEffect_117] 概率判定失败");
                return;
            }

            // 对手害怕1到3回合
            int fearDuration = Random.Range(1, 4);
            buffSystem.ApplyBuff(caster, target, 7, new Dictionary<string, object>()
            {
                { "duration", fearDuration.ToString() }
            });
            SeerGoLogger.Log($"[SideEffect_117] 先手触发害怕，持续{fearDuration}回合");
        }
    }
}
