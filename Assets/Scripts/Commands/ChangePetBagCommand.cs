using QFramework;
using SeerGoGame.Events;

namespace SeerGoGame.Commands
{
    public class ChangePetBagCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            // ·¢ËÍµÇÂ¼³É¹¦event
            this.SendEvent<PetBagOpenEvent>();
        }

    }
}