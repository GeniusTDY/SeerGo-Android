using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=125: {0}回合内，每回合被攻击所受到的伤害不超过{1}
    /// args[0]=duration, args[1]=maxDamagePerHit
    /// </summary>
    public class SideEffect_125 : ISkillSideEffectHandler
    {
        public override int EffectId => 125;

        public override void OnAfterBuffTurnEnd(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 2 || !int.TryParse(args[0], out int duration) || !int.TryParse(args[1], out int maxDamage))
            {
                SeerGoLogger.LogError($"[SideEffect_125] 参数解析失败: {string.Join(",", args)}");
                return;
            }

            // 限制本回合受到的伤害
            if (caster is PlayerBattleController && RoundContext.Current.EnemyDamage > maxDamage)
            {
                SeerGoLogger.Log($"[SideEffect_125] 伤害({RoundContext.Current.EnemyDamage})超过上限({maxDamage})，限制");
                RoundContext.Current.EnemyDamage = maxDamage;
            }
            else if (caster is MonsterBattleController && RoundContext.Current.PlayerDamage > maxDamage)
            {
                SeerGoLogger.Log($"[SideEffect_125] 伤害({RoundContext.Current.PlayerDamage})超过上限({maxDamage})，限制");
                RoundContext.Current.PlayerDamage = maxDamage;
            }
        }
    }
}
