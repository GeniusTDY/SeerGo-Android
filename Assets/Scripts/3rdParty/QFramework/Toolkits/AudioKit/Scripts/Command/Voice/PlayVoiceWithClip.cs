using System;
using UnityEngine;

namespace QFramework
{
    public class PlayVoiceWithClip
    {
        public static void Execute(AudioClip clip, bool loop = false, Action onBeganCallback = null,
            Action onEndedCallback = null, float volumeScale = 1.0f)
        {
            AudioManager.Instance.CheckAudioListener();
            var audioMgr = AudioManager.Instance;

            audioMgr.CurrentVoiceName = "voice" + clip.GetHashCode();

            if (!AudioKit.Settings.IsVoiceOn.Value)
            {
                return;
            }

            AudioKit.VoicePlayer.VolumeScale(volumeScale)
                .OnStart(onBeganCallback);
            AudioKit.VoicePlayer.PrepareByClipAndPlay(AudioManager.Instance.gameObject, clip, audioMgr.CurrentVoiceName, loop);
            AudioKit.VoicePlayer.OnFinish(onEndedCallback);
        }
    }
}