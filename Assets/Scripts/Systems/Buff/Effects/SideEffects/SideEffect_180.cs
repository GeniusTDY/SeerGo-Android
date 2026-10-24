using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=102: 消除对手回合类效果
    /// </summary>
    public class SideEffect_180 : ISkillSideEffectHandler
    {
        public override int EffectId => 180;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if(!(target is BattleEntityController controller))
                return;

            List<int> tempBuffs = new List<int>();
            foreach (var buff in target.ActiveBuffs)
            {
                if(buff.Value.RemainingTurns > 0)
                    tempBuffs.Add(buff.Key);
            }

            tempBuffs.ForEach(buffId =>
            {
                controller.RemoveBuff(buffId);
            });
        }
    }
}
