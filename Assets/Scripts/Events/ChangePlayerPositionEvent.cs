using UnityEngine;

namespace SeerGoGame.Events
{
    // 赛尔位置变更请求事件（Controller → System）
    public struct PlayerPositionRequestEvent
    {
        public string Position;
    }

    // 赛尔位置变更结果事件（System → Controller）
    public struct PlayerPositionResultEvent
    {
        public string Position;
        public bool Success;
        public string Message;
        public string ErrorMessage;
    }

    // 地图初始化赛尔位置
    public struct InitPlayerPositionResultEvent
    {
        public string Position;
        public bool Success;
        public string Message;
        public string ErrorMessage;
    }


}