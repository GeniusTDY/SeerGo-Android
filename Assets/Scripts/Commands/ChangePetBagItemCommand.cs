using QFramework;
using SeerGoGame.Events;

namespace SeerGoGame.Commands
{
    public class ChangePetBagItemCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            this.SendEvent<PetBagItemOpenEvent>();
        }

    }
}