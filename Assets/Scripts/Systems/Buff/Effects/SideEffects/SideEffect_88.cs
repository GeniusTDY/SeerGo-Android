using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=88: {0}%几率伤害为{1}倍
    /// </summary>
    public class SideEffect_88 : ISkillSideEffectHandler
    {
        public override int EffectId => 88;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 2 || !int.TryParse(args[0], out int probability) || !int.TryParse(args[1], out int multiple))
            {
                SeerGoLogger.LogError($"[SideEffect_88] 参数解析失败");
                return;
            }

            bool success = (probability >= 100) || (Random.Range(1, 101) <= probability);

            if (!success)
            {
                SeerGoLogger.LogError($"[SideEffect_88] 判定失败");
                return;
            }

            if (caster is PlayerBattleController)
            {
                RoundContext.Current.PlayerDamage *= multiple;
            }
            else if (caster is MonsterBattleController)
            {
                RoundContext.Current.EnemyDamage *= multiple;
            }
        }
    }
}
