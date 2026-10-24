using QFramework;
using SeerGoGame.Events;

namespace SeerGoGame.Commands
{
    public class ChangeSettingCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            // ·¢ËÍevent
            this.SendEvent<SettingOpenEvent>();
        }

    }
}