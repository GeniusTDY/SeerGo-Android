// Command/SaveArchiveDataCommand.cs
using QFramework;
using SeerGoGame.Interfaces;
using SeerGoGame.Utilities;

namespace SeerGoGame.Commands
{
    public class SaveArchiveDataCommand : AbstractCommand
    {
        private ArchiveData _archiveData;

        private IArchiveSystem archiveSystem;

        public SaveArchiveDataCommand(ArchiveData archiveData)
        {
            _archiveData = archiveData;
        }

        protected override void OnExecute()
        {
            // 存档
            archiveSystem = this.GetSystem<IArchiveSystem>();
            archiveSystem.SaveGame("autosave0",_archiveData);
        }
    }

}