using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=75: 命中时有30%几率令对手麻痹、睡眠或害怕
    /// </summary>
    public class SideEffect_75 : ISkillSideEffectHandler
    {
        public override int EffectId => 75;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            int probability = 30;
            // 概率判定
            bool success = (probability >= 100) || (Random.Range(1, 101) <= probability);

            if (!success)
            {
                SeerGoLogger.Log($"[SideEffect_75] 概率判定失败");
                return;
            }
            int buffId = new int[] { 5, 7, 8 }[Random.Range(0, 3)];

            SeerGoLogger.Log($"<color=yellow>[SideEffect_75] 概率判定成功，应用Buff{buffId}</color>");
            buffSystem.ApplyBuff(caster, target, buffId);
        }
    }
}
