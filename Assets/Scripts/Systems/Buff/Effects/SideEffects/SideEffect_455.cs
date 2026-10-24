using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=455: 每损失{0}点体力则额外附加{1}点固定伤害
    /// argsNum=2, args[0]=损失体力阈值, args[1]=额外固定伤害
    /// 示例: 每损失100点体力则附加20点固伤
    /// </summary>
    public class SideEffect_455 : ISkillSideEffectHandler
    {
        public override int EffectId => 455;

        public override void OnAfterApplyDamage(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 2 || !int.TryParse(args[0], out int hpThreshold) || !int.TryParse(args[1], out int extraDamage))
            {
                SeerGoLogger.LogError($"[SideEffect_455] 参数解析失败");
                return;
            }

            if (hpThreshold <= 0 || extraDamage <= 0) return;

            if (!(caster is BattleEntityController casterController))
                return;

            // 计算已损失体力值
            int maxHp = casterController.GetMaxHp();
            int currentHp = casterController.GetCurrentPetData().CurrentStatus.Hp;
            int lostHp = maxHp - currentHp;

            if (lostHp < hpThreshold) return;

            // 计算额外固定伤害: (已损失体力 / 阈值) * 每段额外伤害
            int extraTotalDamage = (lostHp / hpThreshold) * extraDamage;

            // 对目标施加额外固定伤害
            if (caster is PlayerBattleController && target is MonsterBattleController monsterTarget)
            {
                monsterTarget.TakeDamage(extraTotalDamage);
                SeerGoLogger.Log($"[SideEffect_455] 玩家损失{lostHp}点体力，额外附加{extraTotalDamage}点固伤");
            }
            else if (caster is MonsterBattleController && target is PlayerBattleController playerTarget)
            {
                playerTarget.TakeDamage(extraTotalDamage);
                SeerGoLogger.Log($"[SideEffect_455] 对手损失{lostHp}点体力，额外附加{extraTotalDamage}点固伤");
            }
        }
    }
}
