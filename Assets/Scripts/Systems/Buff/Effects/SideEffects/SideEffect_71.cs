using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=71: 牺牲暴击 - 自己牺牲(体力降到0)，使下一只出战精灵在前两回合内必定致命一击
    /// </summary>
    public class SideEffect_71 : ISkillSideEffectHandler
    {
        public override int EffectId => 71;

        public override void OnAfterCastSkill(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (!(caster is BattleEntityController casterController))
                return;

            PetData casterData = casterController.GetCurrentPetData();
            if (casterData == null)
                return;

            // 将自身HP降为0
            int originalHp = casterData.CurrentStatus.Hp;

            // 更新UI显示 - 传入实际扣除的HP值
            if (caster is PlayerBattleController playerController)
            {
                playerController.TakeDamage(originalHp);
            }
            else if (caster is MonsterBattleController monsterController)
            {
                monsterController.TakeDamage(originalHp);
            }

            // 应用牺牲暴击Buff（这个buff会在下次切换精灵时触发，持续2回合）
            int buffId = 20011; // 牺牲暴击Buff ID
            
            buffSystem.ApplyBuff(caster, target, buffId, new Dictionary<string, object>
            {
                { "TargetType", BuffTargetType.Self },
                { "RemainingTurns", "2" },
                { "SwitchCount", 1 }, // 切换后生效
                { "Keep", true }
            });

            SeerGoLogger.Log($"<color=yellow>[牺牲暴击] {casterData.Basic.Name}牺牲自身，下一只精灵前2回合必定暴击</color>");
        }
    }
}
