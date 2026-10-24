using QFramework;
using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 尤纳斯暗影屏障，免疫伤害
    /// </summary>
    public class ShadowBarrierBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType)
        {
            return buffType == "SHADOW_BARRIER";
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            return -10001f;
        }

        /// <summary>
        /// 检查是否免疫伤害
        /// </summary>
        public static bool IsImmuneToDamage(IBuffable character)
        {
            const int TargetSkillId = 10323;
            const int ImmuneBuffId = 30007;

            int enemySkillId = RoundContext.Current.EnemySkill?.ID.ToInt() ?? 0;
            int playerSkillId = RoundContext.Current.PlayerSkill?.ID.ToInt() ?? 0;

            if ((character is PlayerBattleController && enemySkillId == TargetSkillId) ||
                (character is MonsterBattleController && playerSkillId == TargetSkillId))
            {
                character.RemoveBuff(ImmuneBuffId);
                return false;
            }

            // 空值或条件不满足时，返回伤害免疫判断结果
            return character.ActiveBuffs.ContainsKey(ImmuneBuffId);
        }

    }
}
