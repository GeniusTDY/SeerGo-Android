using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Utilities;
using System.Linq;
using UnityEngine;

namespace SeerGoGame.Systems
{
    /// <summary>
    /// 道具效果 116: 经验&学习力翻倍Buff
    /// SideEffectArg: "倍率 次数" （如 "2 100"）
    /// </summary>
    public class ItemSideEffect_116 : IItemSideEffectHandler
    {
        public override int EffectId => 116;
        public override int ArgsNum => 2;

        private const int MaxRemainCount = 999;
        private const string BuffType = "exp";

        public override bool Apply(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            if (!int.TryParse(args[0], out int multiplier) || multiplier <= 0) return false;
            if (!int.TryParse(args[1], out int useCount) || useCount <= 0) return false;

            return ApplyOrUpdateRewardBuff(itemConfig, multiplier, useCount);
        }

        public override string GetMessage(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            return $"已激活{args[0]}倍经验&学习力增幅，剩余{args[1]}次";
        }

        private bool ApplyOrUpdateRewardBuff(ItemConfig itemConfig, int multiplier, int useCount)
        {
            var archiveModel = GetArchitecture().GetModel<IArchiveModel>();
            if (archiveModel == null) return false;

            var existingBuff = archiveModel.CurrentData.RewardBuffs
                .FirstOrDefault(b => b.BuffType == BuffType);

            if (existingBuff != null)
            {
                if (existingBuff.Multiplier == multiplier)
                    existingBuff.RemainCount = Mathf.Min(existingBuff.RemainCount + useCount, MaxRemainCount);
                else
                {
                    existingBuff.BuffId = itemConfig.id;
                    existingBuff.Multiplier = multiplier;
                    existingBuff.RemainCount = Mathf.Min(useCount, MaxRemainCount);
                }
            }
            else
            {
                archiveModel.CurrentData.RewardBuffs.Add(new RewardBuffData
                {
                    BuffId = itemConfig.id,
                    Multiplier = multiplier,
                    RemainCount = Mathf.Min(useCount, MaxRemainCount),
                    BuffType = BuffType,
                });
            }

            return true;
        }
    }
}
