using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=128: {0}回合内将受到的攻击伤害转化为自身体力
    /// args[0]=duration
    /// </summary>
    public class SideEffect_128 : ISkillSideEffectHandler
    {
        public override int EffectId => 128;

        public override void OnAfterBuffTurnEnd(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int duration))
            {
                SeerGoLogger.LogError($"[SideEffect_128] 参数解析失败: {string.Join(",", args)}");
                return;
            }

            // 将受到的伤害转化为回血
            if (caster is PlayerBattleController player && RoundContext.Current.EnemyDamage > 0)
            {
                int heal = RoundContext.Current.EnemyDamage;
                player.TakeDamage(heal, "+");
                RoundContext.Current.EnemyDamage = 0;
                SeerGoLogger.Log($"[SideEffect_128] 伤害转回血: {heal}");
            }
            else if (caster is MonsterBattleController monster && RoundContext.Current.PlayerDamage > 0)
            {
                int heal = RoundContext.Current.PlayerDamage;
                monster.TakeDamage(heal, "+");
                RoundContext.Current.PlayerDamage = 0;
                SeerGoLogger.Log($"[SideEffect_128] 伤害转回血: {heal}");
            }
        }
    }
}
