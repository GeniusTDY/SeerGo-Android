using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=58: 必定暴击 - 下{0}回合自身攻击技能必定打出致命一击
    /// </summary>
    public class SideEffect_58 : ISkillSideEffectHandler
    {
        public override int EffectId => 58;

        public override void OnAfterBuffTurnEnd(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int duration))
            {
                SeerGoLogger.LogError($"[必定暴击] 参数解析失败");
                return;
            }

            // 应用必定暴击Buff
            int buffId = 20008; // 必定暴击Buff ID
            
            buffSystem.ApplyBuff(caster, target, buffId, new Dictionary<string, object>
            {
                { "RemainingTurns", duration.ToString() }
            });

            SeerGoLogger.Log($"<color=yellow>[必定暴击] 获得{duration}回合必定暴击状态</color>");
        }
    }
}
