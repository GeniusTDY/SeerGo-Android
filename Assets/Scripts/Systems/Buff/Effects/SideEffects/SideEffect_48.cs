using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=48: 异常免疫 - {0}回合内免疫所有受到的异常状态
    /// </summary>
    public class SideEffect_48 : ISkillSideEffectHandler
    {
        public override int EffectId => 48;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int duration))
            {
                SeerGoLogger.LogError($"[异常免疫] 参数解析失败");
                return;
            }

            // 应用异常免疫Buff
            int buffId = 20004; // 异常免疫Buff ID
            
            buffSystem.ApplyBuff(caster, target, buffId, new Dictionary<string, object>
            {
                { "RemainingTurns", duration.ToString() }
            });

            SeerGoLogger.Log($"<color=yellow>[异常免疫] 获得{duration}回合异常状态免疫</color>");
        }
    }
}
