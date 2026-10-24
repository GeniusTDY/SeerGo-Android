namespace QFramework
{
    internal class StopVoiceCommand
    {
        internal static void Execute() => AudioManager.Instance.VoicePlayer.Stop();
    }
}