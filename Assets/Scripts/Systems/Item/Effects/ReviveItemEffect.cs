using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Utilities;

namespace SeerGoGame.Systems
{
    /// <summary>
    /// 复活道具效果
    /// </summary>
    public class ReviveItemEffect : IItemEffectHandler
    {
        public override bool CanHandle(string itemType)
        {
            return itemType == "revive";
        }

        public override bool ApplyEffect(ItemConfig itemConfig, PetData targetPet)
        {
            // 复活并恢复指定百分比的HP
            int revivePercent = (int)itemConfig.effectValue; // 如 50 表示恢复50%
            int hpToRestore = targetPet.CurrentStatus.MaxHp * revivePercent / 100;
            
            targetPet.CurrentStatus.Hp = hpToRestore;
            
            SeerGoLogger.Log($"<color=cyan>[ReviveItemEffect] {targetPet.Basic.Name} 已复活！恢复了 {hpToRestore} HP</color>");
            
            return true;
        }

        public override bool CanUse(ItemConfig itemConfig, PetData targetPet)
        {
            // 只能对已死亡的精灵使用
            return targetPet.CurrentStatus.Hp <= 0;
        }

        public override string GetUseMessage(ItemConfig itemConfig, PetData targetPet)
        {
            return $"{targetPet.Basic.Name} 已复活！";
        }
    }
}
