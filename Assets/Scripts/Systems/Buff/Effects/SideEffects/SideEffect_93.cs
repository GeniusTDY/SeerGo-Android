using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=93: {0}%几率额外附加{1}点固定伤害
    /// </summary>
    public class SideEffect_93 : ISkillSideEffectHandler
    {
        public override int EffectId => 93;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 2 || !int.TryParse(args[0], out int probability) || !int.TryParse(args[1], out int fixedDamage))
            {
                SeerGoLogger.LogError($"[SideEffect_93] 参数解析失败: {string.Join(",", args)}");
                return;
            }

            // 概率判定
            bool success = (probability >= 100) || (Random.Range(1, 101) <= probability);

            if (!success)
            {
                SeerGoLogger.Log($"[SideEffect_93] 概率判定失败 ({probability}%)");
                return;
            }

            // 额外附加固定伤害
            if (caster is PlayerBattleController)
            {
                RoundContext.Current.PlayerDamage += fixedDamage;
                SeerGoLogger.Log($"[SideEffect_93] 触发额外固定伤害 +{fixedDamage}");
            }
            else if (caster is MonsterBattleController)
            {
                RoundContext.Current.EnemyDamage += fixedDamage;
                SeerGoLogger.Log($"[SideEffect_93] 触发额外固定伤害 +{fixedDamage}");
            }
        }
    }
}
