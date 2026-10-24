using SeerGoGame.Interfaces;
using SeerGoGame.Models;

namespace SeerGoGame.Systems
{
    /// <summary>
    /// 性格重塑胶囊道具效果
    /// </summary>
    public class PersonalityItemEffect : IItemEffectHandler
    {
        public override bool CanHandle(string itemType)
        {
            return itemType == "resetPersonality";
        }

        public override bool ApplyEffect(ItemConfig itemConfig, PetData targetPet)
        {
            int randomNum = UnityEngine.Random.Range(0, 25);
            targetPet.Basic.Personality = randomNum;
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
