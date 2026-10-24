using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=65: 伤害变化效果
    /// {0}回合内{1}系技能威力为{2}倍
    /// </summary>
    public class SideEffect_65 : ISkillSideEffectHandler
    {

        public override int EffectId => 65;

        public override void OnAfterBuffTurnEnd(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 3)
            {
                SeerGoLogger.LogError($"[SideEffect_65] 参数不足,需要2个参数");
                return;
            }

            if (!int.TryParse(args[0], out int duration) ||
                !int.TryParse(args[1], out int type) ||
                     !float.TryParse(args[2], out float multiple))
            {
                SeerGoLogger.LogError($"[SideEffect_65] 参数解析失败: {string.Join(",", args)}");
                return;
            }

            // 属性id
            string[] arg = { type.ToString() };
            // 应用buff
            buffSystem.ApplyBuff(caster, target, 20016, new Dictionary<string, object>
            {
                { "RemainingTurns",  duration.ToString()},
                { "EffectValue", multiple},
                { "Args", arg}
            });
        }

    }
}
