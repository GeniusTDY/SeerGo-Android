namespace QFramework
{
    internal class StopAllSoundCommand
    {
        internal static void Execute()
        {
            AudioKitArchitecture.PlayingSoundPoolModel.ForEachAllSound(player=>player.Stop());
            AudioKitArchitecture.PlayingSoundPoolModel.ClearAllPlayingSound();
        }
    }
}