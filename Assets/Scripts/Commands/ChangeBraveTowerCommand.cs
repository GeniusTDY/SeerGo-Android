using QFramework;
using SeerGoGame.Events;

namespace SeerGoGame.Commands
{
    public class ChangeBraveTowerCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            this.SendEvent<BraveTowerChangeEventOpenEvent>();
        }

    }
}