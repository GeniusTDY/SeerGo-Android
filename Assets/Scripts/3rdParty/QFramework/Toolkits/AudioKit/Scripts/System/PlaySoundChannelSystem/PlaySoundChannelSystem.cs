using System.Collections.Generic;
using UnityEngine;

namespace QFramework
{
    internal class PlaySoundChannelSystem : AbstractSystem
    {
        internal AudioKit.PlaySoundModes DefaultPlaySoundMode = AudioKit.PlaySoundModes.EveryOne;

        internal readonly EveryOneChannel EveryOneChannel = new EveryOneChannel();

        internal readonly IgnoreSameSoundInSoundFramesChannel IgnoreInSoundFramesChannel =
            new IgnoreSameSoundInSoundFramesChannel();

        internal readonly IgnoreSameSoundInGlobalFramesChannel IgnoreInGlobalFramesChannel =
            new IgnoreSameSoundInGlobalFramesChannel();


        internal bool CanPlaySound(AudioPlayer player)
        {

            switch (player.PlaySoundMode)
            {
                case AudioKit.PlaySoundModes.EveryOne:
                    return EveryOneChannel.CanPlaySound(player.AudioName);
                case AudioKit.PlaySoundModes.IgnoreSameSoundInSoundFrames:
                    return IgnoreInSoundFramesChannel.CanPlaySound(player.AudioName);
                case AudioKit.PlaySoundModes.IgnoreSameSoundInGlobalFrames:
                    return IgnoreInGlobalFramesChannel.CanPlaySound(player.AudioName);
                default:
                    return true;
            }
        }

        internal void SoundFinish(AudioPlayer player)
        {
            switch (player.PlaySoundMode)
            {
                case AudioKit.PlaySoundModes.EveryOne:
                    EveryOneChannel.SoundFinish(player.AudioName);
                    break;
                case AudioKit.PlaySoundModes.IgnoreSameSoundInSoundFrames:
                    IgnoreInSoundFramesChannel.SoundFinish(player.AudioName);
                    break;
                case AudioKit.PlaySoundModes.IgnoreSameSoundInGlobalFrames:
                    IgnoreInGlobalFramesChannel.SoundFinish(player.AudioName);
                    break;
            }
        }

        protected override void OnInit()
        {
        }
    }
}