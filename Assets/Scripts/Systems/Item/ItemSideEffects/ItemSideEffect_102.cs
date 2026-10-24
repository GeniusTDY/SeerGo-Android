using SeerGoGame.Interfaces;
using SeerGoGame.Models;

namespace SeerGoGame.Systems
{
    /// <summary>
    /// 道具效果 102: 恢复HP
    /// SideEffectArg: "恢复量" （-1 表示完全恢复）
    /// 注意：HP 实际修改由控制器完成（INBATTLE 走战斗流程含弹窗/药水反转，OUTBATTLE 走背包流程），
    ///       SideEffect 仅负责配置参数管理和使用条件校验。
    /// </summary>
    public class ItemSideEffect_102 : IItemSideEffectHandler
    {
        public override int EffectId => 102;
        public override int ArgsNum => 1;

        public override bool Apply(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            if (!int.TryParse(args[0], out int healAmount))
                return false;
            if (healAmount <= 0 && healAmount != -1)
                return false;
            // HP 修改由控制器处理，SideEffect 只做校验
            return true;
        }

        public override bool CanUse(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            // 只要精灵存活就可以使用（满血也可以用，恢复上限为最大HP）
            return targetPet.CurrentStatus.Hp > 0;
        }

        public override string GetMessage(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            if (int.TryParse(args[0], out int val) && val == -1)
                return $"{targetPet.Basic.Name} 的HP已完全恢复";
            return $"{targetPet.Basic.Name} 恢复了 {args[0]} HP";
        }
    }
}
