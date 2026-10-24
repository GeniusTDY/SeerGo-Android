using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=107: 若本次攻击造成的伤害小于{0}，则自身{1}提升1个等级
    /// args[0]=伤害阈值, args[1]=要提升的属性类型(atk/def/spAtk/spDef/spd/accuracy/critRate)
    /// </summary>
    public class SideEffect_107 : ISkillSideEffectHandler
    {
        public override int EffectId => 107;

        public override void OnAfterApplyDamage(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 2 || !int.TryParse(args[0], out int threshold))
            {
                SeerGoLogger.LogError($"[SideEffect_107] 参数解析失败: {string.Join(",", args)}");
                return;
            }

            int actualDamage = caster is PlayerBattleController
                ? RoundContext.Current.PlayerDamage
                : RoundContext.Current.EnemyDamage;

            if (actualDamage >= threshold)
            {
                SeerGoLogger.Log($"[SideEffect_107] 伤害({actualDamage}) >= 阈值({threshold})，不触发");
                return;
            }

            // 解析属性类型并应用提升buff
            int statBuffId = GetStatBuffId(args[1]);
            if (statBuffId == -1)
            {
                SeerGoLogger.LogError($"[SideEffect_107] 未知属性类型: {args[1]}");
                return;
            }

            buffSystem.ApplyBuff(caster, target, statBuffId, new Dictionary<string, object>
            {
                { "EffectValue", 1f }
            });

            SeerGoLogger.Log($"[SideEffect_107] 伤害低于阈值，{args[1]}提升1级");
        }

        private int GetStatBuffId(string statType)
        {
            return statType switch
            {
                "atk" => 10000,
                "def" => 10001,
                "spAtk" => 10002,
                "spDef" => 10003,
                "spd" => 10004,
                "accuracy" => 10005,
                "critRate" => 10006,
                _ => -1
            };
        }
    }
}
