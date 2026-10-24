using QFramework;
using SeerGoGame.Events;

namespace SeerGoGame.Commands
{
    public class ChangeSkillReplaceMainCommand : AbstractCommand
    {
        private PetData _petData;
        public int _petIndex;
        public ChangeSkillReplaceMainCommand(PetData petData, int petIndex = -1)
        {
            _petData = petData;
            _petIndex = petIndex;
        }
        protected override void OnExecute()
        {
            this.SendEvent(new SkillReplaceMainOpenEvent(_petData,_petIndex));
        }

    }
}