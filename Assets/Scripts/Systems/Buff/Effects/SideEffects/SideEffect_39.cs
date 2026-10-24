using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=39: PP封印 - {0}%降低对手所有技能{1}点PP值，然后若对手所选择的技能PP值为0则当回合对手无法行动
    /// </summary>
    public class SideEffect_39 : ISkillSideEffectHandler
    {
        public override int EffectId => 39;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 2 || !int.TryParse(args[0], out int probability) || !int.TryParse(args[1], out int ppReduction))
            {
                SeerGoLogger.LogError($"[PP封印] 参数解析失败");
                return;
            }

            if (!(target is BattleEntityController targetController))
                return;

            // 概率判定
            bool success = (probability >= 100) || (Random.Range(1, 101) <= probability);

            if (!success)
            {
                SeerGoLogger.Log($"[PP封印] 概率判定失败 ({probability}%)");
                return;
            }

            PetData targetData = targetController.GetCurrentPetData();
            if (targetData == null || targetData.Skills == null)
                return;

            // 减少所有技能的PP值
            foreach (var equippedSkill in targetData.Skills.NormalSkill)
            {
                if (equippedSkill.currentPP > 0)
                {
                    equippedSkill.currentPP = Mathf.Max(0, equippedSkill.currentPP - ppReduction);
                }
            }
        }
    }
}
