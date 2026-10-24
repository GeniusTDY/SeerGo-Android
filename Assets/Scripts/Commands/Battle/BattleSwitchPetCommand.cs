using QFramework;
using SeerGoGame.Events;
using SeerGoGame.Models;

namespace SeerGoGame.Commands
{
    public class BattleSwitchPetCommand : AbstractCommand
    {
        private int _index;

        public BattleSwitchPetCommand(int index)
        {
            _index = index;
        }

        protected override void OnExecute()
        {
            this.SendEvent(new BattleSwitchPetEvent { Index = _index});
        }

    }
}