using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=32: 使用后{0}回合攻击击中对象要害概率增加1/16
    /// </summary>
    public class SideEffect_32 : ISkillSideEffectHandler
    {
        public override int EffectId => 32;

        public override void OnAfterBuffTurnEnd(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {

            if (args.Length < 1 || !int.TryParse(args[0], out int duration))
            {
                SeerGoLogger.LogError($"[使用后n回合攻击击中对象要害概率增加1/16] 参数解析失败");
                return;
            }

            buffSystem.ApplyBuff(caster, target, 10007, new Dictionary<string, object>
            {
                { "RemainingTurns", duration.ToString() }
            });

        }
    }
}
