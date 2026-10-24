using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=34: 双倍奉还 - 将所受的伤害{0}倍反馈给对手
    /// </summary>
    public class SideEffect_34 : ISkillSideEffectHandler
    {
        public override int EffectId => 34;

        public override void OnBeforeApplyDamage(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !float.TryParse(args[0], out float damageMultiplier))
            {
                SeerGoLogger.LogError($"[SideEffect_34] 参数解析失败");
                return;
            }

            if (caster is PlayerBattleController player)
            {
                SeerGoLogger.Log($"<color=yellow>[双倍奉还]玩家 技能效果，敌人伤害:{RoundContext.Current.EnemyDamage}</color>");
                if(RoundContext.Current.EnemyDamage > 0)
                {
                    RoundContext.Current.PlayerDamage = (int)(RoundContext.Current.EnemyDamage * damageMultiplier);
                    RoundContext.Current.IsPlayerSkillCrit = false; // 双倍奉还清除暴击标记
                }
            }
            else if (caster is MonsterBattleController monster)
            {
                SeerGoLogger.Log($"<color=yellow>[双倍奉还]玩家 技能效果，玩家伤害:{RoundContext.Current.EnemyDamage}</color>");
                if (RoundContext.Current.PlayerDamage > 0)
                {
                    RoundContext.Current.EnemyDamage = (int)(RoundContext.Current.PlayerDamage * damageMultiplier);
                    RoundContext.Current.IsEnemySkillCrit = false; // 双倍奉还清除暴击标记
                }
            }
        }
    }
}
