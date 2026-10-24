using System;
using UnityEngine;

namespace QFramework
{
    internal class PlayMusicCommand
    {
        internal static void Execute(string musicName, bool loop = true, Action onBeganCallback = null,
            Action onEndCallback = null, float volume = 1f)
        {
            AudioManager.Instance.CheckAudioListener();
            var audioMgr = AudioManager.Instance;
            audioMgr.CurrentMusicName = musicName;

            Debug.Log(">>>>>> Start Play Music");

            AudioKit.MusicPlayer.VolumeScale(volume)
                .OnStart(onBeganCallback)
                .PrepareByNameAsyncAndPlay(audioMgr.gameObject, musicName, loop)
                .OnFinish(onEndCallback);
        }
    }
}