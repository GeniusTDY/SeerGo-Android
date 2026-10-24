using System;
using System.Xml.Serialization;

namespace SeerGoGame.Models
{
    [Serializable]
    public class TeleportEntity
    {
        [XmlAttribute("id")] public string Id;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("pos")] public string Position;
        [XmlAttribute("targetMapId")] public string TargetMapId;
    }

    [Serializable]
    public class MapTeleport : TeleportEntity
    {
        public override string ToString()
        {
            return $"TeleportEntity[Id={Id}, Name={Name}, Pos={Position}]";
        }
    }
}