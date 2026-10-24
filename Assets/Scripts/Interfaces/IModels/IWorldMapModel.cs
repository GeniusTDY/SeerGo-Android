using QFramework;
using SeerGoGame.Models;
using System.Collections.Generic;

namespace SeerGoGame.Interfaces
{
    public interface IWorldMapModel : IModel
    {
        List<GalaxyData> GalaxyList { get; }
        List<PlanetData> CurrentPlanetList { get; }
    }
}