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
    /// 副作用ID=92: {0}回合内，受到物理攻击时，有{1}%几率将对手冻伤
    /// </summary>
    public class SideEffect_92 : ISkillSideEffectHandler
    {
        public override int EffectId => 92;

        public override void OnAfterBuffTurnEnd(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 2 || !int.TryParse(args[0], out int duration) || !int.TryParse(args[1], out int probability))
            {
                SeerGoLogger.LogError($"[SideEffect_92] 参数解析失败: {string.Join(",", args)}");
                return;
            }

            // 概率判定
            bool success = (probability >= 100) || (Random.Range(1, 101) <= probability);

            if (!success)
            {
                SeerGoLogger.Log($"[SideEffect_92] 概率判定失败 ({probability}%)");
                return;
            }

            // 检查攻击者是否使用了物理攻击技能(Category==1)，若是则给攻击者挂冻伤
            if (caster is PlayerBattleController)
            {
                if (RoundContext.Current.EnemySkill.Category == 1)
                {
                    buffSystem.ApplyBuff(caster, target, 2, new Dictionary<string, object>()
                    {
                        { "duration", duration.ToString() }
                    });
                    SeerGoLogger.Log($"[SideEffect_92] 受到物理攻击，触发冻伤反馈(持续{duration}回合)");
                }
            }
            else if (caster is MonsterBattleController)
            {
                if (RoundContext.Current.PlayerSkill.Category == 1)
                {
                    buffSystem.ApplyBuff(caster, target, 2, new Dictionary<string, object>()
                    {
                        { "duration", duration.ToString() }
                    });
                    SeerGoLogger.Log($"[SideEffect_92] 受到物理攻击，触发冻伤反馈(持续{duration}回合)");
                }
            }
        }
    }
}
