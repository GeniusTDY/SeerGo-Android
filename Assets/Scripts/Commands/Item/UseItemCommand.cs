using QFramework;
using SeerGoGame.Events;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Systems;
using SeerGoGame.Utilities;

namespace SeerGoGame.Commands
{
    /// <summary>
    /// 使用道具命令（非战斗场景）
    /// </summary>
    public class UseItemCommand : AbstractCommand
    {
        private int itemId;
        private int petIndex;
        private bool isInBattle;

        public UseItemCommand(int itemId, int petIndex, bool isInBattle = false)
        {
            this.itemId = itemId;
            this.petIndex = petIndex;
            this.isInBattle = isInBattle;
        }

        protected override void OnExecute()
        {
            var archiveModel = this.GetModel<IArchiveModel>();
            var itemModel = this.GetModel<IItemModel>();
            var itemEffectManager = this.GetSystem<ItemEffectManager>();

            // 检查道具是否存在
            if (!itemModel.ItemConfigs.TryGetValue(itemId, out var itemConfig))
            {
                SeerGoLogger.LogError($"<color=red>[UseItemCommand] 道具ID {itemId} 不存在</color>");
                this.SendEvent(new UseItemFailedEvent(itemId, "道具不存在"));
                return;
            }

            // 检查背包中是否有该道具
            var itemData = archiveModel.CurrentData.ItemStorage.Find(item => item.Id == itemId);
            if (itemData == null || itemData.Num <= 0)
            {
                SeerGoLogger.LogError($"<color=red>[UseItemCommand] 背包中没有道具 {itemConfig.name}</color>");
                this.SendEvent(new UseItemFailedEvent(itemId, "背包中没有该道具"));
                return;
            }

            // 检查使用场景是否合法
            if (isInBattle && !itemModel.IsContainScope(itemConfig, "INBATTLE"))
            {
                SeerGoLogger.LogError($"<color=red>[UseItemCommand] 道具 {itemConfig.name} 无法在战斗中使用</color>");
                this.SendEvent(new UseItemFailedEvent(itemId, "无法在战斗中使用"));
                return;
            }

            if (!isInBattle && !itemModel.IsContainScope(itemConfig, "OUTBATTLE"))
            {
                SeerGoLogger.LogError($"<color=red>[UseItemCommand] 道具 {itemConfig.name} 只能在战斗中使用</color>");
                this.SendEvent(new UseItemFailedEvent(itemId, "只能在战斗中使用"));
                return;
            }

            // 获取目标精灵
            if (petIndex < 0 || petIndex >= archiveModel.CurrentData.PetBag.Count)
            {
                SeerGoLogger.LogError($"<color=red>[UseItemCommand] 精灵索引 {petIndex} 无效</color>");
                this.SendEvent(new UseItemFailedEvent(itemId, "精灵索引无效"));
                return;
            }

            var targetPet = archiveModel.CurrentData.PetBag[petIndex];

            // 使用道具
            bool success = itemEffectManager.UseItem(itemId, targetPet);

            if (success)
            {
                // 减少道具数量
                itemData.Num--;
                
                // 如果数量为0，从背包中移除
                if (itemData.Num <= 0)
                {
                    archiveModel.CurrentData.ItemStorage.Remove(itemData);
                }

                SeerGoLogger.Log($"<color=green>[UseItemCommand] 成功使用 {itemConfig.name}，剩余 {itemData.Num} 个</color>");
                string useSence = isInBattle ? "INBATTLE" : "OUTBATTLE";
                // 发送使用成功事件
                this.SendEvent(new UseItemSuccessEvent(itemId, petIndex, targetPet, useSence));
            }
            else
            {
                SeerGoLogger.LogWarning($"<color=yellow>[UseItemCommand] 使用 {itemConfig.name} 失败</color>");
                this.SendEvent(new UseItemFailedEvent(itemId, "使用失败"));
            }
        }
    }
    
    /// <summary>
    /// 战斗中使用道具命令
    /// </summary>
    public class BattleUseItemCommand : AbstractCommand
    {
        private int itemId;
        private int petIndex;

        public BattleUseItemCommand(int itemId, int petIndex)
        {
            this.itemId = itemId;
            this.petIndex = petIndex;
        }

        protected override void OnExecute()
        {
            // 发送战斗使用道具事件
            this.SendEvent(new BattleUseItemEvent(itemId, petIndex));
        }
    }

    /// <summary>
    /// 使用道具成功事件
    /// </summary>
    public struct UseItemSuccessEvent
    {
        public int ItemId;
        public int PetIndex;
        public PetData TargetPet;
        public string UseSence;

        public UseItemSuccessEvent(int itemId, int petIndex, PetData targetPet, string useSence)
        {
            ItemId = itemId;
            PetIndex = petIndex;
            TargetPet = targetPet;
            UseSence = useSence;
        }
    }

    /// <summary>
    /// 使用道具失败事件
    /// </summary>
    public struct UseItemFailedEvent
    {
        public int ItemId;
        public string Reason;

        public UseItemFailedEvent(int itemId, string reason)
        {
            ItemId = itemId;
            Reason = reason;
        }
    }
}
