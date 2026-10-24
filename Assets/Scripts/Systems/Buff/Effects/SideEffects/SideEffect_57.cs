using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=57: {0}回合内每回合使用技能恢复自身最大体力的1/{1}
    /// </summary>
    public class SideEffect_57 : ISkillSideEffectHandler
    {
        public override int EffectId => 57;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 2 || !int.TryParse(args[0], out int duration) || !float.TryParse(args[1], out float multiple))
            {
                SeerGoLogger.LogError($"[SideEffect_57] 参数解析失败");
                return;
            }

            int buffId = 20021; 
            
            buffSystem.ApplyBuff(caster, target, buffId, new Dictionary<string, object>
            {
                { "RemainingTurns", duration.ToString() },
                { "EffectValue", multiple }
            });

            SeerGoLogger.Log($"<color=yellow>[SideEffect_57] 恢复体力，持续{duration}回合</color>");
        }
    }
}
