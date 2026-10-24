using QFramework;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Utilities;
using System.Linq;

namespace SeerGoGame.Systems
{
    /// <summary>
    /// 道具效果 104: 治疗指定异常状态
    /// SideEffectArg: "异常类型" （如 poison/burn/freeze/paralysis/sleep）
    /// </summary>
    public class ItemSideEffect_104 : IItemSideEffectHandler
    {
        public override int EffectId => 104;
        public override int ArgsNum => 1;

        public override bool Apply(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            string statusType = args[0];
            SeerGoLogger.Log($"<color=cyan>[ItemSideEffect_104] {targetPet.Basic.Name} 的{GetStatusName(statusType)}状态已解除</color>");
            return true;
        }

        public override bool CanUse(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            return targetPet.CurrentStatus.Hp > 0;
        }

        public override string GetMessage(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            return $"{targetPet.Basic.Name} 的{GetStatusName(args[0])}状态已解除";
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
}
