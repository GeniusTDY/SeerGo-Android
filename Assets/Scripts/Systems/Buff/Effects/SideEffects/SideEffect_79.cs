using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=79: 损失1/2的体力，提升自身的能力
    /// 特攻 + 2、速度 + 1、命中 + 1 当前体力减半
    /// </summary>
    public class SideEffect_79 : ISkillSideEffectHandler
    {
        public override int EffectId => 79;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if(caster is PlayerBattleController player)
            {
                var petData = player.GetCurrentPetData();
                player.TakeDamage((int)Math.Max(petData.CurrentStatus.Hp / 2f, 1));

                buffSystem.ApplyBuff(caster, target, 10002, new Dictionary<string, object>
                {
                    { "EffectValue", 2f }
                });
                buffSystem.ApplyBuff(caster, target, 10004, new Dictionary<string, object>
                {
                    { "EffectValue", 1f }
                });
                buffSystem.ApplyBuff(caster, target, 10005, new Dictionary<string, object>
                {
                    { "EffectValue", 1f }
                });
            }else if (caster is MonsterBattleController monster)
            {
                var petData = monster.GetCurrentPetData();
                monster.TakeDamage((int)Math.Max(petData.CurrentStatus.Hp / 2f, 1));

                buffSystem.ApplyBuff(caster, target, 10002, new Dictionary<string, object>
                {
                    { "EffectValue", 2f }
                });
                buffSystem.ApplyBuff(caster, target, 10004, new Dictionary<string, object>
                {
                    { "EffectValue", 1f }
                });
                buffSystem.ApplyBuff(caster, target, 10005, new Dictionary<string, object>
                {
                    { "EffectValue", 1f }
                });
            }


        }
    }
}
