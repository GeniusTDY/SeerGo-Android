using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using Random = UnityEngine.Random;

/**
 * 技能效果: 异常状态
 */
public class StatusConditionSideEffect : ISkillSideEffectHandler
{
    private int effectId;

    public override int EffectId => effectId;

    public StatusConditionSideEffect(int id)
    {
        effectId = id;
    }

    public override void OnAfterCastSkill(IBuffable caster, IBuffable target, SkillData skill, string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
    {
        if (!int.TryParse(args[0], out int probability))
        {
            SeerGoLogger.LogError($"[技能效果] 参数解析失败: {string.Join(",", args)}");
            return;
        }

        SeerGoLogger.Log($"<color=cyan>[异常状态] 施法者类型:{caster.GetType().Name}, 目标类型:{target.GetType().Name}, 效果ID:{EffectId}, 概率:{probability}%</color>");

        // 概率判定
        bool success = (probability >= 100) || (Random.Range(1, 101) <= probability);

        if (!success)
        {
            SeerGoLogger.Log($"[异常状态] 概率判定失败 ({probability}%)");
            return;
        }
        
        int buffId = EffectId switch
        {
            11 => 1,// 中毒
            12 => 3,// 烧伤
            14 => 2,// 冻伤
            _ => 0 //默认无效果
        };

        if (buffId == 0)
        {
            SeerGoLogger.LogError($"[异常状态] 无效的效果ID: {EffectId}");
            return;
        }

        SeerGoLogger.Log($"<color=yellow>[异常状态] 概率判定成功，准备应用Buff ID:{buffId}</color>");
        buffSystem.ApplyBuff(caster, target, buffId);
    }
}
