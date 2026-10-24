using QFramework;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Utilities;
using System.Linq;

namespace SeerGoGame.Systems
{
    /// <summary>
    /// 道具效果 105: 治疗全部异常状态
    /// SideEffectArg: 无参数
    /// </summary>
    public class ItemSideEffect_105 : IItemSideEffectHandler
    {
        public override int EffectId => 105;
        public override int ArgsNum => 0;

        public override bool Apply(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            SeerGoLogger.Log($"<color=cyan>[ItemSideEffect_105] {targetPet.Basic.Name} 的所有异常状态已解除</color>");
            return true;
        }

        public override bool CanUse(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            return targetPet.CurrentStatus.Hp > 0;
        }

        public override string GetMessage(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            return $"{targetPet.Basic.Name} 的所有异常状态已解除";
        }
    }
}
