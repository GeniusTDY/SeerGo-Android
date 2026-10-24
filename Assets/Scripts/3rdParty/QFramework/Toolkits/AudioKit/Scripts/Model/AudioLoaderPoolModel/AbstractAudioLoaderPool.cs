using System.Collections.Generic;

namespace QFramework
{
    public abstract class AbstractAudioLoaderPool : IAudioLoaderPool
    {
        private readonly Stack<IAudioLoader> mPool = new Stack<IAudioLoader>(16);

        public IAudioLoader AllocateLoader() => mPool.Count > 0 ? mPool.Pop() : CreateLoader();

        protected abstract IAudioLoader CreateLoader();

        public void RecycleLoader(IAudioLoader loader) => mPool.Push(loader);
    }
}