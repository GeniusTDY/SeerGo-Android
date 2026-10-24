using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace SeerGoGame.Models
{
    [XmlRoot("map")]
    public class MapData
    {
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("category")] public int Category;
        [XmlAttribute("id")] public int Id;
        [XmlAttribute("weather")] public int Weather;

        public string fightMapColor;
        public string initialPoint;

        public MusicData music;

        [XmlArray("entities")]
        [XmlArrayItem("npc", typeof(MapNpc))]
        [XmlArrayItem("teleport", typeof(MapTeleport))]
        public List<object> MapEntities = new List<object>();

        public override string ToString()
        {
            return $"Map[Id={Id}, Name={Name}, Category={Category}, Entities={MapEntities.Count}]";
        }
        // 专门获取所有NPC的只读属性
        [XmlIgnore] // 此特性确保该属性不会被XML序列化器处理
        public List<MapNpc> AllNpcs
        {
            get
            {
                // 使用 OfType<MapNpc>() 方法从混合列表中安全地筛选出所有MapNpc对象
                return MapEntities.OfType<MapNpc>().ToList();
            }
        }

        // 专门获取所有传送门的只读属性
        [XmlIgnore]
        public List<MapTeleport> AllTeleports
        {
            get
            {
                return MapEntities.OfType<MapTeleport>().ToList();
            }
        }
    }


}