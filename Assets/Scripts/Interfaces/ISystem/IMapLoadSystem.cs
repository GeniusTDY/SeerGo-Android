using QFramework;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Interfaces
{
    public interface IMapLoadSystem : ISystem
    {
        void RequestMapLoad(int mapId);
        void PrintCurrentMapInfo();
        void HandleNpcClick(string npcId, string action, string parameters, Transform transform = null, List<GameObject> resultList = null);
    }
}