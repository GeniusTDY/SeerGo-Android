using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=121: 属性相同时，{0}%几率让对方麻痹
    /// </summary>
    public class SideEffect_121 : ISkillSideEffectHandler
    {
        public override int EffectId => 121;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int probability))
            {
                SeerGoLogger.LogError($"[SideEffect_121] 参数解析失败: {string.Join(",", args)}");
                return;
            }

            // 检查双方属性是否相同
            int casterType = caster is PlayerBattleController
                ? RoundContext.Current.PlayerType
                : RoundContext.Current.EnemyType;
            int targetType = target is PlayerBattleController
                ? RoundContext.Current.PlayerType
                : RoundContext.Current.EnemyType;

            if (casterType != targetType)
            {
                SeerGoLogger.Log($"[SideEffect_121] 属性不同，不触发");
                return;
            }

            bool success = (probability >= 100) || (Random.Range(1, 101) <= probability);
            if (!success)
            {
                SeerGoLogger.Log($"[SideEffect_121] 概率判定失败 ({probability}%)");
                return;
            }

            // 麻痹 (buff id=5)
            buffSystem.ApplyBuff(caster, target, 5);
            SeerGoLogger.Log($"[SideEffect_121] 属性相同，麻痹生效");
        }
    }
}
