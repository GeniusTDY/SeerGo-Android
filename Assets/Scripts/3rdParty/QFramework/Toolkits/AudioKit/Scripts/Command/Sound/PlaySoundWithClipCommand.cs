using System;
using UnityEngine;

namespace QFramework
{
    internal class PlaySoundWithClipCommand
    {
        internal static AudioPlayer Execute(AudioClip clip, bool loop = false, Action<AudioPlayer> callBack = null,
            float volume = 1.0f, float pitch = 1,AudioKit.PlaySoundModes? playSoundModes = null)
        {
            AudioManager.Instance.CheckAudioListener();

            var soundName = clip.name;
            if (soundName.IsTrimNotNullAndEmpty())
            {
                soundName = "AudioClip:" + clip.GetHashCode();
            }

            var soundPlayer = AudioPlayer.Allocate(AudioKit.Settings.SoundVolume);
            soundPlayer.SetPlaySoundMode(playSoundModes);
            soundPlayer.OnFinish(() => callBack?.Invoke(soundPlayer));
            
            soundPlayer.VolumeScale(volume)
                .PrepareByClipAndPlay(AudioManager.Instance.gameObject, clip, soundName, loop)
                .Pitch(pitch);

 

            return soundPlayer;
        }
    }
}