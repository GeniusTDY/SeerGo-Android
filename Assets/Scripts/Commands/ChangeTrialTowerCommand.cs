using QFramework;
using SeerGoGame.Events;

namespace SeerGoGame.Commands
{
    public class ChangeTrialTowerCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            this.SendEvent<TrialTowerChangeEventOpenEvent>();
        }

    }
}