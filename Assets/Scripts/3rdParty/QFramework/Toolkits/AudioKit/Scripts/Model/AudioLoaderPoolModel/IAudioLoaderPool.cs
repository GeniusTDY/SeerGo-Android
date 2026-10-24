namespace QFramework
{
    public interface IAudioLoaderPool
    {
        IAudioLoader AllocateLoader();
        void RecycleLoader(IAudioLoader loader);
    }
}