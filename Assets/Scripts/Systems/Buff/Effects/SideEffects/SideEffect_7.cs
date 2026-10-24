using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=7: 愤怒 - 对方体力高于自己时才能命中，将对方体力减到和自己相同
    /// 特殊效果：直接设置固定伤害值
    /// </summary>
    public class SideEffect_7 : ISkillSideEffectHandler
    {
        public override int EffectId => 7;

        public override void OnAfterCastSkill(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {

            if (ImmuneSkillBuffEffect.IsImmuneToLifeBondSkill(target))
                return;
            if (ShadowBarrierBuffEffect.IsImmuneToDamage(target))
                return;

            if (!(caster is BattleEntityController casterController) ||
                !(target is BattleEntityController targetController))
                return;

            PetData casterData = casterController.GetCurrentPetData();
            PetData targetData = targetController.GetCurrentPetData();

            if (casterData == null || targetData == null)
                return;

            int casterHp = casterData.CurrentStatus.Hp;
            int targetHp = targetData.CurrentStatus.Hp;

            // 只有对方血量高于自己才能生效
            if (targetHp <= casterHp)
            {
                SeerGoLogger.Log($"[愤怒效果] 目标HP({targetHp}) <= 施法者HP({casterHp})，效果不触发");
                return;
            }

            // 计算伤害：将对方HP减到和自己相同
            int damage = targetHp - casterHp;

            if (caster is PlayerBattleController)
            {
                RoundContext.Current.PlayerDamage = damage;
                RoundContext.Current.IsPlayerSkillCrit = false; // 愤怒效果清除暴击标记
            }
            else if (caster is MonsterBattleController)
            {
                RoundContext.Current.EnemyDamage = damage;
                RoundContext.Current.IsEnemySkillCrit = false; // 愤怒效果清除暴击标记
            }

            SeerGoLogger.Log($"<color=yellow>[愤怒效果] 造成伤害: {damage}，将目标HP从{targetHp}减到{casterHp}</color>");
        }
    }
}
