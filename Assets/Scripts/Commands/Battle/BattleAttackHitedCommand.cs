using QFramework;
using SeerGoGame.Events;
using SeerGoGame.Models;

namespace SeerGoGame.Commands
{
    public class BattleAttackHitedCommand : AbstractCommand
    {
        private string _role;

        public BattleAttackHitedCommand(string role)
        {
            _role = role;
        }

        protected override void OnExecute()
        {
            if (_role == "Player")
            {
                this.SendEvent(new PlayerAttackHitEvent());
            }else if (_role == "Monster")
            {
                this.SendEvent(new MonsterAttackHitEvent());
            }
        }

    }
}