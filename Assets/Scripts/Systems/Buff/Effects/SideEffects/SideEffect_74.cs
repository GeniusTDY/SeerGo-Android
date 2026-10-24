using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=74: 命中时有30%几率令对手中毒、烧伤或冻伤
    /// </summary>
    public class SideEffect_74 : ISkillSideEffectHandler
    {
        public override int EffectId => 74;

        public override void OnAfterCastSkill(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            int probability = 30;
            // 概率判定
            bool success = (probability >= 100) || (Random.Range(1, 101) <= probability);

            if (!success)
            {
                SeerGoLogger.Log($"[SideEffect_74] 概率判定失败");
                return;
            }

            int buffId = Random.Range(1, 4);
            SeerGoLogger.Log($"<color=yellow>[SideEffect_74] 概率判定成功，应用Buff{buffId}</color>");
            buffSystem.ApplyBuff(caster, target, buffId);
        }
    }
}
