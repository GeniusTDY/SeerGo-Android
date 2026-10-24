using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=115: {0}%几率附加速度的1/{1}伤害
    /// args[0]=probability, args[1]=分母
    /// </summary>
    public class SideEffect_115 : ISkillSideEffectHandler
    {
        public override int EffectId => 115;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 2 || !int.TryParse(args[0], out int probability) || !int.TryParse(args[1], out int denominator))
            {
                SeerGoLogger.LogError($"[SideEffect_115] 参数解析失败: {string.Join(",", args)}");
                return;
            }

            bool success = (probability >= 100) || (Random.Range(1, 101) <= probability);
            if (!success)
            {
                SeerGoLogger.Log($"[SideEffect_115] 概率判定失败 ({probability}%)");
                return;
            }

            if (caster is BattleEntityController casterController)
            {
                int speed = casterController.GetCurrentPetData().CurrentStatus.Spd;
                int bonusDamage = speed / denominator;

                if (caster is PlayerBattleController)
                {
                    RoundContext.Current.PlayerDamage += bonusDamage;
                    SeerGoLogger.Log($"[SideEffect_115] 速度({speed})/分母({denominator}) => 附加伤害+{bonusDamage}");
                }
                else if (caster is MonsterBattleController)
                {
                    RoundContext.Current.EnemyDamage += bonusDamage;
                    SeerGoLogger.Log($"[SideEffect_115] 速度({speed})/分母({denominator}) => 附加伤害+{bonusDamage}");
                }
            }
        }
    }
}
