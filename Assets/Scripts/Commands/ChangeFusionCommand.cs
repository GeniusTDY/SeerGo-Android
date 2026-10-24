using QFramework;
using SeerGoGame.Events;

namespace SeerGoGame.Commands
{
    public class ChangeFusionCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            this.SendEvent<FusionChangeEventOpenEvent>();
        }

    }
}