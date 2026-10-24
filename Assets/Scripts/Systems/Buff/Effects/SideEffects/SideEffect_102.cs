using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=102: 对手处于麻痹状态时，威力翻倍
    /// </summary>
    public class SideEffect_102 : ISkillSideEffectHandler
    {
        public override int EffectId => 102;

        public override void OnBeforeDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            // 检查目标是否处于麻痹状态 (buff id=5)
            if (!target.ActiveBuffs.ContainsKey(5))
                return;

            if (caster is PlayerBattleController)
            {
                RoundContext.Current.PlayerSkillPower *= 2;
                SeerGoLogger.Log($"[SideEffect_102] 对手麻痹，威力翻倍 => {RoundContext.Current.PlayerSkillPower}");
            }
            else if (caster is MonsterBattleController)
            {
                RoundContext.Current.EnemySkillPower *= 2;
                SeerGoLogger.Log($"[SideEffect_102] 对手麻痹，威力翻倍 => {RoundContext.Current.EnemySkillPower}");
            }
        }
    }
}
