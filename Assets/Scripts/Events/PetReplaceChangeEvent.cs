
namespace SeerGoGame.Events
{
    public struct PetReplaceOpenEvent
    {
        public int petIndex;
        public PetReplaceOpenEvent(int petIndex)
        {
            this.petIndex = petIndex;
        }
    }

    public struct PetReplaceCloseEvent
    {

    }
}
