using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=63: 将能力下降状态反馈给对手
    /// </summary>
    public class SideEffect_63 : ISkillSideEffectHandler
    {
        public override int EffectId => 63;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            List<int> tempBuffs = new List<int>();
            foreach (var buff in caster.ActiveBuffs)
            {
                if (buff.Value.Config.tab == 9 && buff.Value.EffectValue < 0)
                {
                    tempBuffs.Add(buff.Key);
                }
            }
            // 反馈
            foreach (int buffId in tempBuffs)
            {
                buffSystem.ApplyBuff(caster, target, buffId, new Dictionary<string, object>
                    {
                        { "TargetType", BuffTargetType.Enemy },
                        { "EffectValue", caster.ActiveBuffs[buffId].EffectValue }
                    });
            }
            // 移除
            foreach (int buffId in tempBuffs)
            {
                caster.RemoveBuff(buffId);
            }
            

        }
    }
}
