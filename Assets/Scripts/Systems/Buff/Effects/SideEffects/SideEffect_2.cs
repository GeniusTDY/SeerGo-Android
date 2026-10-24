using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=2: 对方体力低于一半，威力加倍
    /// 注意：这个效果需要在伤害计算前调用（修改skill.Power）
    /// </summary>
    public class SideEffect_2 : ISkillSideEffectHandler
    {
        public override int EffectId => 2;

        public override void OnBeforeDamageCalculation(IBuffable caster, IBuffable target, SkillData skill, string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (!(caster is BattleEntityController casterController))
                return;
            if (!(target is BattleEntityController targetController))
                return;

            PetData targetData = targetController.GetCurrentPetData();
            if (targetData == null)
                return;

            int currentHp = targetData.CurrentStatus.Hp;
            int maxHp = targetController.GetMaxHp();
            // 对方HP小于一半时，威力翻倍
            if (currentHp < maxHp / 2f)
            {
                if (caster is PlayerBattleController)
                    RoundContext.Current.PlayerSkillPower *= 2;
                else
                    RoundContext.Current.EnemySkillPower *= 2;
            }
        }
    }
}