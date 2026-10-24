using QFramework;

namespace SeerGoGame.Interfaces
{
    public interface IStorage : IUtility
    {
        void Save<T>(string key, T value);
        T Load<T>(string key);
        bool ContainsKey(string key);
        void Delete(string key);
    }
}