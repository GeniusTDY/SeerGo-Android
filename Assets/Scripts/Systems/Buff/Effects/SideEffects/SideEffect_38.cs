using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=38: 降低对方{0}点战斗时的最高HP
    /// </summary>
    public class SideEffect_38 : ISkillSideEffectHandler
    {
        public override int EffectId => 38;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {

            if (args.Length < 1 || !float.TryParse(args[0], out float hpMultiple))
            {
                SeerGoLogger.LogError($"[SideEffect_38] 参数解析失败");
                return;
            }

            buffSystem.ApplyBuff(caster, target, 10008, new Dictionary<string, object>()
            {
                {"EffectValue", hpMultiple}
            });

        }
    }
}
