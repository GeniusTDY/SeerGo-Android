using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SeerGoGame.Systems
{
    /// <summary>
    /// 奖励翻倍Buff道具效果处理器
    /// 处理三类buff: 经验&学习力翻倍(rewardExpBuff)、赛尔豆翻倍(rewardSeerBeanBuff)、道具掉落率翻倍(rewardDropRateBuff)
    /// 同一buffType下只能存在一个buff:
    ///   - 相同倍率 → 叠加次数
    ///   - 不同倍率 → 替换（倍率和次数都替换）
    /// 实际倍率效果在BattleRewardService中应用
    /// </summary>
    public class RewardBuffItemEffect : IItemEffectHandler
    {
        private const int MaxRemainCount = 999;

        // 道具类型到存档buffType的映射
        private static readonly Dictionary<string, string> TypeToBuffType = new()
        {
            ["rewardExpBuff"] = "exp",
            ["rewardSeerBeanBuff"] = "seerBean",
            ["rewardDropRateBuff"] = "dropRate",
        };

        public override bool CanHandle(string itemType)
        {
            return TypeToBuffType.ContainsKey(itemType);
        }

        public override bool ApplyEffect(ItemConfig itemConfig, PetData targetPet)
        {
            if (!TypeToBuffType.TryGetValue(itemConfig.type, out string buffType))
            {
                SeerGoLogger.LogError($"<color=red>[RewardBuffItemEffect] 未知buff类型: {itemConfig.type}</color>");
                return false;
            }

            int multiplier = (int)itemConfig.effectValue;
            int useCount = int.Parse(itemConfig.effectArgs);

            var archiveModel = GetArchitecture().GetModel<IArchiveModel>();
            if (archiveModel == null)
            {
                SeerGoLogger.LogError("<color=red>[RewardBuffItemEffect] 无法获取存档模型</color>");
                return false;
            }

            // 查找当前buffType下是否已有激活的buff
            var existingBuff = archiveModel.CurrentData.RewardBuffs
                .FirstOrDefault(b => b.BuffType == buffType);

            if (existingBuff != null)
            {
                if (existingBuff.Multiplier == multiplier)
                {
                    // 相同倍率 → 叠加次数（上限999）
                    existingBuff.RemainCount = Mathf.Min(existingBuff.RemainCount + useCount, MaxRemainCount);
                    SeerGoLogger.Log($"<color=cyan>[RewardBuffItemEffect] 叠加次数: type={buffType}, multiplier={multiplier}x, 新增次数={useCount}, 当前剩余={existingBuff.RemainCount}</color>");
                }
                else
                {
                    // 不同倍率 → 替换
                    int oldMultiplier = existingBuff.Multiplier;
                    existingBuff.BuffId = itemConfig.id;
                    existingBuff.Multiplier = multiplier;
                    existingBuff.RemainCount = Mathf.Min(useCount, MaxRemainCount);
                    SeerGoLogger.Log($"<color=cyan>[RewardBuffItemEffect] 替换buff: type={buffType}, {oldMultiplier}x → {multiplier}x, 次数={useCount}</color>");
                }
            }
            else
            {
                archiveModel.CurrentData.RewardBuffs.Add(new RewardBuffData
                {
                    BuffId = itemConfig.id,
                    Multiplier = multiplier,
                    RemainCount = Mathf.Min(useCount, MaxRemainCount),
                    BuffType = buffType,
                });
                SeerGoLogger.Log($"<color=cyan>[RewardBuffItemEffect] 新增buff: type={buffType}, multiplier={multiplier}x, 次数={useCount}</color>");
            }

            return true;
        }

        public override bool CanUse(ItemConfig itemConfig, PetData targetPet)
        {
            return true;
        }

        public override string GetUseMessage(ItemConfig itemConfig, PetData targetPet)
        {
            if (!TypeToBuffType.TryGetValue(itemConfig.type, out string buffType))
                return $"使用了{itemConfig.name}";

            string buffTypeName = buffType switch
            {
                "exp" => "经验&学习力",
                "seerBean" => "赛尔豆",
                "dropRate" => "道具掉落率",
                _ => buffType,
            };

            int multiplier = (int)itemConfig.effectValue;
            return $"已激活{multiplier}倍{buffTypeName}增幅，剩余{itemConfig.effectArgs}次";
        }
    }
}
