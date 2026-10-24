using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=101: 给对手造成伤害时，伤害数值的{0}%恢复自身体力
    /// </summary>
    public class SideEffect_101 : ISkillSideEffectHandler
    {
        public override int EffectId => 101;

        public override void OnAfterApplyDamage(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !float.TryParse(args[0], out float multiple))
            {
                SeerGoLogger.LogError($"[SideEffect_101] 参数解析失败");
                return;
            }

            if(caster is PlayerBattleController player)
            {
                if(RoundContext.Current.PlayerDamage <= 0)
                    return;

                player.TakeDamage((int)(RoundContext.Current.PlayerDamage * (multiple/100)), "+");
            }else if(caster is MonsterBattleController monster)
            {
                if(RoundContext.Current.EnemyDamage <= 0)
                    return;

                monster.TakeDamage((int)(RoundContext.Current.EnemyDamage * (multiple/100)), "+");
            }
        }
    }
}
