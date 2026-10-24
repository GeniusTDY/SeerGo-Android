using UnityEngine;

namespace SeerGoGame.Events
{
    // 地图加载请求事件（Controller → System）
    public struct MapLoadRequestEvent
    {
        public int TargetMapId;
        public Vector2 SpawnPosition;
    }

    // 地图加载完成事件（Model → 所有层）
    public struct MapLoadedEvent
    {
        public int MapId;
        public string MapName;
        public bool Success;
        public string ErrorMessage;
    }

    // NPC交互请求事件（Controller → System）
    public struct NpcClickRequestEvent
    {
        public string NpcId;
        public string ActionType;
        public string Parameters;
    }

    // NPC交互结果事件（System → Controller）
    public struct NpcClickResultEvent
    {
        public string NpcId;
        public bool Success;
        public string Message;
        public string ErrorMessage;
    }

    // 成就解锁事件（System → Controller）
    public struct AchievementUnlockedEvent
    {
        public string AchievementId;
        public string AchievementName;
    }
    public struct PetLoadedEvent
    {
      
    }
}