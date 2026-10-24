using QFramework;
using SeerGoGame.Events;

namespace SeerGoGame.Commands
{
    public class ChangeShopCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            // ·¢ËÍevent
            this.SendEvent<ShopOpenEvent>();
        }

    }
}