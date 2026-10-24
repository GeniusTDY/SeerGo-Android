using QFramework;
using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 最后一滴血需要指定技能造成伤害
    /// </summary>
    public class SkillOnlyDamageBuffEffect : IBuffEffectHandler
    {
        private const int TargetSkillId = 10100;
        private const int BuffToRemoveId = 30008;
        private const int FixedDamage = 1;

        public override bool CanHandle(string buffType) 
        {
            return buffType == "SKILL_ONLY_DAMAGE";
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            return -10001f;
        }

        /// <summary>
        ///  最后一滴血需要指定技能造成伤害
        /// </summary>
        public static bool IsImmuneToSkillOnlyDamage(IBuffable character)
        {

            // 指定技能移除buff  贯穿水枪
            int enemySkillId = RoundContext.Current.EnemySkill?.ID.ToInt() ?? 0;
            int playerSkillId = RoundContext.Current.PlayerSkill?.ID.ToInt() ?? 0;

            if ((character is PlayerBattleController && enemySkillId == TargetSkillId) ||
                (character is MonsterBattleController && playerSkillId == TargetSkillId))
            {
                (character as PlayerBattleController)?.TakeDamage(FixedDamage);
                (character as MonsterBattleController)?.TakeDamage(FixedDamage);

                return false;
            }

            return character.ActiveBuffs.ContainsKey(30008); // 技能免疫
        }
    }
}
