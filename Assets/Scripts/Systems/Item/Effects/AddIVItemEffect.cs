using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using UnityEngine;

namespace SeerGoGame.Systems
{
    /// <summary>
    /// 个体值修改道具效果
    /// </summary>
    public class AddIVItemEffect : IItemEffectHandler
    {
        public override bool CanHandle(string itemType)
        {
            return itemType == "addIV";
        }

        public override bool ApplyEffect(ItemConfig itemConfig, PetData targetPet)
        {

            targetPet.Talent.IV = Mathf.Clamp(targetPet.Talent.IV + (int)itemConfig.effectValue, 0, 31);

            // 重新计算属性
            IPetAttributeCalcSystem petAttributeCalcSystem = GetArchitecture().GetSystem<IPetAttributeCalcSystem>();

            petAttributeCalcSystem.CalculateAllStats(targetPet);

            return true;
        }

        public override bool CanUse(ItemConfig itemConfig, PetData targetPet)
        {
            return true;
        }

        public override string GetUseMessage(ItemConfig itemConfig, PetData targetPet)
        {
            return $"{targetPet.Basic.Name}";
        }
    }
}
