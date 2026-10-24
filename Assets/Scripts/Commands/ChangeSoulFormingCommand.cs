using QFramework;
using SeerGoGame.Events;

namespace SeerGoGame.Commands
{
    public class ChangeSoulFormingCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            this.SendEvent<SoulFormingChangeEventOpenEvent>();
        }

    }
}