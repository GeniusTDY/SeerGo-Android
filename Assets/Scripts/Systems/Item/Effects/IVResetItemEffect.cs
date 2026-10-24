using SeerGoGame.Interfaces;
using SeerGoGame.Models;

namespace SeerGoGame.Systems
{
    /// <summary>
    /// 个体值修改道具效果
    /// </summary>
    public class IVResetItemEffect : IItemEffectHandler
    {
        public override bool CanHandle(string itemType)
        {
            return itemType == "resetIV";
        }

        public override bool ApplyEffect(ItemConfig itemConfig, PetData targetPet)
        {
            int randomNum = UnityEngine.Random.Range(0, 32);
            targetPet.Talent.IV = randomNum;

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
