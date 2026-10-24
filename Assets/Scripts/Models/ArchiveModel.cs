using QFramework;
using SeerGoGame.Interfaces;

namespace SeerGoGame.Models
{
    public class ArchiveModel : AbstractModel, IArchiveModel
    {
        public ArchiveData CurrentData { get; set; }
        public string CurrentSaveName { get; set; } = "default";

        protected override void OnInit()
        {
            CurrentData = new ArchiveData();
        }
    }
}