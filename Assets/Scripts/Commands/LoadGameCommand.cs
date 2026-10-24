using QFramework;
using SeerGoGame.Interfaces;

// ¥Êµµ√¸¡Ó
namespace SeerGoGame.Commands
{
    public class LoadGameCommand : AbstractCommand
    {
        public string SaveName;

        public LoadGameCommand(string saveName)
        {
            SaveName = saveName;
        }
        protected override void OnExecute()
        {
            var archiveSystem = this.GetSystem<IArchiveSystem>();
            var data = archiveSystem.LoadGame(SaveName);
        }
    }
}