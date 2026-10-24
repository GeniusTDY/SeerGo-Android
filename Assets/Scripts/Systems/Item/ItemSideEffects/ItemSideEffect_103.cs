using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using UnityEngine;

namespace SeerGoGame.Systems
{
    /// <summary>
    /// 道具效果 103: 恢复PP
    /// SideEffectArg: "恢复量"
    /// </summary>
    public class ItemSideEffect_103 : IItemSideEffectHandler
    {
        public override int EffectId => 103;
        public override int ArgsNum => 1;

        public override bool Apply(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            if (!int.TryParse(args[0], out int ppAmount) || ppAmount <= 0)
                return false;

            targetPet.Skills.NormalSkill.ForEach(skill =>
            {
                skill.currentPP = Mathf.Min(skill.currentPP + ppAmount, skill.maxPP);
            });

            return true;
        }

        public override bool CanUse(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            // PP未满任意技能可用
            return targetPet.CurrentStatus.Hp > 0;
        }

        public override string GetMessage(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            return $"{targetPet.Basic.Name} 的技能PP恢复了 {args[0]} 点";
        }
    }
}
