using System;
using UnityEngine;

namespace QFramework
{
    internal class PlayMusicWithClipCommand
    {
        internal static void Execute(AudioClip clip, bool loop = true, Action onBeganCallback = null,
            Action onEndCallback = null, float volume = 1f)
        {
            AudioManager.Instance.CheckAudioListener();
            var audioMgr = AudioManager.Instance;
            audioMgr.CurrentMusicName = "music" + clip.GetHashCode();

            Debug.Log(">>>>>> Start Play Music");
            AudioKit.MusicPlayer.VolumeScale(volume)
                .OnStart(onBeganCallback)
                .PrepareByClipAndPlay(audioMgr.gameObject, clip, audioMgr.CurrentMusicName, loop)
                .OnFinish(onEndCallback);
        }
    }
}