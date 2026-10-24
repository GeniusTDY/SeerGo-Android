using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=15: 害怕 - 命中后{0}%令对方害怕
    /// </summary>
    public class SideEffect_15 : ISkillSideEffectHandler
    {
        public override int EffectId => 15;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int probability))
            {
                SeerGoLogger.LogError($"[害怕效果] 参数解析失败");
                return;
            }

            SeerGoLogger.Log($"<color=cyan>[害怕效果] 概率:{probability}%</color>");

            // 概率判定
            bool success = (probability >= 100) || (Random.Range(1, 101) <= probability);

            if (!success)
            {
                SeerGoLogger.Log($"[害怕效果] 概率判定失败");
                return;
            }

            // 应用害怕Buff (ID=7)
            SeerGoLogger.Log($"<color=yellow>[害怕效果] 概率判定成功，应用害怕Buff</color>");
            buffSystem.ApplyBuff(caster, target, 7);

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
                if (item.Value.Config.id != 7 && item.Value.Config.tab == 1)
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
