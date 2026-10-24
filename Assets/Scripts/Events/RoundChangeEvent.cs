namespace SeerGoGame.Events
{
    public class RoundCompleteEvent
    {
        // 空的回合完成事件，只用作信号
    }

    public class RoundUpdateEvent
    {
        public int RoundCount { get; set; }
    }

    public class RoundStartEvent
    {
        public int RoundCount { get; set; }
        public bool IsPlayerTurn { get; set; }
    }
}