using QFramework;
using SeerGoGame.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace SeerGoGame.Interfaces
{
    public interface ICreateSpriteEntitySystem : ISystem
    {
        void CreateSpriteAsync(MapNpc item, Transform parent, List<GameObject> resultList);
        Task UnloadAllSpritesAsync();
        void CreateTeleporterSprite(MapTeleport item, Transform parentTransform, List<GameObject> spritesList);
    }
}