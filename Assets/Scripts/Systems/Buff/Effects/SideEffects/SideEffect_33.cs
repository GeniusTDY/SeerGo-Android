using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=33: 消除对手能力提升状态
    /// </summary>
    public class SideEffect_33 : ISkillSideEffectHandler
    {
        public override int EffectId => 33;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            var buffsToRemove = new List<int>();

            foreach (var buff in target.ActiveBuffs.Values)
            {
                // tab=9 表示能力效果
                if (buff.Config.tab == 9 && buff.EffectValue > 0)
                {
                    buffsToRemove.Add(buff.Config.id);
                }
            }

            foreach (int buffId in buffsToRemove)
            {
                target.RemoveBuff(buffId);
            }

            SeerGoLogger.Log($"[解除敌方增益buff] {caster} 解除了 {buffsToRemove.Count} 个增益状态");

        }
    }
}
