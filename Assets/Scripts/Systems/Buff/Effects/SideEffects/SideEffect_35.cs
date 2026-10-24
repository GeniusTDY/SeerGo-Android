using SeerGoGame.Controllers.Battle;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=35: 惩罚 - 对方能力等级越高，此技能威力越大
    /// 计算对方所有正面能力等级之和，每级增加20威力
    /// </summary>
    public class SideEffect_35 : ISkillSideEffectHandler
    {
        public override int EffectId => 35;

        public override void OnBeforeDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            // 统计目标的所有正面Buff等级
            int totalPositiveLevel = 0;

            foreach (var buff in target.ActiveBuffs.Values)
            {
                // 只统计属性提升类Buff (ID 10000-10005) 且效果值为正
                if (buff.Config.id >= 10000 && buff.Config.id <= 10005 && buff.EffectValue > 0)
                {
                    totalPositiveLevel += (int)buff.EffectValue;
                }
            }

            if (totalPositiveLevel <= 0)
            {
                SeerGoLogger.Log($"[惩罚效果] 目标没有正面能力提升，无额外威力");
                return;
            }
            // 最多6级
            totalPositiveLevel = Mathf.Min(totalPositiveLevel, 6);

            // 每级增加20威力
            if (caster is PlayerBattleController)
            {
                RoundContext.Current.PlayerSkillPower += totalPositiveLevel * 20;
                SeerGoLogger.Log($"<color=yellow>[惩罚效果] 玩家能力等级+{totalPositiveLevel}，威力从提升到{RoundContext.Current.PlayerSkillPower}</color>");

            }
            else if (caster is MonsterBattleController)
            {
                RoundContext.Current.EnemySkillPower += totalPositiveLevel * 20;
                SeerGoLogger.Log($"<color=yellow>[惩罚效果] 敌方能力等级+{totalPositiveLevel}，威力从提升到{RoundContext.Current.EnemySkillPower}</color>");

            }


        }
    }
}
