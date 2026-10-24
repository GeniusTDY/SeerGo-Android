using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=508: 减少{0}点下回合所受的伤害
    /// argsNum=1, args[0]=减少伤害值
    /// </summary>
    public class SideEffect_508 : ISkillSideEffectHandler
    {
        public override int EffectId => 508;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int reduceAmount))
            {
                SeerGoLogger.LogError($"[SideEffect_508] 参数解析失败");
                return;
            }

            buffSystem.ApplyBuff(caster, caster, 20042, new Dictionary<string, object>
            {
                { "RemainingTurns", "1" },
                { "EffectValue", (float)reduceAmount }
            });

            SeerGoLogger.Log($"[SideEffect_508] 施加下回合伤害减免{reduceAmount}点");
        }
    }
}
