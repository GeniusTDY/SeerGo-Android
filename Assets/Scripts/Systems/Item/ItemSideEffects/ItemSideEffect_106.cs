using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Utilities;

namespace SeerGoGame.Systems
{
    /// <summary>
    /// 道具效果 106: 复活
    /// SideEffectArg: "恢复HP百分比" （如 50 表示恢复50%HP）
    /// </summary>
    public class ItemSideEffect_106 : IItemSideEffectHandler
    {
        public override int EffectId => 106;
        public override int ArgsNum => 1;

        public override bool Apply(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            if (!int.TryParse(args[0], out int revivePercent) || revivePercent <= 0)
                return false;

            int hpToRestore = targetPet.CurrentStatus.MaxHp * revivePercent / 100;
            targetPet.CurrentStatus.Hp = hpToRestore;

            SeerGoLogger.Log($"<color=cyan>[ItemSideEffect_106] {targetPet.Basic.Name} 已复活！恢复了 {hpToRestore} HP</color>");
            return true;
        }

        public override bool CanUse(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            // 只能对已死亡的精灵使用
            return targetPet.CurrentStatus.Hp <= 0;
        }

        public override string GetMessage(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            return $"{targetPet.Basic.Name} 已复活！";
        }
    }
}
