using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using UnityEngine;

namespace SeerGoGame.Systems
{
    /// <summary>
    /// 道具效果 114: 增加个体值
    /// SideEffectArg: "增加值" （如 "1" 或 "5"）
    /// 个体值上限31
    /// </summary>
    public class ItemSideEffect_114 : IItemSideEffectHandler
    {
        public override int EffectId => 114;
        public override int ArgsNum => 1;

        public override bool Apply(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            if (!int.TryParse(args[0], out int addValue) || addValue <= 0) return false;

            targetPet.Talent.IV = Mathf.Clamp(targetPet.Talent.IV + addValue, 0, 31);

            IPetAttributeCalcSystem petAttributeCalcSystem = GetArchitecture().GetSystem<IPetAttributeCalcSystem>();
            petAttributeCalcSystem?.CalculateAllStats(targetPet);

            return true;
        }

        public override bool CanUse(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            return targetPet.Talent.IV < 31;
        }

        public override string GetMessage(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            return $"{targetPet.Basic.Name}的个体值+{args[0]}（当前{targetPet.Talent.IV}）";
        }
    }
}
