using System;
using System.Xml.Serialization;

namespace SeerGoGame.Models
{
    [Serializable]
    public class MusicData
    {
        [XmlAttribute("category")] public int Category;
        public string bgm;
    }
}