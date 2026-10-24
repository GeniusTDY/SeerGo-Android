using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Buffs
{
    /// <summary>
    /// 副作用ID=4/5: 属性变化效果 立即生效
    /// ID=4: 改变自身属性
    /// ID=5: 改变对手属性
    /// </summary>
    public class StatChangeSideEffect : ISkillSideEffectHandler
    {
        private int effectId;

        public override int EffectId => effectId;

        public StatChangeSideEffect(int id)
        {
            effectId = id;
        }

        public override void OnAfterDamageCalculation(IBuffable caster, IBuffable target, SkillData skill,
                           string[] args, EffectInfo effectInfo, BuffSystem buffSystem)
        {
            // 参数: args[0]=属性类型, args[1]=概率, args[2]=变化值
            if (args.Length < 3)
            {
                SeerGoLogger.LogError($"[属性变化] 参数不足,需要3个参数");
                return;
            }

            if (!int.TryParse(args[0], out int statType) ||
                !int.TryParse(args[1], out int probability) ||
                !int.TryParse(args[2], out int changeValue))
            {
                SeerGoLogger.LogError($"[属性变化] 参数解析失败: {string.Join(",", args)}");
                return;
            }

            // 确定目标 后面需要删除
            string targetName = "-1";
            if (effectId == 4)
            {
                if (caster is PlayerBattleController player)
                    targetName = player._currentPetData.Basic.Name;
                else if (caster is MonsterBattleController monster)
                    targetName = monster._currentMonsterData.Basic.Name;
            }
            else
            {
                if (target is PlayerBattleController player)
                    targetName = player._currentPetData.Basic.Name;
                else if (target is MonsterBattleController monster)
                    targetName = monster._currentMonsterData.Basic.Name;
            }

            // 概率判定
            bool success = (probability >= 100) || (Random.Range(1, 101) <= probability);

            if (!success)
            {
                SeerGoLogger.Log($"[属性变化] 概率判定失败 ({probability}%)");
                return;
            }

            // 根据属性类型和变化值确定buff ID
            int buffId = GetBuffIdForStatChange(statType, changeValue);

            if (buffId <= 0)
            {
                SeerGoLogger.LogError($"[属性变化] 未找到对应的Buff ID (属性类型:{statType}, 变化值:{changeValue})");
                return;
            }

            // 应用buff
            buffSystem.ApplyBuff(caster, target, buffId, new Dictionary<string, object>
            {
                { "TargetType", effectId == 4 ? BuffTargetType.Self : BuffTargetType.Enemy },
                { "EffectValue", (float)changeValue }
            });

        }

        /// <summary>
        /// 根据属性类型和变化值映射到buff ID
        /// </summary>
        private int GetBuffIdForStatChange(int statType, int changeValue)
        {
            // statType: 0=攻击,  1=防御, 2=特攻, 0=特防,4=速度, 5=命中
            // changeValue: -1=下降, +1=上升

            switch (statType)
            {
                case 0: return 10000; // 攻击
                case 1: return 10001; // 防御
                case 2: return 10002; // 特攻
                case 3: return 10003; // 特防
                case 4: return 10004; // 速度
                case 5: return 10005; // 命中
                case 6: return 10006; // 暴击率
            }
            return 0; // 未找到
        }
    }
}
