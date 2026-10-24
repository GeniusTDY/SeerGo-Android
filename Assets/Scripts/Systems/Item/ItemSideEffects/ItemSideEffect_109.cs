using SeerGoGame.Interfaces;
using SeerGoGame.Models;

namespace SeerGoGame.Systems
{
    /// <summary>
    /// 道具效果 109: 重置全部学习力（EV）
    /// SideEffectArg: 无参数
    /// </summary>
    public class ItemSideEffect_109 : IItemSideEffectHandler
    {
        public override int EffectId => 109;
        public override int ArgsNum => 0;

        public override bool Apply(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            IPetAttributeCalcSystem petAttributeCalcSystem = GetArchitecture().GetSystem<IPetAttributeCalcSystem>();
            if (petAttributeCalcSystem == null) return false;

            targetPet.Talent.EV.Def = 0;
            targetPet.Talent.EV.Spd = 0;
            targetPet.Talent.EV.SpAtk = 0;
            targetPet.Talent.EV.SpDef = 0;
            targetPet.Talent.EV.Atk = 0;
            targetPet.Talent.EV.Hp = 0;
            petAttributeCalcSystem.CalculateAllStats(targetPet);
            return true;
        }

        public override string GetMessage(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            return $"{targetPet.Basic.Name} 的全部学习力已重置";
        }
    }
}
