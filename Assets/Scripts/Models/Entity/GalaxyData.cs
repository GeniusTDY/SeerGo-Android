using SeerGoGame.Utilities;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 星系数据结构
/// </summary>
namespace SeerGoGame.Models
{
    [System.Serializable]
    public class GalaxyData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ResId { get; set; }
        public Vector2 Size { get; set; }
        public Vector2 Position { get; set; }
        public Color Color { get; set; }
        public List<PlanetData> Planets { get; set; } = new List<PlanetData>();
        // 使用方式
        public void PrintWithToString()
        {
            foreach (var planet in Planets)
            {
                SeerGoLogger.Log(planet.ToString()); 
            }
        }
    }
}
