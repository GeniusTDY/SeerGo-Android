using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=545: {0}回合内若受到伤害高于{1}则对手{2}
    /// argsNum=3, args[0]=持续回合数, args[1]=伤害阈值, args[2]=异常状态类型
    /// 状态类型: 麻痹0,中毒1,烧伤2,冻伤5,害怕6,疲惫7,睡眠8,石化9,冰封15
    /// </summary>
    public class SideEffect_545 : ISkillSideEffectHandler
    {
        public override int EffectId => 545;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 3 || !int.TryParse(args[0], out int duration) ||
                !int.TryParse(args[1], out int threshold) || !int.TryParse(args[2], out int statusType))
            {
                SeerGoLogger.LogError($"[SideEffect_545] 参数解析失败");
                return;
            }

            buffSystem.ApplyBuff(caster, caster, 20043, new Dictionary<string, object>
            {
                { "RemainingTurns", duration.ToString() },
                { "EffectValue", (float)threshold },
                { "Args", new string[] { statusType.ToString() } }
            });

            SeerGoLogger.Log($"[SideEffect_545] 施加{duration}回合反伤异常buff，阈值={threshold}，状态类型={statusType}");
        }
    }
}
