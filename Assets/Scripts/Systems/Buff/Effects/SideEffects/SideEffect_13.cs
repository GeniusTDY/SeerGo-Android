using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=13: 寄生 - {0}回合吸取对方最大体力的1/8(对草系无效)
    /// </summary>
    public class SideEffect_13 : ISkillSideEffectHandler
    {
        public override int EffectId => 13;

        public override void OnAfterCastSkill(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (target is BattleEntityController battleEntity)
            {
                // 草系无效
                if (battleEntity.GetCurrentPetData().Basic.Attributes == 1)
                {
                    SeerGoLogger.Log($"[寄生效果] 草系无效");
                    return;
                }
            }
            if (args.Length < 1 || !int.TryParse(args[0], out int duration))
            {
                SeerGoLogger.LogError($"[寄生效果] 参数解析失败");
                return;
            }

            SeerGoLogger.Log($"<color=cyan>[寄生效果] 回合:{duration}</color>");

            // 应用寄生Buff (ID=12)
            SeerGoLogger.Log($"<color=yellow>[寄生效果] 概率判定成功，应用寄生Buff</color>");
            buffSystem.ApplyBuff(caster, target, 12, new Dictionary<string, object>
            {
                { "RemainingTurns",  duration.ToString()},
            });
        }

    }
}
