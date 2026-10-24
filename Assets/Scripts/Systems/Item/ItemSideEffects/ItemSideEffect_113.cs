using SeerGoGame.Interfaces;
using SeerGoGame.Models;

namespace SeerGoGame.Systems
{
    /// <summary>
    /// 道具效果 113: 重置个体值（随机0-31）
    /// SideEffectArg: 无参数
    /// </summary>
    public class ItemSideEffect_113 : IItemSideEffectHandler
    {
        public override int EffectId => 113;
        public override int ArgsNum => 0;

        public override bool Apply(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            int randomNum = UnityEngine.Random.Range(0, 32);
            targetPet.Talent.IV = randomNum;

            IPetAttributeCalcSystem petAttributeCalcSystem = GetArchitecture().GetSystem<IPetAttributeCalcSystem>();
            petAttributeCalcSystem?.CalculateAllStats(targetPet);

            return true;
        }

        public override string GetMessage(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            return $"{targetPet.Basic.Name}的个体值已重置为{targetPet.Talent.IV}";
        }
    }
}
