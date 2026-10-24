using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=97: 对手处于冻伤状态时，威力翻倍
    /// </summary>
    public class SideEffect_97 : ISkillSideEffectHandler
    {
        public override int EffectId => 97;

        public override void OnBeforeDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            // 检查目标是否处于冻伤状态 (buff id=2)
            if (target is BattleEntityController targetController)
            {
                if (targetController.ActiveBuffs.ContainsKey(2))
                {
                    if (caster is PlayerBattleController)
                    {
                        RoundContext.Current.PlayerSkillPower *= 2;
                        SeerGoLogger.Log($"[SideEffect_97] 对手冻伤，威力翻倍 => {RoundContext.Current.PlayerSkillPower}");
                    }
                    else if (caster is MonsterBattleController)
                    {
                        RoundContext.Current.EnemySkillPower *= 2;
                        SeerGoLogger.Log($"[SideEffect_97] 对手冻伤，威力翻倍 => {RoundContext.Current.EnemySkillPower}");
                    }
                }
            }
        }
    }
}
