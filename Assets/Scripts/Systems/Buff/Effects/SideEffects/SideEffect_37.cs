using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=37: 自身HP小于1/{0}时威力为{1}倍
    /// </summary>
    public class SideEffect_37 : ISkillSideEffectHandler
    {
        public override int EffectId => 37;

        public override void OnBeforeDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {

            if (args.Length < 2 || !int.TryParse(args[0], out int hpMultiple) || !int.TryParse(args[1], out int powerMultiple))
            {
                SeerGoLogger.LogError($"[SideEffect_37] 参数解析失败");
                return;
            }

            if (caster is PlayerBattleController player)
            {
                if (player.GetCurrentPetData().CurrentStatus.Hp < player.GetMaxHp() * 1f / hpMultiple)
                    RoundContext.Current.PlayerSkillPower *= powerMultiple;
            }
            else if (caster is MonsterBattleController monster)
            {
                if (monster.GetCurrentPetData().CurrentStatus.Hp < monster.GetMaxHp() * 1f / hpMultiple)
                    RoundContext.Current.EnemySkillPower *= powerMultiple;
            }
        }
    }
}
