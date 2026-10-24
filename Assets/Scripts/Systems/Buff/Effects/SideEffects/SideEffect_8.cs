using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=8: 保留 - 伤害大于对方体力时，对方会余下1体力
    /// </summary>
    public class SideEffect_8 : ISkillSideEffectHandler
    {
        public override int EffectId => 8;

        public override void OnAfterCastSkill(IBuffable caster, IBuffable target, SkillData skill,
                                        string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (!TryGetTargetHp(target, out int targetHp)) return;
            if (targetHp == 1)
            {
                SetDamage(caster, 0);
                return;
            }
            int currentDamage = GetDamage(caster);
            if (currentDamage >= targetHp)
            {
                int adjustedDamage = targetHp - 1;
                SetDamage(caster, adjustedDamage);
                LogDamageAdjustment(currentDamage, adjustedDamage);
            }
        }

        private bool TryGetTargetHp(IBuffable target, out int targetHp)
        {
            targetHp = 0;
            var targetData = (target as BattleEntityController)?.GetCurrentPetData();
            if (targetData == null) return false;

            targetHp = targetData.CurrentStatus.Hp;
            return true;
        }

        private int GetDamage(IBuffable caster)
        {
            return caster switch
            {
                PlayerBattleController _ => RoundContext.Current.PlayerDamage,
                MonsterBattleController _ => RoundContext.Current.EnemyDamage,
                _ => 0
            };
        }

        private void SetDamage(IBuffable caster, int damage)
        {
            if (caster is PlayerBattleController)
                RoundContext.Current.PlayerDamage = damage;
            else if (caster is MonsterBattleController)
                RoundContext.Current.EnemyDamage = damage;
        }

        private void LogDamageAdjustment(int original, int adjusted)
        {
            SeerGoLogger.Log($"<color=yellow>[保留效果] 伤害从{original}调整为{adjusted}，目标保留1HP</color>");
        }
    }
}
