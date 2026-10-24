using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=52: 先制失效 - {0}回合若本方先手攻击，使得对方的技能失效
    /// </summary>
    public class SideEffect_52 : ISkillSideEffectHandler
    {
        public override int EffectId => 52;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int duration))
            {
                SeerGoLogger.LogError($"[先制失效] 参数解析失败");
                return;
            }

            // 应用先制失效Buff
            int buffId = 20005; // 先制失效Buff ID
            
            buffSystem.ApplyBuff(caster, target, buffId, new Dictionary<string, object>
            {
                { "RemainingTurns", duration.ToString() }
            });

            SeerGoLogger.Log($"<color=yellow>[先制失效] 获得{duration}回合先制压制效果</color>");
        }
    }
}
