namespace QFramework
{
    public class DefaultAudioLoaderPool : AbstractAudioLoaderPool
    {
        protected override IAudioLoader CreateLoader() => new DefaultAudioLoader();
    }
}