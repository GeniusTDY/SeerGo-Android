using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 特性1018：回避 - 被技能命中的几率减少5%
    /// Args: "5 0" - 第一个参数是回避百分比
    /// </summary>
    public class NewSeEffect_1018 : INewSeEffectHandler
    {
        public override int SeId => 1018;

        public override void OnBeforeDamageCalculation(NewSeConfig seConfig, IBuffable caster, IBuffable target,
            SkillData skill, string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            // Args格式: "回避百分比 标志"
            // 回避效果：在伤害计算前降低对方命中率
            if (args == null || args.Length < 1) return;

            if (!int.TryParse(args[0], out int evasionRate)) return;

            var roundContext = Controllers.Battle.RoundContext.Current;

            // caster是被攻击方（触发回避特性的那个）
            // target是攻击方
            if (caster is PlayerBattleController)
            {
                // 玩家被攻击，增加回避率（降低对方命中率）
                if (roundContext.EnemySkill == skill)
                {
                    int currentAccuracy = roundContext.EnemySkillAccuracy;
                    roundContext.EnemySkillAccuracy = System.Math.Max(0, currentAccuracy - evasionRate);
                    SeerGoLogger.Log($"<color=green>[特性-回避] 敌方技能命中率降低 {evasionRate}% ({currentAccuracy} -> {roundContext.EnemySkillAccuracy})</color>");
                }
            }
            else if (caster is MonsterBattleController)
            {
                // 敌方被攻击，增加回避率（降低对方命中率）
                if (roundContext.PlayerSkill == skill)
                {
                    int currentAccuracy = roundContext.PlayerSkillAccuracy;
                    roundContext.PlayerSkillAccuracy = System.Math.Max(0, currentAccuracy - evasionRate);
                    SeerGoLogger.Log($"<color=green>[特性-回避] 玩家技能命中率降低 {evasionRate}% ({currentAccuracy} -> {roundContext.PlayerSkillAccuracy})</color>");
                }
            }
        }
    }
}
