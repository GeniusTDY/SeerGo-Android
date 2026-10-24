using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 特性1001：叶绿 - 草属性技能威力增加5%
    /// Args: "1 5" - 第一个参数是属性ID，第二个是百分比
    /// </summary>
    public class NewSeEffect_SkillPowerUP : INewSeEffectHandler
    {
        private int seId;
        public override int SeId => seId;

        public NewSeEffect_SkillPowerUP(int id)
        {
            seId = id;
        }

        public override void OnBeforeDamageCalculation(NewSeConfig seConfig, IBuffable caster, IBuffable target,
            SkillData skill, string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            // Args格式: "属性ID 百分比"
            // 例如: "1 5" 表示草属性技能威力增加5%
            if (args == null || args.Length < 2) return;

            int elementId = 0;
            int percentage = 0;

            if (!int.TryParse(args[0], out elementId)) return;
            if (!int.TryParse(args[1], out percentage)) return;

            // 检查技能属性是否匹配
            if (skill.Type == elementId)
            {
                // 修改技能威力
                // 这里通过修改RoundContext中的技能威力来实现
                var roundContext = Controllers.Battle.RoundContext.Current;

                if (caster is PlayerBattleController)
                {
                    // 玩家技能
                    if (roundContext.PlayerSkill == skill)
                    {
                        int basePower = roundContext.PlayerSkillPower;
                        int extraPower = (int)(basePower * percentage / 100.0f);
                        roundContext.PlayerSkillPower = basePower + extraPower;
                        SeerGoLogger.Log($"<color=green>[特性-叶绿] 草属性技能威力增加 {percentage}% ({basePower} -> {roundContext.PlayerSkillPower})</color>");
                    }
                }
                else if (caster is MonsterBattleController)
                {
                    // 敌方技能
                    if (roundContext.EnemySkill == skill)
                    {
                        int basePower = roundContext.EnemySkillPower;
                        int extraPower = (int)(basePower * percentage / 100.0f);
                        roundContext.EnemySkillPower = basePower + extraPower;
                        SeerGoLogger.Log($"<color=green>[特性-叶绿] 草属性技能威力增加 {percentage}% ({basePower} -> {roundContext.EnemySkillPower})</color>");
                    }
                }
            }
        }
    }
}
