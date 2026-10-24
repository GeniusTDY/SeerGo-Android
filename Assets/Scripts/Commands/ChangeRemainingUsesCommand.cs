using QFramework;
using SeerGoGame.Events;

namespace SeerGoGame.Commands
{
    public class ChangeRemainingUsesCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            this.SendEvent<RemainingUsesChangeEventOpenEvent>();
        }

    }
}