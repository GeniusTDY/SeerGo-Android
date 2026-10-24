using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=77: {0}回合内若使用技能则恢复{1}点体力值
    /// </summary>
    public class SideEffect_77 : ISkillSideEffectHandler
    {
        public override int EffectId => 77;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 2 || !int.TryParse(args[0], out int duration) || !float.TryParse(args[1], out float hp))
            {
                SeerGoLogger.LogError($"[SideEffect_77] 参数解析失败");
                return;
            }

            int buffId = 20026; 
            
            buffSystem.ApplyBuff(caster, target, buffId, new Dictionary<string, object>
            {
                { "RemainingTurns", duration.ToString() },
                { "EffectValue", hp }
            });

            SeerGoLogger.Log($"<color=yellow>[SideEffect_77] 恢复体力，持续{duration}回合</color>");
        }
    }
}
