using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=55: 属性反转 - {0}回合内，自身与对方反转战斗时属性
    /// </summary>
    public class SideEffect_55 : ISkillSideEffectHandler
    {
        public override int EffectId => 55;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int duration))
            {
                SeerGoLogger.LogError($"[属性反转] 参数解析失败");
                return;
            }

            int buffId = 20006; // 属性反转Buff ID
            
            // 给目标添加
            buffSystem.ApplyBuff(caster, target, buffId, new Dictionary<string, object>
            {
                { "RemainingTurns", duration.ToString() }
            });

            // 和属性同化互斥
            target.ActiveBuffs.Remove(20007);

            SeerGoLogger.Log($"<color=yellow>[属性反转] 双方属性反转{duration}回合</color>");
        }
    }
}
