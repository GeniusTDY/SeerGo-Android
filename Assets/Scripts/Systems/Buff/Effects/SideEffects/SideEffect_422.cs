using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=422:附加所造成伤害值{0}%的固定伤害
    /// </summary>
    public class SideEffect_422 : ISkillSideEffectHandler
    {
        public override int EffectId => 422;

        public override void OnAfterApplyDamage(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int multiple))
            {
                SeerGoLogger.LogError($"[SideEffect_422] 参数解析失败");
                return;
            }

            // 发起者伤害加成
            if (caster is PlayerBattleController playerBattle)
            {
                MonsterBattleController monster = target as MonsterBattleController;
                int ExtraDamage = (int)(RoundContext.Current.PlayerDamage * multiple / 100f);
                monster.TakeDamage(ExtraDamage);
            }
            else if (caster is MonsterBattleController monsterBattleController)
            {
                PlayerBattleController player = target as PlayerBattleController;
                int ExtraDamage = (int)(RoundContext.Current.EnemyDamage * multiple / 100f);
                player.TakeDamage(ExtraDamage);
            }
        }
    }
}
