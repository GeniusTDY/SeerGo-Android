namespace SeerGoGame.Events
{
    /// <summary>
    /// 玩家精灵被击败事件
    /// </summary>
    public class PlayerPetDefeatedEvent
    {
        public int PetIndex;

        public PlayerPetDefeatedEvent(int petIndex)
        {
            PetIndex = petIndex;
        }
    }

    /// <summary>
    /// 敌方精灵被击败事件
    /// </summary>
    public class MonsterPetDefeatedEvent
    {
        public int PetIndex;

        public MonsterPetDefeatedEvent(int petIndex)
        {
            PetIndex = petIndex;
        }
    }
}
