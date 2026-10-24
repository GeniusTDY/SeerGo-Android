using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=45: 伤害变化效果
    /// ID=45: {0}回合自身的防御力和对手相同
    /// </summary>
    public class SideEffect_45 : ISkillSideEffectHandler
    {
        public override int EffectId => 45;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1)
            {
                SeerGoLogger.LogError($"[SideEffect_45] 参数不足,需要1个参数");
                return;
            }

            if (!int.TryParse(args[0], out int duration))
            {
                SeerGoLogger.LogError($"[SideEffect_45] 参数解析失败: {string.Join(",", args)}");
                return;
            }

            // 应用buff
            buffSystem.ApplyBuff(caster, target, 20017, new Dictionary<string, object>
            {
                { "RemainingTurns",  duration.ToString()}
            });
        }

    }
}
