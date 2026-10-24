using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=132: 若当前HP低于对手，则技能威力翻倍
    /// </summary>
    public class SideEffect_132 : ISkillSideEffectHandler
    {
        public override int EffectId => 132;

        public override void OnBeforeDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (!(caster is BattleEntityController casterCtrl) || !(target is BattleEntityController targetCtrl))
                return;

            int casterHp = casterCtrl.GetCurrentPetData().CurrentStatus.Hp;
            int targetHp = targetCtrl.GetCurrentPetData().CurrentStatus.Hp;

            if (casterHp >= targetHp)
            {
                SeerGoLogger.Log($"[SideEffect_132] HP({casterHp}) >= 对手HP({targetHp})，不触发");
                return;
            }

            if (caster is PlayerBattleController)
            {
                RoundContext.Current.PlayerSkillPower *= 2;
                SeerGoLogger.Log($"[SideEffect_132] HP低于对手，威力翻倍 => {RoundContext.Current.PlayerSkillPower}");
            }
            else if (caster is MonsterBattleController)
            {
                RoundContext.Current.EnemySkillPower *= 2;
                SeerGoLogger.Log($"[SideEffect_132] HP低于对手，威力翻倍 => {RoundContext.Current.EnemySkillPower}");
            }
        }
    }
}
