using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=21: 作用{0}~{1}回合, 每回合反弹对手1/{2}伤害
    /// </summary>
    public class SideEffect_21 : ISkillSideEffectHandler
    {
        public override int EffectId => 21;

        public override void OnAfterCastSkill(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 3 || !int.TryParse(args[0], out int duration1) || !int.TryParse(args[1], out int duration2) || !float.TryParse(args[2], out float multiple))
            {
                SeerGoLogger.LogError($"[反弹效果] 参数解析失败");
                return;
            }

            int duration = Random.Range(duration1, duration2 + 1);
            SeerGoLogger.Log($"<color=cyan>[反弹效果] 持续时间:{duration}</color>");
            buffSystem.ApplyBuff(caster, target, 20012, new Dictionary<string, object>()
            {
                {"RemainingTurns", duration.ToString()},
                {"EffectValue", multiple}
            });
        
        }

    }
}
