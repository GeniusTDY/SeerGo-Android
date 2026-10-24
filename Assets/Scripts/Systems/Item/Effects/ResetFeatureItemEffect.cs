using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Systems
{
    /// <summary>
    /// 特性重组剂道具效果
    /// 从new_se.json中随机选取一个特性ID，赋值给精灵的FeatureId
    /// 仅当精灵已有特性（FeatureId != 0）时才可使用
    /// </summary>
    public class ResetFeatureItemEffect : IItemEffectHandler
    {
        public override bool CanHandle(string itemType)
        {
            return itemType == "resetFeature";
        }

        public override bool ApplyEffect(ItemConfig itemConfig, PetData targetPet)
        {
            // 获取所有特性ID列表
            List<int> allSeIds = NewSeManager.Instance.GetAllSeIds();
            if (allSeIds == null || allSeIds.Count == 0)
                return false;

            // 随机选取一个特性ID
            int randomIndex = Random.Range(0, allSeIds.Count);
            int newFeatureId = allSeIds[randomIndex];

            // 赋值
            targetPet.Feature.FeatureId = newFeatureId;

            return true;
        }

        public override bool CanUse(ItemConfig itemConfig, PetData targetPet)
        {
            // 仅当FeatureId不为0（精灵已有特性）时才可使用
            if (targetPet.Feature.FeatureId == 0)
                return false;

            // 还需确保有可用的特性配置
            return NewSeManager.Instance.GetAllSeIds().Count > 0;
        }

        public override string GetUseMessage(ItemConfig itemConfig, PetData targetPet)
        {
            return $"{targetPet.Basic.Name}的特性已重置";
        }
    }
}
