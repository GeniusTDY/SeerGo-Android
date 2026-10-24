using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=620: {0}回合内致命一击率上升{1}/16
    /// argsNum=2, args[0]=持续回合数, args[1]=暴击率分子(分母固定16)
    /// </summary>
    public class SideEffect_620 : ISkillSideEffectHandler
    {
        public override int EffectId => 620;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 2 || !int.TryParse(args[0], out int duration) || !int.TryParse(args[1], out int critRateNum))
            {
                SeerGoLogger.LogError($"[SideEffect_620] 参数解析失败");
                return;
            }

            float critRate = critRateNum / 16f;

            buffSystem.ApplyBuff(caster, caster, 20044, new Dictionary<string, object>
            {
                { "RemainingTurns", duration.ToString() },
                { "EffectValue", critRate }
            });

            SeerGoLogger.Log($"[SideEffect_620] 施加{duration}回合暴击率提升buff，暴击率={critRateNum}/16");
        }
    }
}
