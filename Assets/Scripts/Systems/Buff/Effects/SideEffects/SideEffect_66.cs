using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=66: 当次攻击击败对方出战精灵时恢复自身最大体力的1/{0}
    /// </summary>
    public class SideEffect_66 : ISkillSideEffectHandler
    {
        public override int EffectId => 66;

        public override void OnAfterApplyDamage(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int multiple))
            {
                return;
            }

            int heal = 0;
            
            if (caster is PlayerBattleController playerCaster)
            {
                MonsterBattleController monsterBattle = target as MonsterBattleController;

                if (monsterBattle.GetCurrentMonsterData().CurrentStatus.Hp <= 0)
                {
                    heal = (int)(playerCaster.GetMaxHp() * (1f/multiple));

                    playerCaster.TakeDamage(heal,"+");
                    SeerGoLogger.Log($"<color=yellow>恢复:{heal}</color>");
                }
            }
            else if (caster is MonsterBattleController monsterCaster)
            {
                PlayerBattleController playerBattle = target as PlayerBattleController;
                if (playerBattle.GetCurrentPetData().CurrentStatus.Hp <= 0)
                {
                    heal = (int)(monsterCaster.GetMaxHp() * (1f / multiple));

                    monsterCaster.TakeDamage(heal);
                    SeerGoLogger.Log($"<color=yellow>恢复:{heal}</color>");
                }
            }
        }
    }
}
