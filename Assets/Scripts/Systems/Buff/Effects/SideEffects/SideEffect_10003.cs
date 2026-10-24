using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using System;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=10003: 技能威力=【精灵个体值* 5*精灵等级÷100】
    /// 布林克克“剧蚀之触”
    /// 公式: 技能威力=【精灵个体值* 5*精灵等级÷100】
    /// </summary>
    public class SideEffect_10003 : ISkillSideEffectHandler
    {
        public override int EffectId => 10003;

        public override void OnBeforeDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (caster is BattleEntityController casterController)
            {
                var petData = casterController.GetCurrentPetData();
                int iv = petData.Talent.IV;
                int lv = petData.Exp.Level;

                if (caster is PlayerBattleController)
                {
                    
                    RoundContext.Current.PlayerSkillPower = (int)Math.Floor(iv * 5 * lv / 100f);
                }
                else if (caster is MonsterBattleController)
                {
                    RoundContext.Current.EnemySkillPower = (int)Math.Floor(iv * 5 * lv / 100f);
                }
            }
        }
    }
}
