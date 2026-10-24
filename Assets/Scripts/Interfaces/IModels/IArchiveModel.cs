// Model/ArchiveModel.cs
using QFramework;
namespace SeerGoGame.Interfaces
{
    public interface IArchiveModel : IModel
    {
        ArchiveData CurrentData { get; set; }
        string CurrentSaveName { get; set; }
    }
}