using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=62: 延时反杀 - {0}回合后若对方没有将自己击败，则对方死亡
    /// </summary>
    public class SideEffect_62 : ISkillSideEffectHandler
    {
        public override int EffectId => 62;

        public override void OnAfterCastSkill(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (ImmuneSkillBuffEffect.IsImmuneToLifeBondSkill(target))
                return;
            if (args.Length < 1 || !int.TryParse(args[0], out int countdown))
            {
                SeerGoLogger.LogError($"[延时反杀] 参数解析失败");
                return;
            }

            // 给目标应用同归于尽Buff
            int buffId = 20010; // 同归于尽Buff ID
            
            buffSystem.ApplyBuff(caster, target, buffId, new Dictionary<string, object>
            {
                { "TargetType", BuffTargetType.Enemy },
                { "RemainingTurns", countdown.ToString() },
            });

            SeerGoLogger.Log($"<color=yellow>[延时反杀] 目标将在{countdown}回合后死亡</color>");
        }
    }
}
