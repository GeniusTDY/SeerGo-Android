using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=29:额外增加{0}点固定伤害"
    /// </summary>
    public class SideEffect_29 : ISkillSideEffectHandler
    {
        public override int EffectId => 29;

        public override void OnAfterApplyDamage(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int ExtraDamage))
            {
                SeerGoLogger.LogError($"[额外增加n点固定伤害效果] 参数解析失败");
                return;
            }
            SeerGoLogger.Log($"<color=cyan>[额外增加n点固定伤害效果] n:{ExtraDamage}</color>");
            // 发起者伤害加成
            if (caster is PlayerBattleController playerBattle)
            {
                MonsterBattleController monster = target as MonsterBattleController;
                monster.TakeDamage(ExtraDamage);
            }
            else if (caster is MonsterBattleController monsterBattleController)
            {
                PlayerBattleController player = target as PlayerBattleController;
                player.TakeDamage(ExtraDamage);
            }
        }
    }
}
