using QFramework;
using System.Collections.Generic;

namespace SeerGoGame.Interfaces
{
    public interface IPetAttributeCalcSystem : ISystem
    {
        List<int> CalculateAllStats(PetData petData);

    }
}