using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=54: {0}回合使对方攻击伤害是正常状态下的1/{1}倍
    /// </summary>
    public class SideEffect_54 : ISkillSideEffectHandler
    {
        public override int EffectId => 54;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 2 || !int.TryParse(args[0], out int duration) || !float.TryParse(args[1], out float multiple))
            {
                SeerGoLogger.LogError($"[SideEffect_54] 参数解析失败");
                return;
            }

            float damageMultiple = 1 / multiple;

            int buffId = 20020; 

            buffSystem.ApplyBuff(caster, target, buffId, new Dictionary<string, object>
            {
                { "RemainingTurns", duration.ToString() },
                { "EffectValue", damageMultiple }
            });
            
            SeerGoLogger.Log($"<color=yellow>[SideEffect_54] 获得{duration}回合伤害{damageMultiple}倍降低效果</color>");
        }
    }
}
