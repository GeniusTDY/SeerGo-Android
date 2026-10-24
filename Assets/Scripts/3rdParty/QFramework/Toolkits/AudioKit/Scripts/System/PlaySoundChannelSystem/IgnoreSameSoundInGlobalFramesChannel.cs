using System.Collections.Generic;
using UnityEngine;

namespace QFramework
{
    internal class IgnoreSameSoundInGlobalFramesChannel : PlaySoundChannel
    {
        private readonly Dictionary<string, int> mSoundFrameCountForName = new Dictionary<string, int>();
        private int mGlobalFrameCount;
        internal int GlobalFrameCountForIgnoreSameSound = 10;

        internal override bool CanPlaySound(string soundName)
        {
            if (Time.frameCount - mGlobalFrameCount <= GlobalFrameCountForIgnoreSameSound)
            {
                if (mSoundFrameCountForName.ContainsKey(soundName))
                {
                    return false;
                }

                mSoundFrameCountForName.Add(soundName, 0);
            }
            else
            {
                mGlobalFrameCount = Time.frameCount;
                mSoundFrameCountForName.Clear();
                mSoundFrameCountForName.Add(soundName, 0);
            }

            return true;
        }

        internal override void SoundFinish(string soundName)
        {
        }
    }
}