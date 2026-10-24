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
    /// 副作用ID=109: {0}回合内，给对手造成伤害时，有{1}%几率令对手冻伤
    /// </summary>
    public class SideEffect_109 : ISkillSideEffectHandler
    {
        public override int EffectId => 109;

        public override void OnAfterBuffTurnEnd(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 2 || !int.TryParse(args[0], out int duration) || !int.TryParse(args[1], out int probability))
            {
                SeerGoLogger.LogError($"[SideEffect_109] 参数解析失败: {string.Join(",", args)}");
                return;
            }

            // 概率判定
            bool success = (probability >= 100) || (Random.Range(1, 101) <= probability);
            if (!success)
            {
                SeerGoLogger.Log($"[SideEffect_109] 概率判定失败 ({probability}%)");
                return;
            }

            // 检查本方本回合是否造成了伤害
            bool dealtDamage = caster is PlayerBattleController
                ? RoundContext.Current.PlayerDamage > 0
                : RoundContext.Current.EnemyDamage > 0;

            if (!dealtDamage)
            {
                SeerGoLogger.Log($"[SideEffect_109] 本回合未造成伤害，不触发");
                return;
            }

            // 给对手挂冻伤
            buffSystem.ApplyBuff(caster, target, 2, new Dictionary<string, object>()
            {
                { "duration", duration.ToString() }
            });
            SeerGoLogger.Log($"[SideEffect_109] 造成伤害，触发冻伤效果(持续{duration}回合)");
        }
    }
}
