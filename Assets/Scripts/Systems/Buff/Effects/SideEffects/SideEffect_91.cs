using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=91: {0}回合内，对手的状态变化会同时作用在自己身上
    /// </summary>
    public class SideEffect_91 : ISkillSideEffectHandler
    {
        public override int EffectId => 91;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int duration))
            {
                SeerGoLogger.LogError($"[SideEffect_91] 参数解析失败: {string.Join(",", args)}");
                return;
            }

            // 给施法者自身添加 STAT_MIRROR buff，持续 duration 回合
            buffSystem.ApplyBuff(caster, target, 30015, new Dictionary<string, object>()
            {
                { "RemainingTurns", duration.ToString() },
                { "TargetType", BuffTargetType.Self }
            });

            SeerGoLogger.Log($"[SideEffect_91] 属性同步生效: {duration}回合内，对手的状态变化会同时作用在自己身上");
        }
    }
}
