using QFramework;
using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=9: 连击增强 - 连续使用每次威力增加{0}，最高威力{1}
    /// 需要通过Buff来跟踪连击次数
    /// </summary>
    public class SideEffect_9 : ISkillSideEffectHandler
    {
        public override int EffectId => 9;

        public override void OnBeforeDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 2 || !int.TryParse(args[0], out int powerIncrease) || !int.TryParse(args[1], out int maxPower))
            {
                return;
            }

            int skillId = skill.ID.ToInt();
            bool isPlayer = caster is PlayerBattleController;

            // 获取对应的字典
            var useCountDict = isPlayer
                ? RoundContext.Current.PlayerSkillUseCount
                : RoundContext.Current.EnemySkillUseCount;

            // 更新连击次数
            useCountDict.TryGetValue(skillId, out int consecutiveCount);

            // 计算威力提升
            int totalPowerIncrease = powerIncrease * consecutiveCount;

            // 获取当前威力并计算最终威力
            int currentPower = isPlayer
                ? RoundContext.Current.PlayerSkillPower
                : RoundContext.Current.EnemySkillPower;

            int finalPower = Math.Min(currentPower + totalPowerIncrease, maxPower);

            // 设置最终威力
            if (isPlayer)
            {
                RoundContext.Current.PlayerSkillPower = finalPower;
                SeerGoLogger.Log($"<color=cyan>[玩家连击增强] 技能:{skill.Name}, 连击次数:{consecutiveCount}, 最终威力:{finalPower}</color>");

            }
            else
            {
                RoundContext.Current.EnemySkillPower = finalPower;
                SeerGoLogger.Log($"<color=cyan>[敌人连击增强] 技能:{skill.Name}, 连击次数:{consecutiveCount}, 最终威力:{finalPower}</color>");

            }


            useCountDict[skillId] = ++consecutiveCount;

        }

    }
}
