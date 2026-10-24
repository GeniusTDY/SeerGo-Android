using QFramework;
using System.Threading.Tasks;
using UnityEngine;
namespace SeerGoGame.Interfaces
{

    public interface IMapEffectSystem : ISystem
    {
        Task LoadEffectsAsync(int mapId, Transform parent);
        Task UnloadEffectsAsync(int mapId);
    }

}