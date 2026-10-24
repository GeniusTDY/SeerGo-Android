using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=98: {0}回合内，对雄性精灵的伤害为{1}倍
    /// </summary>
    public class SideEffect_98 : ISkillSideEffectHandler
    {
        public override int EffectId => 98;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 2 || !int.TryParse(args[0], out int duration) || !float.TryParse(args[1], out float multiplier))
            {
                SeerGoLogger.LogError($"[SideEffect_98] 参数解析失败: {string.Join(",", args)}");
                return;
            }

            // 给施法者自身添加性别伤害倍率buff，持续 duration 回合
            buffSystem.ApplyBuff(caster, target, 20032, new Dictionary<string, object>()
            {
                { "RemainingTurns", duration.ToString() },
                { "EffectValue", multiplier }
            });

            SeerGoLogger.Log($"[SideEffect_98] 对雄性伤害{multiplier}倍效果生效，持续{duration}回合");
        }
    }
}
