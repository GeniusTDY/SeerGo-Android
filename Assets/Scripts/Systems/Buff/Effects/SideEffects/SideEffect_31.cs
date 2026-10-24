using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=31: 1回合做{0}~{1}次攻击
    /// </summary>
    public class SideEffect_31 : ISkillSideEffectHandler
    {
        public override int EffectId => 31;

        public override void OnBeforeDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {

            if (args.Length < 2 || !int.TryParse(args[0], out int count1) || !int.TryParse(args[1], out int count2))
            {
                SeerGoLogger.LogError($"[1回合做m~n次攻击效果] 参数解析失败");
                return;
            }

            int counts = Random.Range(count1, count2 + 1);


            if (caster is PlayerBattleController)
            {
                RoundContext.Current.PlayerSkillCaclDamageCount = counts;
            }
            else if (caster is MonsterBattleController)
            {
                RoundContext.Current.EnemySkillCaclDamageCount = counts;
            }
        }
    }
}
