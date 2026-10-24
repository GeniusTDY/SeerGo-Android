using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=130: 对方为{0}则附加{1}点伤害
    /// args[0]=elementType, args[1]=bonusDamage
    /// </summary>
    public class SideEffect_130 : ISkillSideEffectHandler
    {
        public override int EffectId => 130;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 2 || !int.TryParse(args[0], out int checkType) || !int.TryParse(args[1], out int bonusDamage))
            {
                SeerGoLogger.LogError($"[SideEffect_130] 参数解析失败: {string.Join(",", args)}");
                return;
            }

            int targetType = target is PlayerBattleController
                ? RoundContext.Current.PlayerType
                : RoundContext.Current.EnemyType;

            if (targetType != checkType)
            {
                SeerGoLogger.Log($"[SideEffect_130] 对方属性({targetType})不匹配({checkType})，不触发");
                return;
            }

            if (caster is PlayerBattleController)
            {
                RoundContext.Current.PlayerDamage += bonusDamage;
                SeerGoLogger.Log($"[SideEffect_130] 对手为指定属性，附加伤害 +{bonusDamage}");
            }
            else if (caster is MonsterBattleController)
            {
                RoundContext.Current.EnemyDamage += bonusDamage;
                SeerGoLogger.Log($"[SideEffect_130] 对手为指定属性，附加伤害 +{bonusDamage}");
            }
        }
    }
}
