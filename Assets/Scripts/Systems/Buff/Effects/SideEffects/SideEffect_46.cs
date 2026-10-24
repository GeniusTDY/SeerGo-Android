using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=46: 抵挡 - 抵挡下{0}次对手的攻击
    /// 需要通过Buff记录剩余抵挡次数
    /// </summary>
    public class SideEffect_46 : ISkillSideEffectHandler
    {
        public override int EffectId => 46;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int blockCount))
            {
                SeerGoLogger.LogError($"[抵挡] 参数解析失败");
                return;
            }

            // 应用抵挡Buff
            int buffId = 20002; // 抵挡Buff ID
            
            buffSystem.ApplyBuff(caster, target, buffId, new Dictionary<string, object>
            {
                { "RemainingTurns", blockCount.ToString()} 
            });

            SeerGoLogger.Log($"<color=yellow>[抵挡] 获得{blockCount}次攻击抵挡</color>");
        }
    }
}
