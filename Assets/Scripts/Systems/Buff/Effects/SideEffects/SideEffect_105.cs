using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=105: 给予对象损伤的1/{0}，会回复自己的HP
    /// </summary>
    public class SideEffect_105 : ISkillSideEffectHandler
    {
        public override int EffectId => 105;

        public override void OnAfterApplyDamage(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !float.TryParse(args[0], out float multiple))
            {
                SeerGoLogger.LogError($"[SideEffect_105] 参数解析失败: {string.Join(",", args)}");
                return;
            }

            if (caster is PlayerBattleController player)
            {
                if (RoundContext.Current.PlayerDamage <= 0)
                    return;

                int heal = (int)(RoundContext.Current.PlayerDamage / multiple);
                player.TakeDamage(heal, "+");
                SeerGoLogger.Log($"[SideEffect_105] 恢复体力: {heal}");
            }
            else if (caster is MonsterBattleController monster)
            {
                if (RoundContext.Current.EnemyDamage <= 0)
                    return;

                int heal = (int)(RoundContext.Current.EnemyDamage / multiple);
                monster.TakeDamage(heal, "+");
                SeerGoLogger.Log($"[SideEffect_105] 恢复体力: {heal}");
            }
        }
    }
}
