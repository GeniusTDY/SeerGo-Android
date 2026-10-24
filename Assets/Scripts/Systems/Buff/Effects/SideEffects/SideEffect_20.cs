using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=20: 疲惫1 - 命中后{0}%令自己疲惫n回合
    /// </summary>
    public class SideEffect_20 : ISkillSideEffectHandler
    {
        public override int EffectId => 20;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 2 || !int.TryParse(args[0], out int probability) || !int.TryParse(args[1], out int duration))
            {
                SeerGoLogger.LogError($"[SideEffect_20] 参数解析失败");
                return;
            }

            SeerGoLogger.Log($"<color=cyan>[SideEffect_20] 概率:{probability}%</color>");

            // 概率判定
            bool success = (probability >= 100) || (Random.Range(1, 101) <= probability);

            if (!success)
            {
                SeerGoLogger.Log($"[SideEffect_20] 概率判定失败");
                return;
            }

            // 应用疲惫1Buff (ID=6)
            SeerGoLogger.Log($"<color=yellow>[SideEffect_20] 概率判定成功，应用SideEffect_20Buff</color>");
            buffSystem.ApplyBuff(caster, target, 6, new Dictionary<string, object>
            {
                { "RemainingTurns", duration.ToString() },
                { "TargetType", BuffTargetType.Self }

            });

            List<int> tempRemoveBuffs = new List<int>();

            // 清理其他控制类异常buff
            if (caster is BattleEntityController battleController)
            {
                RemoveControlBuffsExcept(battleController.ActiveBuffs, tempRemoveBuffs);
            }
        }

        public void RemoveControlBuffsExcept(Dictionary<int, BuffInstance> activeBuffs, List<int> buffersToRemove)
        {
            foreach (var item in activeBuffs)
            {
                if (item.Value.Config.id != 6 && item.Value.Config.tab == 1)
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
