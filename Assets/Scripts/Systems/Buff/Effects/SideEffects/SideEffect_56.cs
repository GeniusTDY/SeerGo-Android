using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=56: 属性同化 - {0}回合内使自身的属性和对方的属性相同
    /// </summary>
    public class SideEffect_56 : ISkillSideEffectHandler
    {
        public override int EffectId => 56;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int duration))
            {
                SeerGoLogger.LogError($"[属性同化] 参数解析失败");
                return;
            }
            if (!(target is BattleEntityController targetController))
                return;
            // 获取目标的属性
            PetData targetData = targetController.GetCurrentPetData();
            if (targetData == null)
                return;

            // 应用属性同化Buff
            int buffId = 20007; // 属性同化Buff ID
            
            buffSystem.ApplyBuff(caster, target, buffId, new Dictionary<string, object>
            {
                { "RemainingTurns", duration.ToString() },
            });

            // 和属性反转互斥
            caster.ActiveBuffs.Remove(20006);

            SeerGoLogger.Log($"<color=yellow>[属性同化] 自身属性同化为目标属性，持续{duration}回合</color>");
        }
    }
}
