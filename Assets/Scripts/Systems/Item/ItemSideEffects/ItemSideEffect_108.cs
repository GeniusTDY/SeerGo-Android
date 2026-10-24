using SeerGoGame.Interfaces;
using SeerGoGame.Models;

namespace SeerGoGame.Systems
{
    /// <summary>
    /// 道具效果 108: 胶囊捕捉
    /// SideEffectArg: "捕捉倍率"
    /// </summary>
    public class ItemSideEffect_108 : IItemSideEffectHandler
    {
        public override int EffectId => 108;
        public override int ArgsNum => 1;

        public override bool Apply(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            // 捕捉逻辑在 BattleManageController 中处理
            // 这里只做标记，实际由战斗控制器接管
            return true;
        }

        public override string GetMessage(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            return $"向 {targetPet.Basic.Name} 投掷了{itemConfig.name}";
        }
    }
}
