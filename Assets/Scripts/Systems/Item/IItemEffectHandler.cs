using QFramework;
using SeerGoGame.Models;

namespace SeerGoGame.Interfaces
{
    /// <summary>
    /// 道具效果处理器接口
    /// 每种道具类型实现自己的效果逻辑
    /// </summary>
    public abstract class IItemEffectHandler
    {
        /// <summary>
        /// 该处理器是否能处理指定类型的道具
        /// </summary>
        public abstract bool CanHandle(string itemType);

        /// <summary>
        /// 应用道具效果到目标精灵
        /// </summary>
        /// <param name="itemConfig">道具配置</param>
        /// <param name="targetPet">目标精灵</param>
        /// <param name="system">效果功能系统</param>
        /// <returns>是否成功应用效果</returns>
        public virtual bool ApplyEffect(ItemConfig itemConfig, PetData targetPet)
        {
            return ApplyEffect(itemConfig, targetPet);
        }

        /// <summary>
        /// 检查是否可以使用道具
        /// </summary>
        public virtual bool CanUse(ItemConfig itemConfig, PetData targetPet)
        {
            return true; // 默认可以使用
        }

        /// <summary>
        /// 获取道具使用后的提示信息
        /// </summary>
        public virtual string GetUseMessage(ItemConfig itemConfig, PetData targetPet)
        {
            return $"使用了{itemConfig.name}";
        }

        public virtual IArchitecture GetArchitecture()
        {
            return SeerGoArchitecture.Interface;
        }

    }
}
