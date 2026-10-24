using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=201: 对选中对象或本方全体恢复1/{1}的体力
    /// argsNum=2, args[0]=目标选择(0=自身,1=全体), args[1]=体力分母
    /// </summary>
    public class SideEffect_201 : ISkillSideEffectHandler
    {
        public override int EffectId => 201;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 2 || !int.TryParse(args[1], out int denominator) || denominator <= 0)
            {
                SeerGoLogger.LogError($"[SideEffect_201] 参数解析失败");
                return;
            }

            if (!(caster is BattleEntityController casterController))
                return;

            int recoverHp = Mathf.RoundToInt(casterController.GetMaxHp() * (1f / denominator));
            if (recoverHp <= 0) return;

            RecoverHp(casterController, recoverHp);
            SeerGoLogger.Log($"[恢复体力] {casterController.GetCurrentPetData().Basic.Name}恢复{recoverHp}点体力");
        }

        private void RecoverHp(BattleEntityController controller, int amount)
        {
            if (controller is PlayerBattleController player)
                player.TakeDamage(amount, "+");
            else if (controller is MonsterBattleController monster)
                monster.TakeDamage(amount, "+");
        }
    }
}
