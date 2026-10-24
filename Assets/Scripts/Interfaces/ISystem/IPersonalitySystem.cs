using QFramework;
using SeerGoGame.Models;

namespace SeerGoGame.Interfaces
{
    public interface IPersonalitySystem : ISystem
    {
        PersonalityData GetPersonalityById(int id);
    }
}