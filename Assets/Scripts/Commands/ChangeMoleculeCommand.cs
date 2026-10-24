using QFramework;
using SeerGoGame.Events;

namespace SeerGoGame.Commands
{
    public class ChangeMoleculeCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            this.SendEvent<MoleculeChangeEventOpenEvent>();
        }

    }
}