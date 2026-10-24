using QFramework;
using SeerGoGame.Events;

namespace SeerGoGame.Commands
{
    public class ChangeReplacePetCommand : AbstractCommand
    {
        private int _petIndex;
        public ChangeReplacePetCommand(int petIndex)
        {
           _petIndex = petIndex;
        }

        protected override void OnExecute()
        {
            // ·¢ËÍevent
            this.SendEvent(new PetReplaceOpenEvent(_petIndex));
        }

    }
}