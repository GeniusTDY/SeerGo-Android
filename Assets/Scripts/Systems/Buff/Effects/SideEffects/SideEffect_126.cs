using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=126: {0}回合内，每回合自身攻击和速度提高{1}个等级
    /// args[0]=duration, args[1]=changeAmount
    /// </summary>
    public class SideEffect_126 : ISkillSideEffectHandler
    {
        public override int EffectId => 126;

        public override void OnAfterBuffTurnEnd(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 2 || !int.TryParse(args[0], out int duration) || !int.TryParse(args[1], out int changeAmount))
            {
                SeerGoLogger.LogError($"[SideEffect_126] 参数解析失败: {string.Join(",", args)}");
                return;
            }

            // 每回合自动提升攻击和速度
            buffSystem.ApplyBuff(caster, target, 10000, new Dictionary<string, object>
            {
                { "EffectValue", (float)changeAmount }
            });
            buffSystem.ApplyBuff(caster, target, 10004, new Dictionary<string, object>
            {
                { "EffectValue", (float)changeAmount }
            });

            SeerGoLogger.Log($"[SideEffect_126] 每回合攻击+{changeAmount}, 速度+{changeAmount}");
        }
    }
}
