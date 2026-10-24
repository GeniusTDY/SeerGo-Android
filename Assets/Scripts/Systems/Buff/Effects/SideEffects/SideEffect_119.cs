using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=119: 若伤害为奇数，30%对手疲惫1回合；若为偶数，30%速度+1
    /// </summary>
    public class SideEffect_119 : ISkillSideEffectHandler
    {
        public override int EffectId => 119;

        public override void OnAfterApplyDamage(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            int damage = caster is PlayerBattleController
                ? RoundContext.Current.PlayerDamage
                : RoundContext.Current.EnemyDamage;

            if (damage % 2 == 1)
            {
                // 奇数伤害：30%对手疲惫1回合
                if (Random.Range(1, 101) <= 30)
                {
                    buffSystem.ApplyBuff(caster, target, 6, new Dictionary<string, object>
                    {
                        { "duration", "1" }
                    });
                    SeerGoLogger.Log($"[SideEffect_119] 伤害为奇数，对手疲惫1回合");
                }
            }
            else
            {
                // 偶数伤害：30%自身速度+1
                if (Random.Range(1, 101) <= 30)
                {
                    buffSystem.ApplyBuff(caster, target, 10004, new Dictionary<string, object>
                    {
                        { "EffectValue", 1f }
                    });
                    SeerGoLogger.Log($"[SideEffect_119] 伤害为偶数，自身速度+1");
                }
            }
        }
    }
}
