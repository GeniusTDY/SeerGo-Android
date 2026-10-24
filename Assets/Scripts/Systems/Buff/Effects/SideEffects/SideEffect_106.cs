using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=106: {0}回合内，特殊攻击对自身必定miss
    /// </summary>
    public class SideEffect_106 : ISkillSideEffectHandler
    {
        public override int EffectId => 106;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int duration))
            {
                SeerGoLogger.LogError($"[SideEffect_106] 参数解析失败: {string.Join(",", args)}");
                return;
            }

            // 复用特攻miss buff (20028)
            buffSystem.ApplyBuff(caster, target, 20028, new Dictionary<string, object>
            {
                { "RemainingTurns", duration.ToString() }
            });

            SeerGoLogger.Log($"[SideEffect_106] 特殊攻击miss生效，持续{duration}回合");
        }
    }
}
