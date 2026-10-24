using System;
using UnityEngine;

namespace QFramework
{
    public class DefaultAudioLoader : IAudioLoader
    {
        public AudioClip Clip { get; private set; }

        public AudioClip LoadClip(AudioSearchKeys panelSearchKeys)
        {
            Clip = Resources.Load<AudioClip>(panelSearchKeys.AssetName);
            return Clip;
        }

        public void LoadClipAsync(AudioSearchKeys audioSearchKeys, Action<bool, AudioClip> onLoad)
        {
            var resourceRequest = Resources.LoadAsync<AudioClip>(audioSearchKeys.AssetName);
            resourceRequest.completed += operation =>
            {
                var clip = resourceRequest.asset as AudioClip;
                onLoad(clip, clip);
            };
        }

        public void Unload()
        {
            Resources.UnloadAsset(Clip);
            Clip = null;
        }
    }
}