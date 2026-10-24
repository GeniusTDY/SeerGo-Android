namespace QFramework
{
    internal class EveryOneChannel :PlaySoundChannel
    {
        internal override bool CanPlaySound(string soundName) => true;

        internal override void SoundFinish(string soundName)
        {
        }
    }
}