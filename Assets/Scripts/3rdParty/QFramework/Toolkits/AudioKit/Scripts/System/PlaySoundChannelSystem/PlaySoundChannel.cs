using System.Collections.Generic;
using UnityEngine;

namespace QFramework
{
    internal abstract class PlaySoundChannel
    {


        
        internal abstract bool CanPlaySound(string soundName);
        internal abstract void SoundFinish(string soundName);
    }
}