using QFramework;
using SeerGoGame.Models;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Interfaces
{
    /// <summary>
    /// XML数据系统接口
    /// </summary>
    public interface IWorldMapXmlSystem : ISystem
    {
        void LoadGalaxyData(TextAsset xmlAsset);
        List<GalaxyData> GetGalaxyList();
        GalaxyData GetGalaxyById(int galaxyId);
        List<PlanetData> GetPlanetsByGalaxyId(int galaxyId);
    }

}