using QFramework;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=3: 解除自身的能力下降状态
    /// </summary>
    public class SideEffect_3 : ISkillSideEffectHandler
    {
        public override int EffectId => 3;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            var buffsToRemove = new List<int>();

            foreach (var buff in caster.ActiveBuffs.Values)
            {
                // tab=9 表示能力负面效果
                if (buff.Config.tab == 9 && buff.EffectValue < 0)
                {
                    buffsToRemove.Add(buff.Config.id);
                }
            }

            foreach (int buffId in buffsToRemove)
            {
                caster.RemoveBuff(buffId);
            }

            SeerGoLogger.Log($"[解除Debuff] {caster} 解除了 {buffsToRemove.Count} 个负面状态");
        }

    }
}
