using QFramework;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Systems
{
    /// <summary>
    /// 道具效果管理器
    /// 管理所有道具效果处理器，负责分发和执行道具效果
    /// 新版：路由到 ItemSideEffectManager 的 SideEffect 系统（参照技能副作用模式）
    /// 旧版：保留旧版 handler 注册作为兼容备用
    /// </summary>
    public class ItemEffectManager : AbstractSystem
    {
        private List<IItemEffectHandler> effectHandlers = new List<IItemEffectHandler>();
        private IItemModel itemModel;
        private ItemSideEffectManager sideEffectManager;

        protected override void OnInit()
        {
            itemModel = this.GetModel<IItemModel>();
            sideEffectManager = new ItemSideEffectManager(itemModel);
            RegisterDefaultHandlers();
        }

        /// <summary>
        /// 注册默认的道具效果处理器（旧版兼容）
        /// </summary>
        private void RegisterDefaultHandlers()
        {
            RegisterHandler(new AddExpItemEffect());
            RegisterHandler(new HealItemEffect());
            RegisterHandler(new CureItemEffect());
            RegisterHandler(new ReviveItemEffect());
            RegisterHandler(new BuffItemEffect());
            RegisterHandler(new CapsuleEffect());
            RegisterHandler(new EVResetItemEffect());
            RegisterHandler(new IVResetItemEffect());
            RegisterHandler(new PersonalityItemEffect());
            RegisterHandler(new AddIVItemEffect());
            RegisterHandler(new EVAddItemEffect());
            RegisterHandler(new EVStatResetItemEffect());
            RegisterHandler(new ResetFeatureItemEffect());
            RegisterHandler(new RewardBuffItemEffect());
        }

        /// <summary>
        /// 注册道具效果处理器
        /// </summary>
        public void RegisterHandler(IItemEffectHandler handler)
        {
            if (handler != null && !effectHandlers.Contains(handler))
            {
                effectHandlers.Add(handler);
            }
        }

        /// <summary>
        /// 使用道具（新版：优先走 SideEffect 系统）
        /// </summary>
        /// <param name="itemId">道具ID</param>
        /// <param name="targetPet">目标精灵</param>
        /// <returns>是否成功使用</returns>
        public bool UseItem(int itemId, PetData targetPet)
        {
            // 获取道具配置
            if (!itemModel.ItemConfigs.TryGetValue(itemId, out var itemConfig))
            {
                SeerGoLogger.LogError($"<color=red>[ItemEffectManager] 道具ID {itemId} 不存在</color>");
                return false;
            }

            // 新版优先：如果配置了 sideEffect 字段，走 SideEffect 系统
            if (!string.IsNullOrEmpty(itemConfig.sideEffect))
            {
                return UseItemViaSideEffect(itemConfig, targetPet);
            }

            // 旧版兼容：走 type 路由系统
            return UseItemViaLegacy(itemConfig, targetPet);
        }

        /// <summary>
        /// 通过新版 SideEffect 系统使用道具
        /// </summary>
        private bool UseItemViaSideEffect(ItemConfig itemConfig, PetData targetPet)
        {
            // 检查是否可以使用
            if (!sideEffectManager.CanUseItem(itemConfig, targetPet))
            {
                SeerGoLogger.LogWarning($"<color=yellow>[ItemEffectManager] 无法对精灵 {targetPet.Basic.Name} 使用 {itemConfig.name}</color>");
                return false;
            }

            // 应用所有副作用效果
            bool success = sideEffectManager.ApplyAllEffects(itemConfig, targetPet);

            if (success)
            {
                string message = sideEffectManager.GetUseMessage(itemConfig, targetPet);
                SeerGoLogger.Log($"<color=green>[ItemEffectManager] {message}</color>");
            }
            else
            {
                SeerGoLogger.Log($"<color=red>[ItemEffectManager] {itemConfig.name}使用失败!</color>");
            }

            return success;
        }

        /// <summary>
        /// 通过旧版 type 路由系统使用道具（向后兼容）
        /// </summary>
        private bool UseItemViaLegacy(ItemConfig itemConfig, PetData targetPet)
        {
            // 查找对应的效果处理器
            IItemEffectHandler handler = FindHandler(itemConfig.type);
            if (handler == null)
            {
                SeerGoLogger.LogError($"<color=red>[ItemEffectManager] 找不到类型 '{itemConfig.type}' 的效果处理器</color>");
                return false;
            }

            // 检查是否可以使用
            if (!handler.CanUse(itemConfig, targetPet))
            {
                SeerGoLogger.LogWarning($"<color=yellow>[ItemEffectManager] 无法对精灵 {targetPet.Basic.Name} 使用 {itemConfig.name}</color>");
                return false;
            }

            // 应用效果
            bool success = handler.ApplyEffect(itemConfig, targetPet);

            if (success)
            {
                string message = handler.GetUseMessage(itemConfig, targetPet);
                SeerGoLogger.Log($"<color=green>[ItemEffectManager] {message}</color>");
            }
            else
            {
                SeerGoLogger.Log($"<color=red>[ItemEffectManager] {itemConfig.name}使用失败!</color>");
            }

            return success;
        }

        /// <summary>
        /// 检查道具是否可以使用
        /// </summary>
        public bool CanUseItem(int itemId, PetData targetPet)
        {
            if (!itemModel.ItemConfigs.TryGetValue(itemId, out var itemConfig))
                return false;

            // 新版优先
            if (!string.IsNullOrEmpty(itemConfig.sideEffect))
            {
                return sideEffectManager.CanUseItem(itemConfig, targetPet);
            }

            // 旧版兼容
            IItemEffectHandler handler = FindHandler(itemConfig.type);
            if (handler == null) return false;
            return handler.CanUse(itemConfig, targetPet);
        }

        /// <summary>
        /// 查找对应类型的效果处理器（旧版兼容）
        /// </summary>
        private IItemEffectHandler FindHandler(string itemType)
        {
            foreach (var handler in effectHandlers)
            {
                if (handler.CanHandle(itemType))
                    return handler;
            }
            return null;
        }
    }
}

