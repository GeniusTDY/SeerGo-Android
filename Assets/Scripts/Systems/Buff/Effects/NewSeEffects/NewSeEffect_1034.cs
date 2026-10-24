using SeerGoGame.Models;
using SeerGoGame.Systems;
using Random = UnityEngine.Random;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 特性1034：虚无 - 5%几率完全抵挡一次伤害
    /// Args: "1 5" - 第一个参数是标志，第二个是概率
    /// </summary>
    public class NewSeEffect_1034 : INewSeEffectHandler
    {
        public override int SeId => 1034;

        public override void OnBeforeApplyDamage(NewSeConfig seConfig, IBuffable caster, IBuffable target,
            SkillData skill, string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            // Args格式: "标志 概率"
            // 例如: "1 5" 表示5%几率完全抵挡一次伤害
            if (args == null || args.Length < 2) return;

            int flag = 0;
            int percentage = 0;

            if (!int.TryParse(args[0], out flag)) return;
            if (!int.TryParse(args[1], out percentage)) return;

            // caster是被攻击方（触发虚无特性的那个）
            // target是攻击方
            var roundContext = Controllers.Battle.RoundContext.Current;

            // 判断是否触发完全抵挡
            int randomValue = Random.Range(1, 101); // 1-100
            if (randomValue <= percentage)
            {
                // 设置伤害为0，表示完全抵挡
                if (caster is PlayerBattleController)
                {
                    // 玩家被攻击，完全抵挡敌方攻击
                    roundContext.EnemyDamage = 0;
                    SeerGoLogger.Log($"<color=green>[特性-虚无] 玩家精灵完全抵挡了敌方攻击 ({percentage}%几率触发)</color>");
                }
                else if (caster is MonsterBattleController)
                {
                    // 敌方被攻击，完全抵挡玩家攻击
                    roundContext.PlayerDamage = 0;
                    SeerGoLogger.Log($"<color=green>[特性-虚无] 敌方精灵完全抵挡了玩家攻击 ({percentage}%几率触发)</color>");
                }
            }
            else
            {
                SeerGoLogger.Log($"<color=yellow>[特性-虚无] 未触发 ({randomValue}/{percentage})</color>");
            }
        }
    }
}
