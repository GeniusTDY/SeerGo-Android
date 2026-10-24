using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=42: 伤害变化效果
    /// ID=42: {0}~{1}回合自己使用电招式伤害×2
    /// </summary>
    public class SideEffect_42 : ISkillSideEffectHandler
    {

        public override int EffectId => 42;

        public override void OnAfterBuffTurnEnd(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 2)
            {
                SeerGoLogger.LogError($"[DamageChangeSideEffect1] 参数不足,需要2个参数");
                return;
            }

            if (!int.TryParse(args[0], out int duration1) ||
                !int.TryParse(args[1], out int duration2))
            {
                SeerGoLogger.LogError($"[DamageChangeSideEffect1] 参数解析失败: {string.Join(",", args)}");
                return;
            }

            int duration = Random.Range(duration1, duration2 + 1);
            // 电系id
            string[] arg = { "5" };
            // 应用buff
            buffSystem.ApplyBuff(caster, target, 20016, new Dictionary<string, object>
            {
                { "RemainingTurns",  duration.ToString()},
                { "EffectValue", 2f},
                { "Args", arg}
            });
        }

    }
}
