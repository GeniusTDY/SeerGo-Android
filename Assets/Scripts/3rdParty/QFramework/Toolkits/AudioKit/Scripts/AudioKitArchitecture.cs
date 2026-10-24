using UnityEngine;

namespace QFramework
{
    internal class AudioKitArchitecture : Architecture<AudioKitArchitecture>
    {
        internal static PlaySoundChannelSystem PlaySoundChannelSystem { get; } = new PlaySoundChannelSystem();
        internal static AudioKitSettingsModel SettingsModel { get; } = new AudioKitSettingsModel();
        internal static PlayingSoundPoolModel PlayingSoundPoolModel { get; } = new PlayingSoundPoolModel();

        internal static AudioLoaderPoolModel LoaderPoolModel { get; } = new AudioLoaderPoolModel();


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void AutoInit()
        {
            InitArchitecture();
            LogKit.I("AudioKit.Architecture Inited");
        }

        protected override void Init()
        {
            RegisterSystem(new ConsoleModuleSystem());
            RegisterSystem(PlaySoundChannelSystem);
            
            RegisterModel(PlayingSoundPoolModel);
            RegisterModel(LoaderPoolModel);
            RegisterModel(SettingsModel);
        }
    }
}