using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=94: 命中后{0}%令对手石化
    /// </summary>
    public class SideEffect_94 : ISkillSideEffectHandler
    {
        public override int EffectId => 94;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int probability))
            {
                SeerGoLogger.LogError($"[SideEffect_94] 参数解析失败: {string.Join(",", args)}");
                return;
            }

            SeerGoLogger.Log($"<color=cyan>[石化效果] 概率:{probability}%</color>");

            // 概率判定
            bool success = (probability >= 100) || (Random.Range(1, 101) <= probability);

            if (!success)
            {
                SeerGoLogger.Log($"[SideEffect_94] 概率判定失败");
                return;
            }

            // 应用石化Buff (ID=9)
            SeerGoLogger.Log($"<color=yellow>[石化效果] 概率判定成功，应用石化Buff</color>");
            buffSystem.ApplyBuff(caster, target, 9);

            // 清理其他控制类异常buff (tab==1)
            if (target is BattleEntityController battleController)
            {
                List<int> tempRemoveBuffs = new List<int>();
                foreach (var item in battleController.ActiveBuffs)
                {
                    if (item.Value.Config.id != 9 && item.Value.Config.tab == 1)
                    {
                        tempRemoveBuffs.Add(item.Key);
                    }
                }
                foreach (var id in tempRemoveBuffs)
                {
                    battleController.ActiveBuffs.Remove(id);
                }
            }
        }
    }
}
