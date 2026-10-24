using QFramework;

namespace SeerGoGame.Interfaces
{
    public interface IArchiveSystem : ISystem
    {
        bool SaveGame(string saveName, ArchiveData data);
        ArchiveData LoadGame(string saveName);
        string[] GetAllSaves();
        void DeleteSave(string saveName);
        string GetSavePath(string saveName);
    }
}
