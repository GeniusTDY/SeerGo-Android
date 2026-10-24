// 事件定义
namespace SeerGoGame.Events
{
    public struct MapEffectsSwitchedEvent
    {
        public int NewMapId;
        public MapEffectsSwitchedEvent(int newMapId)
        {
            NewMapId = newMapId;
        }
    }
}