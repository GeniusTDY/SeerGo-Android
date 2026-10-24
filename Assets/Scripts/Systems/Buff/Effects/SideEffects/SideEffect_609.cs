using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=609: 若对手{0}，技能威力翻倍麻痹0，中毒1，烧伤2，冻伤5，害怕6，疲惫7，睡眠8，石化9，冰封15）
    /// </summary>
    public class SideEffect_609 : ISkillSideEffectHandler
    {
        public override int EffectId => 609;

        public override void OnBeforeDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int state))
            {
                return;
            }

            if (!target.ActiveBuffs.ContainsKey(GetAbnormalConfigId(state)))
                return;

            if (caster is PlayerBattleController playerCaster)
            {
                RoundContext.Current.PlayerSkillPower *= 2;
            }
            else if (caster is MonsterBattleController monsterCaster)
            {
                RoundContext.Current.EnemySkillPower *= 2;
            }
        }

        private int GetAbnormalConfigId(int state)
        {
            return state switch
            {
                0 => 5,    // 麻痹→配置id=5
                1 => 1,    // 中毒→配置id=1
                2 => 3,    // 烧伤→配置id=3
                5 => 2,    // 冻伤→配置id=2
                6 => 7,    // 害怕→配置id=7
                7 => 6,    // 疲惫→配置id=6
                8 => 8,    // 睡眠→配置id=8
                9 => 9,    // 石化→配置id=9
                _ => -1    // 无匹配返回-1
            };
        }
    }
}
