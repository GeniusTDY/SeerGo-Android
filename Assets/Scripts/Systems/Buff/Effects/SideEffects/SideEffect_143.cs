using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=143: 使对手的能力提升效果反转成能力下降效果
    /// argsNum=0
    /// </summary>
    public class SideEffect_143 : ISkillSideEffectHandler
    {
        private static readonly int[] StatChangeBuffIds =
        {
            10000, // 攻击
            10001, // 防御
            10002, // 特攻
            10003, // 特防
            10004, // 速度
            10005, // 命中
        };

        public override int EffectId => 143;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (!(target is BattleEntityController targetController))
                return;

            foreach (var statId in StatChangeBuffIds)
            {
                if (target.ActiveBuffs.TryGetValue(statId, out var statBuff))
                {
                    float originalValue = statBuff.EffectValue;
                    statBuff.EffectValue = -originalValue;
                    SeerGoLogger.Log($"[能力反转] 对手{statBuff.Config.name} {originalValue:+#;-#;0} → {-originalValue:+#;-#;0}");
                }
            }
        }
    }
}
