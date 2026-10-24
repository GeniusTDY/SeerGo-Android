using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=10: 麻痹 - 命中后{0}%令对方麻痹
    /// </summary>
    public class SideEffect_10 : ISkillSideEffectHandler
    {
        public override int EffectId => 10;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int probability))
            {
                SeerGoLogger.LogError($"[麻痹效果] 参数解析失败");
                return;
            }

            SeerGoLogger.Log($"<color=cyan>[麻痹效果] 概率:{probability}%</color>");

            // 概率判定
            bool success = (probability >= 100) || (Random.Range(1, 101) <= probability);

            if (!success)
            {
                SeerGoLogger.Log($"[麻痹效果] 概率判定失败");
                return;
            }

            // 应用麻痹Buff (ID=5)
            SeerGoLogger.Log($"<color=yellow>[麻痹效果] 概率判定成功，应用麻痹Buff</color>");
            buffSystem.ApplyBuff(caster, target, 5);

            List<int> tempRemoveBuffs = new List<int>();

            // 清理其他控制类异常buff
            if (target is BattleEntityController battleController)
            {
                RemoveControlBuffsExcept(battleController.ActiveBuffs, tempRemoveBuffs);
            }

        }

        public void RemoveControlBuffsExcept(Dictionary<int, BuffInstance> activeBuffs, List<int> buffersToRemove)
        {
            foreach (var item in activeBuffs)
            {
                if (item.Value.Config.id != 5 && item.Value.Config.tab == 1)
                {
                    buffersToRemove.Add(item.Key);
                }
            }
            foreach (var id in buffersToRemove)
            {
                activeBuffs.Remove(id);
            }
        }
    }
}
