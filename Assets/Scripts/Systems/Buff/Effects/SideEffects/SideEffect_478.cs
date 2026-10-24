using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=478: {0}回合内令对手使用的属性技能无效（BOSS有效）
    /// argsNum=1, args[0]=持续回合数
    /// </summary>
    public class SideEffect_478 : ISkillSideEffectHandler
    {
        public override int EffectId => 478;

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int duration))
            {
                SeerGoLogger.LogError($"[SideEffect_478] 参数解析失败");
                return;
            }

            buffSystem.ApplyBuff(caster, target, 20041, new Dictionary<string, object>
            {
                { "RemainingTurns", duration.ToString() }
            });

            SeerGoLogger.Log($"[SideEffect_478] 施加{duration}回合属性技能无效buff");
        }
    }
}
