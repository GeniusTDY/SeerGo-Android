using QFramework;
using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 伤害变化（减半或者翻倍）
    /// </summary>
    public class DamageChangeBuffEffect : IBuffEffectHandler
    {
        public override bool CanHandle(string buffType)
        {
            return buffType == "FIRE_DAMAGE_HALVED" 
                || buffType == "PHYSICAL_DAMAGE_HALVED" 
                || buffType == "SPECIAL_DAMAGE_HALVED" 
                || buffType == "TYPE_DAMAGE_INCREASE" 
                || buffType == "DAMAGE_INCREASE"
                || buffType == "DAMAGE_REDUCED";
        }

        public override float GetStatMultiplier(IBuffable character, BuffInstance buff, string statType)
        {
            if (buff.Config.type != statType)
                return -10001f;

            // 火系伤害减半
            if (buff.Config.type == "FIRE_DAMAGE_HALVED" && GetCurrentSkillType(character) == 3)
            {
                SeerGoLogger.Log($"<color=yellow>[计算] 火系伤害减半{buff.EffectValue}</color>");
                return buff.EffectValue;
            }
            // 物理系伤害减半
            if (buff.Config.type == "PHYSICAL_DAMAGE_HALVED" && GetCurrentSkillCategory(character) == 1)
            {
                return buff.EffectValue;
            }
            // 技能技能效果减半
            if (buff.Config.type == "SPECIAL_DAMAGE_HALVED" && GetCurrentSkillCategory(character) == 2)
            {
                return buff.EffectValue;
            }
            // 精灵技能属性伤害加倍
            if(buff.Config.type == "TYPE_DAMAGE_INCREASE" && GetCurrentSkillType(character) == buff.Args[0].ToInt())
            {
                return buff.EffectValue;
            }
            // 伤害加倍
            if (buff.Config.type == "DAMAGE_INCREASE")
            {
                return buff.EffectValue;
            }
            // 伤害降低
            if (buff.Config.type == "DAMAGE_REDUCED")
            {
                return buff.EffectValue;
            }

            return -10001f;
        }

        private int GetCurrentSkillType(IBuffable character)
        {
            return character switch
            {
                PlayerBattleController => RoundContext.Current.PlayerSkill.Type,
                MonsterBattleController => RoundContext.Current.EnemySkill.Type,
                _ => -1
            };
        }

        private int GetCurrentSkillCategory(IBuffable character)
        {
            return character switch
            {
                PlayerBattleController => RoundContext.Current.PlayerSkill.Category,
                MonsterBattleController => RoundContext.Current.EnemySkill.Category,
                _ => -1
            };
        }
    }
}
