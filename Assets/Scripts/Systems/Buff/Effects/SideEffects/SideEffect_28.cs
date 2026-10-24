using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=28:降低对方1/{0}的hp
    /// </summary>
    public class SideEffect_28 : ISkillSideEffectHandler
    {
        public override int EffectId => 28;

        public override void OnAfterApplyDamage(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int multiple))
            {
                SeerGoLogger.LogError($"[降低1/n hp效果] 参数解析失败");
                return;
            }
            if (ImmuneSkillBuffEffect.IsImmuneToSkill(target))
                return;

            if (target is BattleEntityController battleEntity)
            {
                int damage = Mathf.RoundToInt(battleEntity.GetCurrentPetData().CurrentStatus.Hp * (1f / multiple));
                battleEntity.TakeDamage(damage);
            }
        }
    }
}
