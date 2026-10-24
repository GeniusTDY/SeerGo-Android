namespace QFramework
{
    internal class PauseMusicCommand
    {
        internal static void Execute() => AudioKit.MusicPlayer.Pause();
    }
}