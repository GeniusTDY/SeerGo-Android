using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=68:{0}回合内，受到致死攻击时则余下1点体力
    /// </summary>
    public class SideEffect_68 : ISkillSideEffectHandler
    {
        public override int EffectId => 68;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int duration))
            {
                SeerGoLogger.LogError($"[SideEffect_68] 参数解析失败");
                return;
            }

            buffSystem.ApplyBuff(caster, target, 20023, new Dictionary<string, object>
            {
                { "RemainingTurns", duration.ToString() }
            });
        }
    }
}
