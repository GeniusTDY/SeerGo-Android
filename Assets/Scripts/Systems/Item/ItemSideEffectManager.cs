using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SeerGoGame.Systems
{
    /// <summary>
    /// 道具副作用管理器 - 统一管理所有道具副作用处理器
    /// 参照 SkillSideEffectManager 设计，使用 EffectId 字典索引
    /// 支持一个道具配置多个副作用，参数按配置文件的 argsNum 自动切片
    /// 
    /// 参数如何确定：
    /// 1. 优先从 ItemEffectInfo.json 配置文件读取 argsNum（与技能 EffectInfo.json 同类同机制）
    /// 2. 配置缺失时 fallback 到处理器的 ArgsNum 属性
    /// 
    /// 示例：道具 sideEffect="102 103" sideEffectArg="20 5"
    ///   → 查配置：102(argsNum=1) 得 ["20"]，103(argsNum=1) 得 ["5"]
    /// </summary>
    public class ItemSideEffectManager
    {
        private Dictionary<int, IItemSideEffectHandler> handlers = new Dictionary<int, IItemSideEffectHandler>();
        private IItemModel itemModel;
        private bool isInitialized = false;

        public ItemSideEffectManager(IItemModel itemModel)
        {
            this.itemModel = itemModel;
        }

        /// <summary>
        /// 延迟初始化：首次使用时才注册所有处理器
        /// </summary>
        private void EnsureInitialized()
        {
            if (isInitialized) return;
            isInitialized = true;

            RegisterHandler(new ItemSideEffect_101());  // 增加经验
            RegisterHandler(new ItemSideEffect_102());  // 恢复HP
            RegisterHandler(new ItemSideEffect_103());  // 恢复PP
            RegisterHandler(new ItemSideEffect_104());  // 治疗指定异常状态
            RegisterHandler(new ItemSideEffect_105());  // 治疗全部异常状态
            RegisterHandler(new ItemSideEffect_106());  // 复活
            RegisterHandler(new ItemSideEffect_107());  // 添加Buff
            RegisterHandler(new ItemSideEffect_108());  // 胶囊捕捉
            RegisterHandler(new ItemSideEffect_109());  // 重置全部学习力
            RegisterHandler(new ItemSideEffect_110());  // 增加学习力
            RegisterHandler(new ItemSideEffect_111());  // 重置单项学习力
            RegisterHandler(new ItemSideEffect_112());  // 重置性格
            RegisterHandler(new ItemSideEffect_113());  // 重置个体值
            RegisterHandler(new ItemSideEffect_114());  // 增加个体值
            RegisterHandler(new ItemSideEffect_115());  // 重置特性
            RegisterHandler(new ItemSideEffect_116());  // 经验学习力翻倍Buff
            RegisterHandler(new ItemSideEffect_117());  // 赛尔豆翻倍Buff
            RegisterHandler(new ItemSideEffect_118());  // 掉落率翻倍Buff
        }

        /// <summary>
        /// 注册一个新的副作用处理器
        /// </summary>
        public void RegisterHandler(IItemSideEffectHandler handler)
        {
            if (handler != null && !handlers.ContainsKey(handler.EffectId))
            {
                handlers[handler.EffectId] = handler;
            }
        }

        /// <summary>
        /// 获取指定副作用需要的参数个数
        /// 优先从 ItemEffectInfo.json 读取，缺失时 fallback 到处理器
        /// </summary>
        private int GetArgsNum(int effectId, IItemSideEffectHandler handler)
        {
            if (itemModel.ItemEffectInfos != null &&
                itemModel.ItemEffectInfos.TryGetValue(effectId, out var effectInfo))
            {
                return effectInfo.argsNum;
            }
            // fallback: 处理器硬编码值
            return handler.ArgsNum;
        }

        /// <summary>
        /// 应用道具的所有副作用效果
        /// </summary>
        public bool ApplyAllEffects(ItemConfig itemConfig, PetData targetPet)
        {
            EnsureInitialized();

            var effectIds = itemConfig.GetSideEffectIds();
            if (effectIds.Count == 0)
            {
                SeerGoLogger.LogWarning($"<color=yellow>[ItemSideEffectManager] 道具 {itemConfig.name} 未配置副作用ID</color>");
                return false;
            }

            string argsStr = !string.IsNullOrEmpty(itemConfig.sideEffectArg)
                ? itemConfig.sideEffectArg
                : itemConfig.effectArgs;

            string[] allArgs = string.IsNullOrEmpty(argsStr)
                ? new string[0]
                : argsStr.Split(' ');

            int argIndex = 0;
            bool allSuccess = true;

            foreach (int effectId in effectIds)
            {
                if (!handlers.TryGetValue(effectId, out var handler))
                {
                    SeerGoLogger.LogError($"<color=red>[ItemSideEffectManager] 未找到道具副作用ID={effectId}的处理器</color>");
                    allSuccess = false;
                    continue;
                }

                // 从配置文件读取参数个数（失败时 fallback 到处理器）
                int argsNum = GetArgsNum(effectId, handler);
                string[] args = SliceArgs(allArgs, ref argIndex, argsNum);

                try
                {
                    bool success = handler.Apply(itemConfig, targetPet, args);
                    if (!success)
                    {
                        allSuccess = false;
                        SeerGoLogger.LogWarning($"<color=yellow>[ItemSideEffectManager] 副作用ID={effectId} 应用失败</color>");
                    }
                    else
                    {
                        string message = handler.GetMessage(itemConfig, targetPet, args);
                        SeerGoLogger.Log($"<color=cyan>[ItemSideEffectManager] {message}</color>");
                    }
                }
                catch (Exception ex)
                {
                    SeerGoLogger.LogError($"<color=red>[ItemSideEffectManager] 副作用ID={effectId} 执行异常: {ex.Message}</color>");
                    allSuccess = false;
                }
            }

            return allSuccess;
        }

        /// <summary>
        /// 检查道具是否可以对目标使用
        /// </summary>
        public bool CanUseItem(ItemConfig itemConfig, PetData targetPet)
        {
            EnsureInitialized();

            var effectIds = itemConfig.GetSideEffectIds();
            if (effectIds.Count == 0) return false;

            string argsStr = !string.IsNullOrEmpty(itemConfig.sideEffectArg)
                ? itemConfig.sideEffectArg
                : itemConfig.effectArgs;

            string[] allArgs = string.IsNullOrEmpty(argsStr)
                ? new string[0]
                : argsStr.Split(' ');

            int argIndex = 0;

            foreach (int effectId in effectIds)
            {
                if (!handlers.TryGetValue(effectId, out var handler))
                    return false;

                int argsNum = GetArgsNum(effectId, handler);
                string[] args = SliceArgs(allArgs, ref argIndex, argsNum);

                if (!handler.CanUse(itemConfig, targetPet, args))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 获取使用道具后的提示信息（取第一个副作用的消息）
        /// </summary>
        public string GetUseMessage(ItemConfig itemConfig, PetData targetPet)
        {
            EnsureInitialized();

            var effectIds = itemConfig.GetSideEffectIds();
            if (effectIds.Count == 0) return $"使用了{itemConfig.name}";

            string argsStr = !string.IsNullOrEmpty(itemConfig.sideEffectArg)
                ? itemConfig.sideEffectArg
                : itemConfig.effectArgs;

            string[] allArgs = string.IsNullOrEmpty(argsStr)
                ? new string[0]
                : argsStr.Split(' ');

            int argIndex = 0;

            if (handlers.TryGetValue(effectIds[0], out var handler))
            {
                int argsNum = GetArgsNum(effectIds[0], handler);
                string[] args = SliceArgs(allArgs, ref argIndex, argsNum);
                return handler.GetMessage(itemConfig, targetPet, args);
            }

            return $"使用了{itemConfig.name}";
        }

        /// <summary>
        /// 从参数数组中按指定数量切片，并推进索引
        /// </summary>
        private static string[] SliceArgs(string[] allArgs, ref int argIndex, int count)
        {
            string[] result = new string[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = (argIndex < allArgs.Length) ? allArgs[argIndex] : "0";
                argIndex++;
            }
            return result;
        }
    }
}
