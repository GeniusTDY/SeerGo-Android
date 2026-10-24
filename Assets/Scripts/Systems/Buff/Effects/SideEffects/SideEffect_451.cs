using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=451: 命中后{0}%随机令对手进入烧伤，冻伤，中毒中的{1}种
    /// argsNum=2, args[0]=概率%, args[1]=随机施加几种状态(1~3)
    /// </summary>
    public class SideEffect_451 : ISkillSideEffectHandler
    {
        public override int EffectId => 451;

        // 状态列表: 烧伤=3, 冻伤=2, 中毒=1
        private static readonly int[] StatusBuffIds = { 1, 2, 3 };

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 2 || !int.TryParse(args[0], out int probability) || !int.TryParse(args[1], out int count))
            {
                SeerGoLogger.LogError($"[SideEffect_451] 参数解析失败");
                return;
            }

            if (count <= 0 || count > 3) count = 1;

            bool success = (probability >= 100) || (Random.Range(1, 101) <= probability);
            if (!success)
            {
                SeerGoLogger.Log($"[SideEffect_451] 概率判定失败 ({probability}%)");
                return;
            }

            // 随机选择N种状态
            var shuffled = new System.Collections.Generic.List<int>(StatusBuffIds);
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }

            for (int i = 0; i < count; i++)
            {
                buffSystem.ApplyBuff(caster, target, shuffled[i]);
                SeerGoLogger.Log($"[SideEffect_451] 施加状态Buff ID={shuffled[i]}");
            }
        }
    }
}
