using QFramework;
using SeerGoGame.Interfaces;

namespace SeerGoGame.Commands
{
    public class StartGameCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            // œ‘ æµ«¬ºΩÁ√Ê
            this.GetSystem<IUISystem>().OpenUI("Login_Bg");
        }

    }
}