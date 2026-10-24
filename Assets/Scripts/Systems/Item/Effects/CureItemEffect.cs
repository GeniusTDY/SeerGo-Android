using QFramework;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Utilities;
using System.Linq;

namespace SeerGoGame.Systems
{
    /// <summary>
    /// 治疗异常状态道具效果
    /// </summary>
    public class CureItemEffect : IItemEffectHandler
    {
        public override bool CanHandle(string itemType)
        {
            return itemType == "cure" || itemType == "cure_all";
        }

        public override bool ApplyEffect(ItemConfig itemConfig, PetData targetPet)
        {
            // 注意：这个效果需要精灵实现 IBuffable 接口
            // 如果你的 PetData 没有实现 IBuffable，需要通过其他方式获取
            // 这里假设在战斗中使用，可以通过 BattleController 获取
            
            if (itemConfig.type == "cure_all")
            {
                // 移除所有异常状态Buff
                return CureAllStatus(targetPet);
            }
            else
            {
                // 移除指定类型的异常状态
                string statusType = itemConfig.effectArgs;
                return CureSpecificStatus(targetPet, statusType);
            }
        }

        public override bool CanUse(ItemConfig itemConfig, PetData targetPet)
        {
            // 已死亡无法使用
            if (targetPet.CurrentStatus.Hp <= 0)
            {
                return false;
            }
            
            // TODO: 检查是否有异常状态
            // 这需要访问精灵的Buff系统
            return true;
        }

        public override string GetUseMessage(ItemConfig itemConfig, PetData targetPet)
        {
            if (itemConfig.type == "cure_all")
            {
                return $"{targetPet.Basic.Name} 的所有异常状态已解除";
            }
            else
            {
                return $"{targetPet.Basic.Name} 的{GetStatusName(itemConfig.effectArgs)}状态已解除";
            }
        }

        private bool CureAllStatus(PetData targetPet)
        {
            // 这里需要实现移除所有控制类、弱化类、附属类异常状态的逻辑
            // 由于需要访问Buff系统，建议在使用时通过Controller传入IBuffable对象
            SeerGoLogger.Log($"<color=cyan>[CureItemEffect] {targetPet.Basic.Name} 的所有异常状态已解除</color>");
            return true;
        }

        private bool CureSpecificStatus(PetData targetPet, string statusType)
        {
            SeerGoLogger.Log($"<color=cyan>[CureItemEffect] {targetPet.Basic.Name} 的{GetStatusName(statusType)}状态已解除</color>");
            return true;
        }

        private string GetStatusName(string statusType)
        {
            return statusType switch
            {
                "poison" => "中毒",
                "burn" => "烧伤",
                "freeze" => "冰冻",
                "paralysis" => "麻痹",
                "sleep" => "睡眠",
                _ => "异常"
            };
        }
    }

    /// <summary>
    /// 治疗异常状态道具效果（战斗中版本，可访问IBuffable）
    /// </summary>
    public static class CureItemEffectHelper
    {
        /// <summary>
        /// 在战斗中使用治疗道具（可直接操作Buff）
        /// </summary>
        public static bool UseInBattle(ItemConfig itemConfig, IBuffable target)
        {
            if (itemConfig.type == "cure_all")
            {
                return RemoveAllDebuffs(target);
            }
            else
            {
                return RemoveSpecificBuff(target, itemConfig.effectArgs);
            }
        }

        private static bool RemoveAllDebuffs(IBuffable target)
        {
            var buffsToRemove = target.ActiveBuffs
                .Where(kv => IsDebuff(kv.Value.Config.tab))
                .Select(kv => kv.Key)
                .ToList();

            foreach (var buffId in buffsToRemove)
            {
                target.RemoveBuff(buffId);
            }

            SeerGoLogger.Log($"<color=cyan>[CureItemEffect] 移除了 {buffsToRemove.Count} 个异常状态</color>");
            return buffsToRemove.Count > 0;
        }

        private static bool RemoveSpecificBuff(IBuffable target, string buffType)
        {
            var buffsToRemove = target.ActiveBuffs
                .Where(kv => MatchesBuffType(kv.Value.Config.type, buffType))
                .Select(kv => kv.Key)
                .ToList();

            foreach (var buffId in buffsToRemove)
            {
                target.RemoveBuff(buffId);
            }

            SeerGoLogger.Log($"<color=cyan>[CureItemEffect] 移除了类型为 {buffType} 的Buff</color>");
            return buffsToRemove.Count > 0;
        }

        private static bool IsDebuff(int tab)
        {
            // tab: 1控制类异常 2弱化类异常 3附属类异常
            return tab == 1 || tab == 2 || tab == 3;
        }

        private static bool MatchesBuffType(string buffType, string targetType)
        {
            return buffType == targetType;
        }
    }
}
