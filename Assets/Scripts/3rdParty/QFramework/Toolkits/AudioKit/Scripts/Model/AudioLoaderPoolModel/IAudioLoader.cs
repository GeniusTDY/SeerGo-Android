using System;
using UnityEngine;

namespace QFramework
{
    public interface IAudioLoader
    {
        AudioClip Clip { get; }
        AudioClip LoadClip(AudioSearchKeys audioSearchKeys);

        void LoadClipAsync(AudioSearchKeys audioSearchKeys, Action<bool, AudioClip> onLoad);
        void Unload();
    }
}