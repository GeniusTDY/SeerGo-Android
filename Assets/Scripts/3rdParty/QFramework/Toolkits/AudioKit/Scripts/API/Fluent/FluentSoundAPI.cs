using UnityEngine;

namespace QFramework
{
    public class FluentSoundAPI : IPoolable, IPoolType
    {
        private string mName = null;
        private AudioClip mClip = null;
        private bool mLoop = false;
        private float mVolumeScale = 1;
        private float mPitch = 1;
        private AudioKit.PlaySoundModes? mPlaySoundModes = null;

        public bool IsRecycled { get; set; }

        public static FluentSoundAPI Allocate() => SafeObjectPool<FluentSoundAPI>.Instance.Allocate();

        public void OnRecycled()
        {
            mName = null;
            mLoop = false;
        }

        public void Recycle2Cache() => SafeObjectPool<FluentSoundAPI>.Instance.Recycle(this);

        public FluentSoundAPI WithName(string name)
        {
            mName = name;
            return this;
        }

        public FluentSoundAPI WithAudioClip(AudioClip clip)
        {
            mClip = clip;
            return this;
        }

        public FluentSoundAPI Loop(bool loop)
        {
            mLoop = loop;
            return this;
        }

        public FluentSoundAPI VolumeScale(float volumeScale)
        {
            mVolumeScale = volumeScale;
            return this;
        }

        public FluentSoundAPI Pitch(float pitch)
        {
            mPitch = pitch;
            return this;
        }
        
        public FluentSoundAPI PlaySoundMode(AudioKit.PlaySoundModes playSoundModes)
        {
            mPlaySoundModes = playSoundModes;
            return this;
        }


        public AudioPlayer Play()
        {
            AudioPlayer soundPlayer = null;
            if (mName != null)
            {
                soundPlayer = AudioKit.PlaySound(mName, mLoop, callBack: (p) => { Recycle2Cache(); },
                    volume: mVolumeScale, pitch: mPitch,mPlaySoundModes);
            }
            else
            {
                soundPlayer = AudioKit.PlaySound(mClip, mLoop, callBack: (p) => { Recycle2Cache(); },
                    volume: mVolumeScale, pitch: mPitch,mPlaySoundModes);
            }

            return soundPlayer;
        }
    }
}