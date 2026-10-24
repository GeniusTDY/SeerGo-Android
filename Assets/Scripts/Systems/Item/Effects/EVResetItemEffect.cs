using SeerGoGame.Interfaces;
using SeerGoGame.Models;

namespace SeerGoGame.Systems
{
    /// <summary>
    /// 重置学习力道具效果
    /// </summary>
    public class EVResetItemEffect : IItemEffectHandler
    {
        public override bool CanHandle(string itemType)
        {
            return itemType == "resetEV";
        }

        public override bool ApplyEffect(ItemConfig itemConfig, PetData targetPet)
        {
            IPetAttributeCalcSystem petAttributeCalcSystem = GetArchitecture().GetSystem<IPetAttributeCalcSystem>();
            if (petAttributeCalcSystem != null)
            {
                targetPet.Talent.EV.Def = 0;
                targetPet.Talent.EV.Spd = 0;
                targetPet.Talent.EV.SpAtk = 0;
                targetPet.Talent.EV.SpDef = 0;
                targetPet.Talent.EV.Atk = 0;
                targetPet.Talent.EV.Hp = 0;
                petAttributeCalcSystem.CalculateAllStats(targetPet);

                return true;
            }

            return false;
        }

        public override bool CanUse(ItemConfig itemConfig, PetData targetPet)
        {
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
