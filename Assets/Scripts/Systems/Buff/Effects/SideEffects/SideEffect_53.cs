using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=53: {0}回合攻击伤害是正常状态下的{1}倍"
    /// </summary>
    public class SideEffect_53 : ISkillSideEffectHandler
    {
        public override int EffectId => 53;

        public override void OnAfterBuffTurnEnd(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 2 || !int.TryParse(args[0], out int duration) || !float.TryParse(args[1], out float multiple))
            {
                SeerGoLogger.LogError($"[SideEffect_53] 参数解析失败");
                return;
            }

            int buffId = 20019; 

            buffSystem.ApplyBuff(caster, target, buffId, new Dictionary<string, object>
            {
                { "RemainingTurns", duration.ToString() },
                { "EffectValue", multiple }
            });
            
            SeerGoLogger.Log($"<color=yellow>[SideEffect_53] 获得{duration}回合伤害{multiple}倍效果</color>");
        }
    }
}
