using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 特性1016：会心 - 所有技能的致命一击率增加
    /// Args: "1 0" - 第一个参数是加成值，第二个是是否必定暴击(0表示增加暴击率)
    /// </summary>
    public class NewSeEffect_1016 : INewSeEffectHandler
    {
        public override int SeId => 1016;

        public override void OnBeforeDamageCalculation(NewSeConfig seConfig, IBuffable caster, IBuffable target,
            SkillData skill, string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            // Args格式: "加成值 标志"
            // 会心效果：在伤害计算前提升暴击率
            if (args == null || args.Length < 1) return;

            if (!int.TryParse(args[0], out int bonusRate)) return;

            var roundContext = Controllers.Battle.RoundContext.Current;

            if (caster is PlayerBattleController)
            {
                if (roundContext.PlayerSkill == skill)
                {
                    // 增加暴击率
                    int currentCritRate = roundContext.PlayerSkillCritRate;
                    roundContext.PlayerSkillCritRate = Mathf.Clamp(currentCritRate + bonusRate, 1, 16);
                    SeerGoLogger.Log($"<color=green>[特性-会心] 玩家技能暴击率增加 {bonusRate}% ({currentCritRate} -> {roundContext.PlayerSkillCritRate})</color>");
                }
            }
            else if (caster is MonsterBattleController)
            {
                if (roundContext.EnemySkill == skill)
                {
                    int currentCritRate = roundContext.EnemySkillCritRate;
                    roundContext.EnemySkillCritRate = Mathf.Clamp(currentCritRate + bonusRate, 1, 16);
                    SeerGoLogger.Log($"<color=green>[特性-会心] 敌方技能暴击率增加 {bonusRate}% ({currentCritRate} -> {roundContext.EnemySkillCritRate})</color>");
                }
            }
        }
    }
}
