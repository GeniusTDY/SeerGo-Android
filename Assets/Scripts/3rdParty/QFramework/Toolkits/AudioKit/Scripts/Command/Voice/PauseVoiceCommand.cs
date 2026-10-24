namespace QFramework
{
    internal class PauseVoiceCommand
    {
        internal static void Execute() => AudioManager.Instance.VoicePlayer.Pause();
    }
}