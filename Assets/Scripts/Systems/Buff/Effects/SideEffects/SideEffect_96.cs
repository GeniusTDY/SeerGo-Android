using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=96: 对手处于烧伤状态时，威力翻倍
    /// </summary>
    public class SideEffect_96 : ISkillSideEffectHandler
    {
        public override int EffectId => 96;

        public override void OnBeforeDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            // 检查目标是否处于烧伤状态 (buff id=3)
            if (target is BattleEntityController targetController)
            {
                if (targetController.ActiveBuffs.ContainsKey(3))
                {
                    if (caster is PlayerBattleController)
                    {
                        RoundContext.Current.PlayerSkillPower *= 2;
                        SeerGoLogger.Log($"[SideEffect_96] 对手烧伤，威力翻倍 => {RoundContext.Current.PlayerSkillPower}");
                    }
                    else if (caster is MonsterBattleController)
                    {
                        RoundContext.Current.EnemySkillPower *= 2;
                        SeerGoLogger.Log($"[SideEffect_96] 对手烧伤，威力翻倍 => {RoundContext.Current.EnemySkillPower}");
                    }
                }
            }
        }
    }
}
