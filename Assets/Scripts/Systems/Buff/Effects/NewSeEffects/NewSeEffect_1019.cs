using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using Random = UnityEngine.Random;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 特性1019：顽强 - 受到致死攻击时有5%几率余下1点体力
    /// Args: "5 1" - 第一个参数是概率，第二个是标志
    /// </summary>
    public class NewSeEffect_1019 : INewSeEffectHandler
    {
        public override int SeId => 1019;

        public override void OnAfterApplyDamage(NewSeConfig seConfig, IBuffable caster, IBuffable target,
            SkillData skill, string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            // Args格式: "概率 标志"
            // 顽强效果：在伤害应用后检查是否触发
            if (args == null || args.Length < 1) return;

            if (!int.TryParse(args[0], out int probability)) return;

            var roundContext = Controllers.Battle.RoundContext.Current;

            // caster是被攻击方（触发顽强特性的那个）
            // target是攻击方
            if (caster is PlayerBattleController playerTarget)
            {
                // 检查玩家是否应该被击杀
                var petData = playerTarget.GetCurrentPetData();
                if (petData != null && petData.CurrentStatus.Hp <= 0)
                {
                    // 触发顽强效果
                    if (Random.Range(1, 101) <= probability)
                    {
                        int newHp = 1;
                        playerTarget.SetPlayerHp(newHp, petData);
                        SeerGoLogger.Log($"<color=green>[特性-顽强] 玩家精灵触发顽强效果，保留1点体力</color>");
                    }
                }
            }
            else if (caster is MonsterBattleController monsterTarget)
            {
                // 检查敌方是否应该被击杀
                var petData = monsterTarget.GetCurrentMonsterData();
                if (petData != null && petData.CurrentStatus.Hp <= 0)
                {
                    // 触发顽强效果
                    if (Random.Range(1, 101) <= probability)
                    {
                        int newHp = 1;
                        monsterTarget.SetMonsterHp(newHp, petData);
                        SeerGoLogger.Log($"<color=green>[特性-顽强] 敌方精灵触发顽强效果，保留1点体力</color>");
                    }
                }
            }
        }
    }
}
