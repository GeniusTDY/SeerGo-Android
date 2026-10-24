using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Systems
{
    /// <summary>
    /// 道具效果 115: 重置特性（随机选择）
    /// SideEffectArg: 无参数
    /// 仅当精灵已有特性（FeatureId != 0）时才可使用
    /// </summary>
    public class ItemSideEffect_115 : IItemSideEffectHandler
    {
        public override int EffectId => 115;
        public override int ArgsNum => 0;

        public override bool Apply(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            List<int> allSeIds = NewSeManager.Instance.GetAllSeIds();
            if (allSeIds == null || allSeIds.Count == 0) return false;

            int randomIndex = Random.Range(0, allSeIds.Count);
            targetPet.Feature.FeatureId = allSeIds[randomIndex];

            return true;
        }

        public override bool CanUse(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            if (targetPet.Feature.FeatureId == 0) return false;
            return NewSeManager.Instance.GetAllSeIds().Count > 0;
        }

        public override string GetMessage(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            return $"{targetPet.Basic.Name}的特性已重置";
        }
    }
}
