using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=511: {0}%概率威力翻倍，体力低于1/{1}时概率增加{2}%
    /// argsNum=3, args[0]=基础概率%, args[1]=体力阈值分母, args[2]=额外概率%
    /// </summary>
    public class SideEffect_511 : ISkillSideEffectHandler
    {
        public override int EffectId => 511;

        public override void OnBeforeDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 3 || !int.TryParse(args[0], out int baseProb) ||
                !int.TryParse(args[1], out int hpDenominator) || !int.TryParse(args[2], out int bonusProb))
            {
                SeerGoLogger.LogError($"[SideEffect_511] 参数解析失败");
                return;
            }

            int finalProb = baseProb;

            // 检查当前体力是否低于阈值
            if (caster is BattleEntityController casterController)
            {
                int maxHp = casterController.GetMaxHp();
                int currentHp = casterController.GetCurrentPetData().CurrentStatus.Hp;
                float threshold = maxHp * (1f / hpDenominator);

                if (currentHp < threshold)
                {
                    finalProb = baseProb + bonusProb;
                    SeerGoLogger.Log($"[SideEffect_511] 体力低于1/{hpDenominator}，概率提升: {baseProb}%+{bonusProb}%={finalProb}%");
                }
            }

            bool success = (finalProb >= 100) || (UnityEngine.Random.Range(1, 101) <= finalProb);
            if (!success)
            {
                SeerGoLogger.Log($"[SideEffect_511] 概率判定失败 ({finalProb}%)");
                return;
            }

            // 技能威力翻倍
            if (caster is PlayerBattleController)
            {
                RoundContext.Current.PlayerSkillPower *= 2;
                SeerGoLogger.Log($"[SideEffect_511] 玩家技能威力翻倍 → {RoundContext.Current.PlayerSkillPower}");
            }
            else if (caster is MonsterBattleController)
            {
                RoundContext.Current.EnemySkillPower *= 2;
                SeerGoLogger.Log($"[SideEffect_511] 对手技能威力翻倍 → {RoundContext.Current.EnemySkillPower}");
            }
        }
    }
}
