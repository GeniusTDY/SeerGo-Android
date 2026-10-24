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
    /// 副作用ID=104: {0}回合内，每次直接攻击都有{1}%几率附带衰弱效果
    /// </summary>
    public class SideEffect_104 : ISkillSideEffectHandler
    {
        public override int EffectId => 104;

        public override void OnAfterBuffTurnEnd(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 2 || !int.TryParse(args[0], out int duration) || !int.TryParse(args[1], out int probability))
            {
                SeerGoLogger.LogError($"[SideEffect_104] 参数解析失败: {string.Join(",", args)}");
                return;
            }

            // 概率判定
            bool success = (probability >= 100) || (Random.Range(1, 101) <= probability);
            if (!success)
            {
                SeerGoLogger.Log($"[SideEffect_104] 概率判定失败 ({probability}%)");
                return;
            }

            // 检查本方本回合是否造成了伤害
            bool dealtDamage = caster is PlayerBattleController
                ? RoundContext.Current.PlayerDamage > 0
                : RoundContext.Current.EnemyDamage > 0;

            if (!dealtDamage)
            {
                SeerGoLogger.Log($"[SideEffect_104] 本回合未造成伤害，不触发");
                return;
            }

            // 给对手挂衰弱
            buffSystem.ApplyBuff(caster, target, 20033, new Dictionary<string, object>
            {
                { "EffectValue", 1f }
            });
            SeerGoLogger.Log($"[SideEffect_104] 造成伤害，附带衰弱效果");
        }
    }
}
