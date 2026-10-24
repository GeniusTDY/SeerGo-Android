using QFramework;
using SeerGoGame.Events;

namespace SeerGoGame.Commands
{
    public class ChangePlayerBagCommand : AbstractCommand
    {
        protected override void OnExecute()
        {

            this.SendEvent<PlayerBagOpenEvent>();
        }

    }
}