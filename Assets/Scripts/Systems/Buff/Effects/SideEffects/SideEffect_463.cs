using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=463: {0}回合内每回合所受的伤害减少{1}点
    /// argsNum=2, args[0]=持续回合数, args[1]=每回合减少伤害值
    /// 给自身施加一个buff，每回合结束时减少本回合受到的伤害
    /// </summary>
    public class SideEffect_463 : ISkillSideEffectHandler
    {
        public override int EffectId => 463;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 2 || !int.TryParse(args[0], out int duration) || !int.TryParse(args[1], out int reduceAmount))
            {
                SeerGoLogger.LogError($"[SideEffect_463] 参数解析失败");
                return;
            }

            buffSystem.ApplyBuff(caster, caster, 20039, new Dictionary<string, object>
            {
                { "RemainingTurns", duration.ToString() },
                { "EffectValue", (float)reduceAmount }
            });

            SeerGoLogger.Log($"[SideEffect_463] 施加{duration}回合伤害减免buff，每回合减免{reduceAmount}点");
        }
    }
}
