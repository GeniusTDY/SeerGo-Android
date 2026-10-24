using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=49: 可以抵挡{0}点伤害"
    /// </summary>
    public class SideEffect_49 : ISkillSideEffectHandler
    {
        public override int EffectId => 49;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !float.TryParse(args[0], out float blockDamage))
            {
                SeerGoLogger.LogError($"[SideEffect_49] 参数解析失败");
                return;
            }

            // 应用异常免疫Buff
            int buffId = 111; // 异常免疫Buff ID
            
            buffSystem.ApplyBuff(caster, target, buffId, new Dictionary<string, object>
            {
                { "EffectValue", blockDamage }
            });

            SeerGoLogger.Log($"<color=yellow>[SideEffect_49] 获得{blockDamage}点护盾</color>");
        }
    }
}
