using QFramework;
using SeerGoGame.Events;

namespace SeerGoGame.Commands
{
    public class RoundChangeCommand : AbstractCommand
    {
        private int _roundCount;
        private bool _isPlayerTurn;

        public RoundChangeCommand(int roundCount, bool IsPlayerTurn)
        {
            _roundCount = roundCount;
            _isPlayerTurn = IsPlayerTurn;
        }

        protected override void OnExecute()
        {
            this.SendEvent(new RoundStartEvent { RoundCount = _roundCount, IsPlayerTurn = _isPlayerTurn });
        }

    }
}