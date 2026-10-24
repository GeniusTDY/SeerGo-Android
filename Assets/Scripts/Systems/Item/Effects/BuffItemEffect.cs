using QFramework;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Utilities;

namespace SeerGoGame.Systems
{
    /// <summary>
    /// 添加Buff道具效果
    /// </summary>
    public class BuffItemEffect : IItemEffectHandler
    {
        public override bool CanHandle(string itemType)
        {
            return itemType == "buff";
        }

        public override bool ApplyEffect(ItemConfig itemConfig, PetData targetPet)
        {
            // effectArgs 中存储的是 Buff ID
            if (int.TryParse(itemConfig.effectArgs, out int buffId))
            {
                SeerGoLogger.Log($"<color=cyan>[BuffItemEffect] {targetPet.Basic.Name} 获得了Buff (ID:{buffId})</color>");
                
                // 注意：这里只是记录，实际应用Buff需要在战斗系统中处理
                // 因为需要 IBuffable 接口
                return true;
            }
            
            SeerGoLogger.LogError($"<color=red>[BuffItemEffect] 无效的Buff ID: {itemConfig.effectArgs}</color>");
            return false;
        }

        public override bool CanUse(ItemConfig itemConfig, PetData targetPet)
        {
            // 已死亡无法使用
            if (targetPet.CurrentStatus.Hp <= 0)
            {
                return false;
            }
            
            return true;
        }

        public override string GetUseMessage(ItemConfig itemConfig, PetData targetPet)
        {
            return $"{targetPet.Basic.Name} 获得了增益效果";
        }
    }

    /// <summary>
    /// Buff道具效果辅助类（战斗中版本）
    /// </summary>
    public static class BuffItemEffectHelper
    {
        /// <summary>
        /// 在战斗中使用Buff道具（可直接操作Buff系统）
        /// </summary>
        public static bool UseInBattle(ItemConfig itemConfig, IBuffable target, IArchitecture architecture)
        {
            if (!int.TryParse(itemConfig.effectArgs, out int buffId))
            {
                return false;
            }

            var buffSystem = architecture.GetSystem<BuffSystem>();
            
            // 应用Buff到目标（caster和target都是自己）
            buffSystem.ApplyBuff(target, target, buffId);
            
            SeerGoLogger.Log($"<color=green>[BuffItemEffect] 成功应用Buff (ID:{buffId})</color>");
            return true;
        }
    }
}
