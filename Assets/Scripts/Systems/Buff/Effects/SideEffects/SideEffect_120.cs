using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=120: 50%几率对方减血1/{0}，50%几率自己减血1/{0}
    /// </summary>
    public class SideEffect_120 : ISkillSideEffectHandler
    {
        public override int EffectId => 120;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !float.TryParse(args[0], out float fraction))
            {
                SeerGoLogger.LogError($"[SideEffect_120] 参数解析失败: {string.Join(",", args)}");
                return;
            }

            float roll = Random.Range(0, 100);
            BattleEntityController casterCtrl = caster as BattleEntityController;
            BattleEntityController targetCtrl = target as BattleEntityController;

            if (roll < 50)
            {
                // 对方减血
                if (targetCtrl != null)
                {
                    int hpLoss = (int)(targetCtrl.GetMaxHp() / fraction);
                    targetCtrl.TakeDamage(hpLoss);
                    SeerGoLogger.Log($"[SideEffect_120] 对方减血: {hpLoss}");
                }
            }
            else
            {
                // 自己减血
                if (casterCtrl != null)
                {
                    int hpLoss = (int)(casterCtrl.GetMaxHp() / fraction);
                    casterCtrl.TakeDamage(hpLoss);
                    SeerGoLogger.Log($"[SideEffect_120] 自己减血: {hpLoss}");
                }
            }
        }
    }
}
