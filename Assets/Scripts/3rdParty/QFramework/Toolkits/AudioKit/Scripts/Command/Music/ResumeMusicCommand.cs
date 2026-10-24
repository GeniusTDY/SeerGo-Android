namespace QFramework
{
    internal class ResumeMusicCommand
    {
        internal static void Execute() => AudioKit.MusicPlayer.Resume();
    }
}