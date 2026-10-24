using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=438: {0}%的几率恢复自身体力的1/{1}
    /// argsNum=2, args[0]=概率%, args[1]=体力分母
    /// </summary>
    public class SideEffect_438 : ISkillSideEffectHandler
    {
        public override int EffectId => 438;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 2 || !int.TryParse(args[0], out int probability) || !int.TryParse(args[1], out int denominator))
            {
                SeerGoLogger.LogError($"[SideEffect_438] 参数解析失败");
                return;
            }

            if (denominator <= 0) return;

            bool success = (probability >= 100) || (Random.Range(1, 101) <= probability);
            if (!success)
            {
                SeerGoLogger.Log($"[SideEffect_438] 概率判定失败 ({probability}%)");
                return;
            }

            if (!(caster is BattleEntityController casterController))
                return;

            int recoverHp = Mathf.RoundToInt(casterController.GetMaxHp() * (1f / denominator));
            if (recoverHp <= 0) return;

            if (casterController is PlayerBattleController player)
                player.TakeDamage(recoverHp, "+");
            else if (casterController is MonsterBattleController monster)
                monster.TakeDamage(recoverHp, "+");

            SeerGoLogger.Log($"[SideEffect_438] 概率判定成功，恢复{recoverHp}点体力");
        }
    }
}
