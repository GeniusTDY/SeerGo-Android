using SeerGoGame.Interfaces;
using SeerGoGame.Models;

namespace SeerGoGame.Systems
{
    /// <summary>
    /// 道具效果 112: 重置性格（随机）
    /// SideEffectArg: 无参数
    /// </summary>
    public class ItemSideEffect_112 : IItemSideEffectHandler
    {
        public override int EffectId => 112;
        public override int ArgsNum => 0;

        public override bool Apply(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            int randomNum = UnityEngine.Random.Range(0, 25);
            targetPet.Basic.Personality = randomNum;

            IPetAttributeCalcSystem petAttributeCalcSystem = GetArchitecture().GetSystem<IPetAttributeCalcSystem>();
            petAttributeCalcSystem?.CalculateAllStats(targetPet);

            return true;
        }

        public override string GetMessage(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            return $"{targetPet.Basic.Name}的性格已重置";
        }
    }
}
