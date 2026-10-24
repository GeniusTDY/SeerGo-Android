using QFramework;
using SeerGoGame.Events;
using SeerGoGame.Interfaces;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Models
{
    public class MapModel : AbstractModel, IMapModel
    {
        public MapData CurrentMap { get; private set; }
        public BindableProperty<int> CurrentMapId { get; } = new BindableProperty<int>();
        public Dictionary<int, MapData> MapCache { get; } = new Dictionary<int, MapData>();

        private IMapParser mapParser;

        protected override void OnInit()
        {
            mapParser = this.GetUtility<IMapParser>();
            // 监听地图ID变化,
            CurrentMapId.Register(newMapId =>
            {
                //   if (newMapId > 0)
                LoadMap(newMapId);
            });
        }

        public void LoadMap(int mapId)
        {

            if (MapCache.TryGetValue(mapId, out MapData cachedMap))
            {
                CurrentMap = cachedMap;

                CurrentMapId.Value = mapId;

                // 发送地图加载完成事件
                this.SendEvent(new MapLoadedEvent
                {
                    MapId = mapId,
                    MapName = CurrentMap.Name,
                    Success = true
                });
                return;
            }

            // 从XML加载地图数据
            string mapPath = ResourcePathBuilder.GetMapDataPath(mapId);
            CurrentMap = mapParser.LoadMapFromResources(mapPath);

            if (CurrentMap != null)
            {
                MapCache[mapId] = CurrentMap;
                CurrentMapId.Value = mapId;


                // 发送地图加载完成事件
                this.SendEvent(new MapLoadedEvent
                {
                    MapId = mapId,
                    MapName = CurrentMap.Name,
                    Success = true
                });
            }
            else
            {
                SeerGoLogger.LogError($"地图模型: 加载地图失败 {mapId}");

                // 发送加载失败事件
                this.SendEvent(new MapLoadedEvent
                {
                    MapId = mapId,
                    Success = false,
                    ErrorMessage = $"地图文件不存在: {mapPath}"
                });
            }
        }

        public void PrintMapInfoToConsole()
        {
            if (CurrentMap == null)
            {
                SeerGoLogger.LogWarning("地图模型: 当前没有加载地图数据");
                return;
            }
/*
            SeerGoLogger.Log("=== 地图详细信息 ===");
            SeerGoLogger.Log($"地图名称: {CurrentMap.Name}");
            SeerGoLogger.Log($"地图ID: {CurrentMap.Id}");
            SeerGoLogger.Log($"分类: {CurrentMap.Category}");
            SeerGoLogger.Log($"赛尔初始位置: {CurrentMap.initialPoint}");
            SeerGoLogger.Log($"实体数量: {CurrentMap.AllNpcs.Count}");

            foreach (var entity in CurrentMap.AllNpcs)
            {
                if (entity is MapNpc npc)
                {
                    SeerGoLogger.Log($"NPC: {npc.Id} - {npc.description}");
                }
            }

            foreach (var entity in CurrentMap.AllTeleports)
            {
                if (entity is MapTeleport teleport)
                {
                    SeerGoLogger.Log($"NPC: {teleport.Id} - {teleport.TargetMapId} - {teleport.Name}");
                }
            }*/
        }

        public Vector2 GetInitialPosition()
        {
            if (CurrentMap == null || string.IsNullOrEmpty(CurrentMap.initialPoint))
                return Vector2.zero;

            return ParseUtils.ParsePosition(CurrentMap.initialPoint);
        }
    }
}