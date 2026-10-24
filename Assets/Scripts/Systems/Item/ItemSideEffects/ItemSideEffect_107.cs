using QFramework;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Utilities;

namespace SeerGoGame.Systems
{
    /// <summary>
    /// 道具效果 107: 添加Buff
    /// SideEffectArg: "BuffID"
    /// </summary>
    public class ItemSideEffect_107 : IItemSideEffectHandler
    {
        public override int EffectId => 107;
        public override int ArgsNum => 1;

        public override bool Apply(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            if (int.TryParse(args[0], out int buffId))
            {
                SeerGoLogger.Log($"<color=cyan>[ItemSideEffect_107] {targetPet.Basic.Name} 获得了Buff (ID:{buffId})</color>");
                return true;
            }

            SeerGoLogger.LogError($"<color=red>[ItemSideEffect_107] 无效的Buff ID: {args[0]}</color>");
            return false;
        }

        public override bool CanUse(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            return targetPet.CurrentStatus.Hp > 0;
        }

        public override string GetMessage(ItemConfig itemConfig, PetData targetPet, string[] args)
        {
            return $"{targetPet.Basic.Name} 获得了增益效果";
        }
    }
}
