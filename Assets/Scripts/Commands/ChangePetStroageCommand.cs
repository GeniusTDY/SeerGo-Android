using QFramework;
using SeerGoGame.Events;

namespace SeerGoGame.Commands
{
    public class ChangePetStroageCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            this.SendEvent<PetStroageOpenEvent>();
        }

    }
}