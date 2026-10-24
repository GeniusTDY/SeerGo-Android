namespace QFramework
{
    internal class ResumeVoiceCommand
    {
        internal static void Execute() => AudioManager.Instance.VoicePlayer.Resume();
    }
}