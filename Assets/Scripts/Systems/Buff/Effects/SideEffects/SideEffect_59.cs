using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=59: 遗愿 - 消耗自身全部体力(体力降到0)，使下一只出战精灵的{0}和{1}能力提升1个等级
    /// </summary>
    public class SideEffect_59 : ISkillSideEffectHandler
    {
        public override int EffectId => 59;

        public override void OnAfterCastSkill(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            if (args.Length < 2 || !int.TryParse(args[0], out int stat1) || !int.TryParse(args[1], out int stat2))
            {
                SeerGoLogger.LogError($"[遗愿] 参数解析失败");
                return;
            }
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
                playerController.SetPlayerHp(originalHp, casterData);
            }
            else if (caster is MonsterBattleController monsterController)
            {
                monsterController.SetMonsterHp(originalHp, casterData);
            }

            int buffId1 = GetBuffIdForStatChange(stat1);
            int buffId2 = GetBuffIdForStatChange(stat2);

            buffSystem.ApplyBuff(caster, target, buffId1, new Dictionary<string, object>
            {
                { "TargetType", BuffTargetType.Self },
                { "RemainingTurns", "-1" },
                { "EffectValue", 1f },
                { "SwitchCount", 1 }, // 切换后触发
                { "Keep", true },
                { "Args", args } // 保存参数，传递给新精灵
            });
            buffSystem.ApplyBuff(caster, target, buffId2, new Dictionary<string, object>
            {
                { "TargetType", BuffTargetType.Self },
                { "RemainingTurns", "-1" },
                { "EffectValue", 1f },
                { "SwitchCount", 1 }, // 切换后触发
                { "Keep", true },
                { "Args", args } // 保存参数，传递给新精灵
            });

            SeerGoLogger.Log($"<color=yellow>[遗愿] {casterData.Basic.Name}牺牲自身，下一只精灵能力提升</color>");
        }

        /// <summary>
        /// 根据属性类型和变化值映射到buff ID
        /// </summary>
        private int GetBuffIdForStatChange(int statType)
        {
            // statType: 0=攻击,  1=防御, 2=特攻, 0=特防,4=速度, 5=命中

            switch (statType)
            {
                case 0: return 10000; // 攻击
                case 1: return 10001; // 防御
                case 2: return 10002; // 特攻
                case 3: return 10003; // 特防
                case 4: return 10004; // 速度
                case 5: return 10005; // 命中
            }
            return 0; // 未找到
        }
    }
}
