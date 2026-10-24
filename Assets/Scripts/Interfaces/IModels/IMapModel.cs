using System.Collections.Generic;
using QFramework;
using SeerGoGame.Models;
using UnityEngine;

namespace SeerGoGame.Interfaces
{
    public interface IMapModel : IModel
    {
        MapData CurrentMap { get; }
        BindableProperty<int> CurrentMapId { get; }
        Dictionary<int, MapData> MapCache { get; }
        void LoadMap(int mapId);
        void PrintMapInfoToConsole();
        Vector2 GetInitialPosition();
    }

}