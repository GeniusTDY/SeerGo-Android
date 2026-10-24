using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=64: 自身在烧伤、冻伤、中毒状态下造成的伤害反而加倍
    /// </summary>
    public class SideEffect_64 : ISkillSideEffectHandler
    {
        public override int EffectId => 64;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if(caster.ActiveBuffs.ContainsKey(1) || caster.ActiveBuffs.ContainsKey(2)|| caster.ActiveBuffs.ContainsKey(3))
            {
                if(caster is PlayerBattleController)
                {
                    RoundContext.Current.PlayerDamage *= 2;
                }
                else if(caster is MonsterBattleController)
                {
                    RoundContext.Current.EnemyDamage *= 2;
                }
            }
        
        }
    }
}
