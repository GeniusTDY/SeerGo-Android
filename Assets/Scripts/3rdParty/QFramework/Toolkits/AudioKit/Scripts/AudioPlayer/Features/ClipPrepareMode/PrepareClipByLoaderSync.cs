using UnityEngine;

namespace QFramework
{
    internal class PrepareClipByLoaderSync : IClipPrepareMode
    {
        private IAudioLoader mLoader;

        public void PrepareClip(AbstractAudioPlayer audioPlayer, GameObject root, string name, bool loop)
        {
            audioPlayer.AudioSourceProxy.CreateOrUpdateAudioSource(root, name);

            //防止卸载后立马加载的情况
            var preLoader = mLoader;

            mLoader = null;
            
            audioPlayer.ClearDataAndStop();
            
            mLoader = AudioKit.Config.AudioLoaderPool.AllocateLoader();

            audioPlayer.AudioName = name;
            audioPlayer.AudioSourceProxy.SetLoop(loop);
            var keys = AudioSearchKeys.Allocate();
            keys.AssetName = name;
            var clip = mLoader.LoadClip(keys);
            audioPlayer.OnClipPrepareFinished(true, clip);
            keys.Recycle2Cache();

            if (preLoader != null)
            {
                preLoader.Unload();
                AudioKit.Config.AudioLoaderPool.RecycleLoader(preLoader);
            }
        }

        public void UnPrepareClip()
        {
            if (mLoader != null)
            {
                mLoader.Unload();
                AudioKit.Config.AudioLoaderPool.RecycleLoader(mLoader);
            }

            mLoader = null;
        }
    }
}