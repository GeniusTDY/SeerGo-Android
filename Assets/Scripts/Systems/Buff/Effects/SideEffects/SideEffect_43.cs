using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=43: 恢复自身最大体力的1/{0}
    /// </summary>
    public class SideEffect_43 : ISkillSideEffectHandler
    {
        public override int EffectId => 43;

        public override void OnAfterApplyDamage(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !float.TryParse(args[0], out float healMultiplier))
            {
                SeerGoLogger.LogError($"[SideEffect_43] 参数解析失败");
                return;
            }

            if (caster is PlayerBattleController player)
            {
                player.TakeDamage((int)(player.GetMaxHp() / healMultiplier), "+");
            }
            else if (caster is MonsterBattleController monster)
            {
                monster.TakeDamage((int)(monster.GetMaxHp() / healMultiplier), "+");
            }
        }
    }
}
