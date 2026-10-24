using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=78: {0}回合内，物理攻击对自身必定miss
    /// </summary>
    public class SideEffect_78 : ISkillSideEffectHandler
    {
        public override int EffectId => 78;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int duration))
            {
                SeerGoLogger.LogError($"[SideEffect_78] 参数解析失败");
                return;
            }
           
            int buffId = 20027;

            buffSystem.ApplyBuff(caster, target, buffId, new Dictionary<string, object>
            {
                { "RemainingTurns", duration.ToString() }
            });


        }
    }
}
