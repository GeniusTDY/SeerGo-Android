using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 特性1017：坚硬 - 受到任何伤害减少5%
    /// Args: "5 0" - 第一个参数是减伤百分比
    /// </summary>
    public class NewSeEffect_1017 : INewSeEffectHandler
    {
        public override int SeId => 1017;

        public override void OnBeforeApplyDamage(NewSeConfig seConfig, IBuffable caster, IBuffable target,
            SkillData skill, string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            // Args格式: "减伤百分比 标志"
            // 坚硬效果：在伤害应用前减少伤害
            if (args == null || args.Length < 1) return;

            if (!int.TryParse(args[0], out int damageReduction)) return;

            var roundContext = Controllers.Battle.RoundContext.Current;

            // caster是被攻击方（触发坚硬特性的那个）
            // target是攻击方
            if (caster is PlayerBattleController)
            {
                // 玩家被攻击，减少伤害
                if (roundContext.EnemyDamage > 0)
                {
                    int originalDamage = roundContext.EnemyDamage;
                    roundContext.EnemyDamage = (int)(originalDamage * (1 - damageReduction / 100.0f));
                    SeerGoLogger.Log($"<color=green>[特性-坚硬] 玩家受到伤害减少 {damageReduction}% ({originalDamage} -> {roundContext.EnemyDamage})</color>");
                }
            }
            else if (caster is MonsterBattleController)
            {
                // 敌方被攻击，减少伤害
                if (roundContext.PlayerDamage > 0)
                {
                    int originalDamage = roundContext.PlayerDamage;
                    roundContext.PlayerDamage = (int)(originalDamage * (1 - damageReduction / 100.0f));
                    SeerGoLogger.Log($"<color=green>[特性-坚硬] 敌方受到伤害减少 {damageReduction}% ({originalDamage} -> {roundContext.PlayerDamage})</color>");
                }
            }
        }
    }
}
