using QFramework;
using SeerGoGame.Events;

namespace SeerGoGame.Commands
{
    public class ChangeSkillReplaceCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            this.SendEvent<SkillReplaceOpenEvent>();
        }

    }
}