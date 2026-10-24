using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=47: 能力防护 - {0}回合内免疫能力下降状态
    /// </summary>
    public class SideEffect_47 : ISkillSideEffectHandler
    {
        public override int EffectId => 47;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int duration))
            {
                SeerGoLogger.LogError($"[能力防护] 参数解析失败");
                return;
            }

            // 应用能力防护Buff
            int buffId = 20003; // 能力防护Buff ID
            
            buffSystem.ApplyBuff(caster, target, buffId, new Dictionary<string, object>
            {
                { "RemainingTurns", duration.ToString() }
            });

            SeerGoLogger.Log($"<color=yellow>[能力防护] 获得{duration}回合能力下降免疫</color>");
        }
    }
}
