
namespace SeerGoGame.Events
{
    public struct SkillReplaceMainOpenEvent
    {
        public PetData petData;
        public int petIndex;

        public SkillReplaceMainOpenEvent(PetData targetPet, int petIndex)
        {
            petData = targetPet;
            this.petIndex = petIndex;
        }
    }


    public struct SkillReplaceCloseMainEvent
    {

    }
}
