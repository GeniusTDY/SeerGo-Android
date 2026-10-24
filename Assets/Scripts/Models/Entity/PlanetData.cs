using UnityEngine;

/// <summary>
/// 行星数据结构
/// </summary>
namespace SeerGoGame.Models
{
    /// <summary>
    /// 行星数据结构
    /// </summary>
    [System.Serializable]
    public class PlanetData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ResId { get; set; }
        public Vector2 Size { get; set; }
        public Vector2 Position { get; set; }
        public Color Color { get; set; }

        public override string ToString()
        {
            return $"行星[{Name}]: {Id}";
        }

    
    }
}