using UnityEngine;

namespace QFramework
{
    internal interface IClipPrepareMode
    {
        void PrepareClip(AbstractAudioPlayer audioPlayer, GameObject root, string name, bool loop);

        void UnPrepareClip();
    }
}