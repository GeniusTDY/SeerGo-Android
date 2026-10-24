using QFramework;
using SeerGoGame.Events;

namespace SeerGoGame.Commands
{
    /// <summary>
    /// 玩家精灵被击败命令
    /// </summary>
    public class PlayerPetDefeatedCommand : AbstractCommand
    {
        private readonly int _petIndex;

        public PlayerPetDefeatedCommand(int petIndex = 0)
        {
            _petIndex = petIndex;
        }

        protected override void OnExecute()
        {
            this.SendEvent(new PlayerPetDefeatedEvent(_petIndex));
        }
    }

    /// <summary>
    /// 敌方精灵被击败命令
    /// </summary>
    public class MonsterPetDefeatedCommand : AbstractCommand
    {
        private readonly int _petIndex;

        public MonsterPetDefeatedCommand(int petIndex = 0)
        {
            _petIndex = petIndex;
        }

        protected override void OnExecute()
        {
            this.SendEvent(new MonsterPetDefeatedEvent(_petIndex));
        }
    }
}
