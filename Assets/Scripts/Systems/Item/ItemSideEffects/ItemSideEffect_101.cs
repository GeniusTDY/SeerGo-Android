using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Utilities;

namespace SeerGoGame.Systems
{
    /// <summary>
    /// 道具效果 101: 增加经验值
    /// SideEffectArg: "经验值"
    /// </summary>
    public class ItemSideEffect_101 : IItemSideEffectHandler
    {
        public override int EffectId => 101;
        public override int ArgsNum => 1;

        public override bool Apply(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            if (!int.TryParse(args[0], out int expValue) || expValue <= 0)
                return false;

            IExpConfigSystem expConfigSystem = GetArchitecture().GetSystem<IExpConfigSystem>();
            if (expConfigSystem != null)
            {
                expConfigSystem.CalcExp(targetPet, expValue, null);
                return true;
            }
            return false;
        }

        public override bool CanUse(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            // 满级无法使用经验道具
            return targetPet.Exp.Level < 100;
        }

        public override string GetMessage(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            return $"{targetPet.Basic.Name} 获得了 {args[0]} 经验值";
        }
    }
}
