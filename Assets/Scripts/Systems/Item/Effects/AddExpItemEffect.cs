using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Utilities;

namespace SeerGoGame.Systems
{
    /// <summary>
    /// 增加经验值道具效果
    /// </summary>
    public class AddExpItemEffect : IItemEffectHandler
    {
        public override bool CanHandle(string itemType)
        {
            return itemType == "exp";
        }

        public override bool ApplyEffect(ItemConfig itemConfig, PetData targetPet)
        {
            IExpConfigSystem expConfigSystem = GetArchitecture().GetSystem<IExpConfigSystem>();
            // 添加经验值
            if (expConfigSystem != null)
            {
                expConfigSystem.CalcExp(targetPet, (int)itemConfig.effectValue, null);
                SeerGoLogger.Log($"<color=cyan>[AddExpItemEffect] {targetPet.Basic.Name} 获得了 {itemConfig.effectValue} 经验值</color>");

                return true;
            }


            return false;
        }

        public override bool CanUse(ItemConfig itemConfig, PetData targetPet)
        {
            // 满级无法使用经验道具
            if (targetPet.Exp.Level >= 100)
            {
                return false;
            }
            return true;
        }

        public override string GetUseMessage(ItemConfig itemConfig, PetData targetPet)
        {
            return $"{targetPet.Basic.Name} 获得了 {itemConfig.effectValue} 经验值";
        }

        /// <summary>
        /// 检查并处理升级
        /// </summary>
        private bool CheckLevelUp(PetData targetPet)
        {
            bool leveledUp = false;
            
         
            return leveledUp;
        }
    }
}
