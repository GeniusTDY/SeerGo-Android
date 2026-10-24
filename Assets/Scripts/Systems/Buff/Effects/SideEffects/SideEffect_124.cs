using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=124: 命中后，{0}%几率随机对方一个属性{1}个等级
    /// args[0]=probability, args[1]=changeAmount (负数=降低)
    /// </summary>
    public class SideEffect_124 : ISkillSideEffectHandler
    {
        private static readonly int[] StatBuffIds = { 10000, 10001, 10002, 10003, 10004, 10005, 10006 };

        public override int EffectId => 124;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 2 || !int.TryParse(args[0], out int probability) || !int.TryParse(args[1], out int changeAmount))
            {
                SeerGoLogger.LogError($"[SideEffect_124] 参数解析失败: {string.Join(",", args)}");
                return;
            }

            bool success = (probability >= 100) || (Random.Range(1, 101) <= probability);
            if (!success)
            {
                SeerGoLogger.Log($"[SideEffect_124] 概率判定失败 ({probability}%)");
                return;
            }

            // 随机选一个属性
            int randomId = StatBuffIds[Random.Range(0, StatBuffIds.Length)];

            buffSystem.ApplyBuff(caster, target, randomId, new Dictionary<string, object>
            {
                { "EffectValue", (float)changeAmount }
            });

            SeerGoLogger.Log($"[SideEffect_124] 随机属性变化: buffId={randomId}, 变化={changeAmount}");
        }
    }
}
