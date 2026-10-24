using QFramework;
using SeerGoGame.Models;

namespace SeerGoGame.Interfaces
{
    /// <summary>
    /// 道具副作用处理器接口（参照技能 ISkillSideEffectHandler 设计）
    /// 每个道具效果通过 EffectId 唯一标识，参数个数由 ItemEffectInfo.json 配置决定
    /// ArgsNum 仅在配置缺失时作为 fallback 使用
    /// </summary>
    public abstract class IItemSideEffectHandler
    {
        /// <summary>
        /// 该处理器对应的副作用ID（唯一标识）
        /// </summary>
        public abstract int EffectId { get; }

        /// <summary>
        /// 该副作用需要的参数个数（fallback，优先从 ItemEffectInfo.json 配置读取）
        /// </summary>
        public virtual int ArgsNum => 0;

        /// <summary>
        /// 应用道具效果到目标精灵
        /// </summary>
        /// <param name="itemConfig">道具配置</param>
        /// <param name="targetPet">目标精灵</param>
        /// <param name="args">已解析的参数数组（长度等于 ArgsNum）</param>
        /// <returns>是否成功应用效果</returns>
        public abstract bool Apply(ItemConfig itemConfig, PetData targetPet, string[] args);

        /// <summary>
        /// 检查是否可以使用道具
        /// </summary>
        public virtual bool CanUse(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            return true;
        }

        /// <summary>
        /// 获取道具使用后的提示信息
        /// </summary>
        public virtual string GetMessage(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            return $"使用了{itemConfig.name}";
        }

        /// <summary>
        /// 获取QFramework架构接口
        /// </summary>
        public virtual IArchitecture GetArchitecture()
        {
            return SeerGoArchitecture.Interface;
        }
    }
}
