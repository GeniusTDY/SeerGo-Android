using System.Collections.Generic;
using UnityEngine;

namespace QFramework
{
    internal class IgnoreSameSoundInSoundFramesChannel : PlaySoundChannel
    {
        private readonly Dictionary<string, int> mSoundFrameCountForName = new Dictionary<string, int>();

        internal int SoundFrameCountForIgnoreSameSound = 10;

        internal override bool CanPlaySound(string soundName)
        {
            if (mSoundFrameCountForName.TryGetValue(soundName, out var frames))
            {
                if (Time.frameCount - frames <= SoundFrameCountForIgnoreSameSound)
                {
                    return false;
                }

                mSoundFrameCountForName[soundName] = Time.frameCount;
            }
            else
            {
                mSoundFrameCountForName.Add(soundName, Time.frameCount);
            }

            return true;
        }

        internal override void SoundFinish(string soundName)
        {
            mSoundFrameCountForName.Remove(soundName);
        }
    }
}