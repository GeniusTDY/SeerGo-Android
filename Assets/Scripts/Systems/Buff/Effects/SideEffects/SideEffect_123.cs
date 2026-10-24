using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=123: {0}回合内，受到任何伤害，自身{1}提高{2}个等级
    /// args[0]=duration, args[1]=statType, args[2]=changeAmount
    /// </summary>
    public class SideEffect_123 : ISkillSideEffectHandler
    {
        public override int EffectId => 123;

        public override void OnAfterBuffTurnEnd(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 3 || !int.TryParse(args[0], out int duration) || !int.TryParse(args[2], out int changeAmount))
            {
                SeerGoLogger.LogError($"[SideEffect_123] 参数解析失败: {string.Join(",", args)}");
                return;
            }

            // 检查本方是否受到了伤害
            bool tookDamage = caster is PlayerBattleController
                ? RoundContext.Current.EnemyDamage > 0
                : RoundContext.Current.PlayerDamage > 0;

            if (!tookDamage)
            {
                SeerGoLogger.Log($"[SideEffect_123] 本回合未受到伤害，不触发");
                return;
            }

            int statBuffId = GetStatBuffId(args[1]);
            if (statBuffId == -1)
            {
                SeerGoLogger.LogError($"[SideEffect_123] 未知属性类型: {args[1]}");
                return;
            }

            buffSystem.ApplyBuff(caster, target, statBuffId, new Dictionary<string, object>
            {
                { "EffectValue", (float)changeAmount }
            });

            SeerGoLogger.Log($"[SideEffect_123] 受到伤害，{args[1]}提升{changeAmount}级");
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
