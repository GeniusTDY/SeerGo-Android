namespace QFramework
{
    public class AudioLoaderPoolModel : AbstractModel
    {
        private IAudioLoaderPool mAudioLoaderPool = new DefaultAudioLoaderPool();

        public IAudioLoaderPool AudioLoaderPool
        {
            get => mAudioLoaderPool;
            set
            {
                LogKit.I("RegisterAudioLoaderPool:" + value.GetType().Name);
                mAudioLoaderPool = value;
            }
        }

        protected override void OnInit()
        {
            LogKit.I("CurrentAudioLoaderPool:" + mAudioLoaderPool.GetType().Name);
        }
    }
}