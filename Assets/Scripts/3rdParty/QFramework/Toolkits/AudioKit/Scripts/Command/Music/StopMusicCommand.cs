namespace QFramework
{
    internal class StopMusicCommand
    {
        internal static void Execute() => AudioKit.MusicPlayer.Stop();
    }
}