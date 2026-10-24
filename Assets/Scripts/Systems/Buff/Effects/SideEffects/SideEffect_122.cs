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
    /// 副作用ID=122: 先出手时，{1}%改变对方{0}等级{2}
    /// args[0]=statType, args[1]=probability, args[2]=changeAmount
    /// </summary>
    public class SideEffect_122 : ISkillSideEffectHandler
    {
        public override int EffectId => 122;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 3 || !int.TryParse(args[1], out int probability) || !int.TryParse(args[2], out int changeAmount))
            {
                SeerGoLogger.LogError($"[SideEffect_122] 参数解析失败: {string.Join(",", args)}");
                return;
            }

            // 判断是否先出手
            bool isFirst = caster is PlayerBattleController
                ? RoundContext.Current.Priority == TurnPriority.PlayerFirst
                : RoundContext.Current.Priority == TurnPriority.EnemyFirst;

            if (!isFirst)
            {
                SeerGoLogger.Log($"[SideEffect_122] 未先手，不触发");
                return;
            }

            bool success = (probability >= 100) || (Random.Range(1, 101) <= probability);
            if (!success)
            {
                SeerGoLogger.Log($"[SideEffect_122] 概率判定失败 ({probability}%)");
                return;
            }

            int statBuffId = GetStatBuffId(args[0]);
            if (statBuffId == -1)
            {
                SeerGoLogger.LogError($"[SideEffect_122] 未知属性类型: {args[0]}");
                return;
            }

            buffSystem.ApplyBuff(caster, target, statBuffId, new Dictionary<string, object>
            {
                { "EffectValue", (float)changeAmount }
            });

            SeerGoLogger.Log($"[SideEffect_122] 先手触发: 对方{args[0]}变化{changeAmount}");
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
