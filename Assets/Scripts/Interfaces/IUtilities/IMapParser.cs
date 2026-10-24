using QFramework;
using SeerGoGame.Models;
using UnityEngine;

namespace SeerGoGame.Interfaces
{
    public interface IMapParser : IUtility
    {
        MapData ParseMapXml(string xmlContent);
        MapData LoadMapFromResources(string mapPath);
        Sprite LoadSpriteByResId(string resId);
    }
}
