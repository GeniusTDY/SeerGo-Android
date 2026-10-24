using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=95: 对手处于睡眠状态时，致命一击率提升{0}%（n/16）
    /// </summary>
    public class SideEffect_95 : ISkillSideEffectHandler
    {
        public override int EffectId => 95;

        public override void OnBeforeDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int critIncrease))
            {
                SeerGoLogger.LogError($"[SideEffect_95] 参数解析失败: {string.Join(",", args)}");
                return;
            }

            // 检查目标是否处于睡眠状态 (buff id=8)
            if (target is BattleEntityController targetController)
            {
                if (targetController.ActiveBuffs.ContainsKey(8))
                {
                    // 提升致命一击率: n/16
                    if (caster is PlayerBattleController)
                    {
                        RoundContext.Current.PlayerSkillCritRate += critIncrease;
                        SeerGoLogger.Log($"[SideEffect_95] 对手睡眠，暴击率提升 +{critIncrease}/16");
                    }
                    else if (caster is MonsterBattleController)
                    {
                        RoundContext.Current.EnemySkillCritRate += critIncrease;
                        SeerGoLogger.Log($"[SideEffect_95] 对手睡眠，暴击率提升 +{critIncrease}/16");
                    }
                }
            }
        }
    }
}
