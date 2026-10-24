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
    /// 副作用ID=84:{0}回合内受到物理攻击时有{1}%几率将对手麻痹
    /// </summary>
    public class SideEffect_84 : ISkillSideEffectHandler
    {
        public override int EffectId => 84;

        public override void OnAfterBuffTurnEnd(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 2 || !int.TryParse(args[0], out int duration) || !int.TryParse(args[1], out int probability))
            {
                SeerGoLogger.LogError($"[SideEffect_84] 参数解析失败");
                return;
            }

            // 概率判定
            bool success = (probability >= 100) || (Random.Range(1, 101) <= probability);

            if (!success)
            {
                SeerGoLogger.Log($"[SideEffect_84] 概率判定失败 ({probability}%)");
                return;
            }

            if (caster is PlayerBattleController playerBattle)
            {
                // 物攻
                if (RoundContext.Current.EnemySkill.Category == 1)
                {
                    buffSystem.ApplyBuff(caster, target, 5, new Dictionary<string, object>()
                    {
                        {"duration",duration.ToString()}
                    });
                }
            }
            else if (caster is MonsterBattleController monsterBattle)
            {
                if (RoundContext.Current.PlayerSkill.Category == 1)
                {
                    buffSystem.ApplyBuff(caster, target, 5, new Dictionary<string, object>()
                    {
                        {"duration",duration.ToString()}
                    });
                }
            }
        }
    }
}
