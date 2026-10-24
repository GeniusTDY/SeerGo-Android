using QFramework;
using SeerGoGame.Interfaces;
using SeerGoGame.Utilities;
using System.Collections.Generic;

namespace SeerGoGame.Models
{
    public class WorldMapModel : AbstractModel, IWorldMapModel
    {
        public List<GalaxyData> GalaxyList { get; private set; } = new List<GalaxyData>();
        public List<PlanetData> CurrentPlanetList { get; } = new List<PlanetData>();

        private IWorldMapXmlParser worldMapXmlParser;

        protected override void OnInit()
        {
            // 解析世界地图数据
            worldMapXmlParser = this.GetUtility<IWorldMapXmlParser>();
            string mapPath = ResourcePathBuilder.GetWorldMapDataPath();
            List<GalaxyData> galaxyXmlDatas = worldMapXmlParser.ParseGalaxiesFromFile(mapPath);
            GalaxyList = galaxyXmlDatas;
        }
    }
}