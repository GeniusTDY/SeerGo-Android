using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using Random = UnityEngine.Random;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 特性1020：瞬杀 - 进攻类技能有5%几率秒杀对方
    /// Args: "5 0" - 第一个参数是概率，第二个是标志
    /// </summary>
    public class NewSeEffect_1020 : INewSeEffectHandler
    {
        public override int SeId => 1020;

        public override void OnBeforeApplyDamage(NewSeConfig seConfig, IBuffable caster, IBuffable target,
            SkillData skill, string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            // Args格式: "概率 标志"
            // 瞬杀效果：在伤害应用前检查是否触发秒杀
            if (args == null || args.Length < 1) return;

            if (!int.TryParse(args[0], out int probability)) return;

            var roundContext = Controllers.Battle.RoundContext.Current;

            // 检查是否为伤害型技能
            if (skill.Category == 1 || skill.Category == 2)
            {
                if (Random.Range(1, 101) <= probability)
                {
                    if (caster is PlayerBattleController)
                    {
                        // 玩家攻击秒杀敌方
                        if (roundContext.PlayerDamage > 0)
                        {
                            var monsterData = (target as MonsterBattleController)?.GetCurrentMonsterData();
                            if (monsterData != null)
                            {
                                roundContext.PlayerDamage = monsterData.CurrentStatus.Hp;
                                SeerGoLogger.Log($"<color=green>[特性-瞬杀] 玩家触发瞬杀效果，秒杀敌方精灵</color>");
                            }
                        }
                    }
                    else if (caster is MonsterBattleController)
                    {
                        // 敌方攻击秒杀玩家
                        if (roundContext.EnemyDamage > 0)
                        {
                            var playerData = (target as PlayerBattleController)?.GetCurrentPetData();
                            if (playerData != null)
                            {
                                roundContext.EnemyDamage = playerData.CurrentStatus.Hp;
                                SeerGoLogger.Log($"<color=green>[特性-瞬杀] 敌方触发瞬杀效果，秒杀玩家精灵</color>");
                            }
                        }
                    }
                }
            }
        }
    }
}
