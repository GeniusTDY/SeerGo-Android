using QFramework;
using SeerGoGame.Events;

namespace SeerGoGame.Commands
{
    public class RoundCompleteCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            this.SendEvent(new RoundCompleteEvent());
        }
    }
}

