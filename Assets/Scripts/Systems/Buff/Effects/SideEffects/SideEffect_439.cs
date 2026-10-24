using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=439: {0}回合内若自身处于能力下降或异常状态，则对手每回合受到{1}点固定伤害
    /// argsNum=2, args[0]=持续回合数, args[1]=固定伤害值
    /// </summary>
    public class SideEffect_439 : ISkillSideEffectHandler
    {
        public override int EffectId => 439;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 2 || !int.TryParse(args[0], out int duration) || !int.TryParse(args[1], out int fixedDamage))
            {
                SeerGoLogger.LogError($"[SideEffect_439] 参数解析失败");
                return;
            }

            buffSystem.ApplyBuff(caster, caster, 20040, new Dictionary<string, object>
            {
                { "RemainingTurns", duration.ToString() },
                { "EffectValue", (float)fixedDamage }
            });

            SeerGoLogger.Log($"[SideEffect_439] 施加{duration}回合条件固伤buff，每回合{fixedDamage}点");
        }
    }
}
