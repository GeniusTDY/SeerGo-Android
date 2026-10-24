using System;

namespace QFramework
{
    internal class PlayVoiceCommand
    {
        internal static void Execute(string voiceName, bool loop = false, Action onBeganCallback = null,
            Action onEndedCallback = null)
        {
            var audioMgr = AudioManager.Instance;
            AudioManager.Instance.CheckAudioListener();
            audioMgr.CurrentVoiceName = voiceName;

            if (!AudioKit.Settings.IsVoiceOn.Value)
            {
                return;
            }
            
            AudioKit.VoicePlayer.OnStart(onBeganCallback);
            AudioKit.VoicePlayer.PrepareByNameAsyncAndPlay(AudioManager.Instance.gameObject, voiceName, loop);
            AudioKit.VoicePlayer.OnFinish(onEndedCallback);
        }
        
    }
}