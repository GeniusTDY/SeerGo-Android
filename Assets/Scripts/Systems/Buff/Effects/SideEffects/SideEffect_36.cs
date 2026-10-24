using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=36: 命中时{0}%的概率秒杀对方"
    /// </summary>
    public class SideEffect_36 : ISkillSideEffectHandler
    {
        public override int EffectId => 36;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {

            if (args.Length < 1 || !int.TryParse(args[0], out int probability))
            {
                SeerGoLogger.LogError($"[SideEffect_36] 参数解析失败");
                return;
            }

            // 概率判定
            bool success = (probability >= 100) || (Random.Range(1, 101) <= probability);

            if (!success)
            {
                SeerGoLogger.Log($"[SideEffect_36] 概率判定失败");
                return;
            }

            if (target is PlayerBattleController player)
            {
                RoundContext.Current.EnemyDamage = player.GetCurrentPetData().CurrentStatus.Hp;
                RoundContext.Current.IsEnemySkillCrit = false; // 秒杀效果清除暴击标记
                SeerGoLogger.Log($"[SideEffect_36] 敌方概率判定成功" + player.GetCurrentPetData().CurrentStatus.Hp);
            }
            else if (target is MonsterBattleController monster)
            {
                RoundContext.Current.PlayerDamage = monster.GetCurrentPetData().CurrentStatus.Hp;
                RoundContext.Current.IsPlayerSkillCrit = false; // 秒杀效果清除暴击标记
                SeerGoLogger.Log($"[SideEffect_36] 玩家概率判定成功" + monster.GetCurrentPetData().CurrentStatus.Hp);
            }
        }
    }
}
