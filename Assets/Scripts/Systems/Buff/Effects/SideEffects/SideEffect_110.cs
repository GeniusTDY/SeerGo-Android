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
    /// 副作用ID=110: {0}回合内，每次躲避攻击都有{1}%几率使自身{2}提升1个等级
    /// args[0]=duration, args[1]=probability, args[2]=statType
    /// </summary>
    public class SideEffect_110 : ISkillSideEffectHandler
    {
        public override int EffectId => 110;

        public override void OnAfterBuffTurnEnd(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 3 || !int.TryParse(args[0], out int duration) || !int.TryParse(args[1], out int probability))
            {
                SeerGoLogger.LogError($"[SideEffect_110] 参数解析失败: {string.Join(",", args)}");
                return;
            }

            // 检查本方是否躲避了攻击
            bool dodged = caster is PlayerBattleController
                ? (!RoundContext.Current.IsEnemySkillHit)
                : (!RoundContext.Current.IsPlayerSkillHit);

            if (!dodged)
            {
                SeerGoLogger.Log($"[SideEffect_110] 本回合未躲避攻击，不触发");
                return;
            }

            // 概率判定
            bool success = (probability >= 100) || (Random.Range(1, 101) <= probability);
            if (!success)
            {
                SeerGoLogger.Log($"[SideEffect_110] 概率判定失败 ({probability}%)");
                return;
            }

            int statBuffId = GetStatBuffId(args[2]);
            if (statBuffId == -1)
            {
                SeerGoLogger.LogError($"[SideEffect_110] 未知属性类型: {args[2]}");
                return;
            }

            buffSystem.ApplyBuff(caster, target, statBuffId, new Dictionary<string, object>
            {
                { "EffectValue", 1f }
            });

            SeerGoLogger.Log($"[SideEffect_110] 躲避攻击，{args[2]}提升1级");
        }

        private int GetStatBuffId(string statType)
        {
            return statType switch
            {
                "atk" => 10000,
                "def" => 10001,
                "spAtk" => 10002,
                "spDef" => 10003,
                "spd" => 10004,
                "accuracy" => 10005,
                "critRate" => 10006,
                _ => -1
            };
        }
    }
}
