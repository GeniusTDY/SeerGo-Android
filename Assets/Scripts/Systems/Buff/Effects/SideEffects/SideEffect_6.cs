using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=6: 反弹伤害 - 对方所受伤害的1/{0}会反弹给自己
    /// 这个效果在伤害计算后处理，扣除施法者的HP
    /// </summary>
    public class SideEffect_6 : ISkillSideEffectHandler
    {
        public override int EffectId => 6;

        public override void OnAfterApplyDamage(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int recoilDivisor) || recoilDivisor == 0)
            {
                return;
            }

            int recoilDamage = 0;
            
            if (caster is PlayerBattleController playerCaster)
            {
                if (RoundContext.Current.PlayerDamage > 0)
                {
                    recoilDamage = RoundContext.Current.PlayerDamage / recoilDivisor;
                    // 反弹伤害给自己
                    if (recoilDamage <= 0) 
                        return;
                    playerCaster.TakeDamage(recoilDamage);
                    SeerGoLogger.Log($"<color=yellow>[反弹伤害] 玩家受到反弹伤害:{recoilDamage}</color>");
                }
            }
            else if (caster is MonsterBattleController monsterCaster)
            {
                if (RoundContext.Current.EnemyDamage > 0)
                {
                    recoilDamage = RoundContext.Current.EnemyDamage / recoilDivisor;
                    if (recoilDamage <= 0)
                        return;
                    monsterCaster.TakeDamage(recoilDamage);
                    SeerGoLogger.Log($"<color=yellow>[反弹伤害] 敌方受到反弹伤害:{recoilDamage}</color>");
                }
            }
        }
    }
}
