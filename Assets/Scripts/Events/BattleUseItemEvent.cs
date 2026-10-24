using SeerGoGame.Models;

namespace SeerGoGame.Events
{
    /// <summary>
    /// 战斗中使用道具事件
    /// </summary>
    public struct BattleUseItemEvent
    {
        public int ItemId;
        public int PetIndex;

        public BattleUseItemEvent(int itemId, int petIndex)
        {
            ItemId = itemId;
            PetIndex = petIndex;
        }
    }
}
