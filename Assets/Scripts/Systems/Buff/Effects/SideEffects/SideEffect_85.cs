using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=85: 使对手的能力提升效果转化到自己身上
    /// </summary>
    public class SideEffect_85 : ISkillSideEffectHandler
    {
        public override int EffectId => 85;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            List<int> tempBuffs = new List<int>();
            foreach (var buff in target.ActiveBuffs)
            {
                if (buff.Value.Config.tab == 9 && buff.Value.EffectValue > 0)
                {
                    tempBuffs.Add(buff.Key);
                }
            }
            // 能力提升效果转化到自己身上
            foreach (int buffId in tempBuffs)
            {
                buffSystem.ApplyBuff(caster, target, buffId, new Dictionary<string, object>
                    {
                        { "TargetType", BuffTargetType.Self },
                        { "EffectValue", caster.ActiveBuffs[buffId].EffectValue }
                    });
            }
            // 移除对方能力提升效果
            foreach (int buffId in tempBuffs)
            {
                target.RemoveBuff(buffId);
            }

        }
    }
}
