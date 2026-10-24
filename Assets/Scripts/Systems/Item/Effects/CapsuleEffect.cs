using SeerGoGame.Interfaces;
using SeerGoGame.Models;

public class CapsuleEffect : IItemEffectHandler
{
    public override bool CanHandle(string itemType)
    {
        return itemType == "capsule";
    }

    public override bool ApplyEffect(ItemConfig itemConfig, PetData targetPet)
    {
       return true;
    }

}
