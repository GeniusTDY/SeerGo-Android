using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=129: 对方为{0}则技能威力翻倍
    /// args[0]=elementType (对手属性类型值)
    /// </summary>
    public class SideEffect_129 : ISkillSideEffectHandler
    {
        public override int EffectId => 129;

        public override void OnBeforeDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int checkType))
            {
                SeerGoLogger.LogError($"[SideEffect_129] 参数解析失败: {string.Join(",", args)}");
                return;
            }

            int targetType = target is PlayerBattleController
                ? RoundContext.Current.PlayerType
                : RoundContext.Current.EnemyType;

            if (targetType != checkType)
            {
                SeerGoLogger.Log($"[SideEffect_129] 对方属性({targetType})不匹配({checkType})，不触发");
                return;
            }

            if (caster is PlayerBattleController)
            {
                RoundContext.Current.PlayerSkillPower *= 2;
                SeerGoLogger.Log($"[SideEffect_129] 对手为指定属性，威力翻倍 => {RoundContext.Current.PlayerSkillPower}");
            }
            else if (caster is MonsterBattleController)
            {
                RoundContext.Current.EnemySkillPower *= 2;
                SeerGoLogger.Log($"[SideEffect_129] 对手为指定属性，威力翻倍 => {RoundContext.Current.EnemySkillPower}");
            }
        }
    }
}
