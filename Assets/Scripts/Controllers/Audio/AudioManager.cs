using UnityEngine;
using System.Collections.Generic;
using SeerGoGame.Utilities;

public class AudioManager : MonoBehaviour
{
    // 懒加载单例
    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AudioManager>();

                if (_instance == null)
                {
                    GameObject audioManagerObj = new GameObject("AudioManager");
                    _instance = audioManagerObj.AddComponent<AudioManager>();
                    DontDestroyOnLoad(audioManagerObj);
                }
            }
            return _instance;
        }
    }

    private AudioSource audioSource;

    // 当前BGM音量（由存档设置决定，范围0-1）
    private float _currentBGMVolume = 1f;

    // 音乐缓存（路径 → AudioClip）
    private Dictionary<string, AudioClip> audioClipCache = new Dictionary<string, AudioClip>();
    // 缓存使用记录
    private List<string> cacheUsageOrder = new List<string>();
    // 最大缓存数量
    private const int MAX_CACHE_SIZE = 30;

    // 当前正在加载的音乐路径（防止重复加载）
    private string _loadingPath = null;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeAudioSource();
    }

    private void InitializeAudioSource()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }

    /// <summary>
    /// 播放音乐（异步加载外部MP3资源）
    /// resourcesPath 格式: "Audio/Fight/FightMusic" 或 "Audio/Maps/BGM_001"
    /// </summary>
    public void PlayMusic(string resourcesPath, float volume = 1f, bool loop = true)
    {
        if (string.IsNullOrEmpty(resourcesPath))
        {
            SeerGoLogger.LogError("音乐路径为空");
            return;
        }

        if (audioSource == null)
            InitializeAudioSource();

        // 停止当前播放
        if (audioSource.isPlaying)
            audioSource.Stop();

        audioSource.loop = loop;

        // 从缓存获取或异步加载音频
        _loadingPath = resourcesPath;
        GetAudioClipFromCache(resourcesPath, (clip) =>
        {
            if (_loadingPath != resourcesPath)
                return; // 已被新的加载请求覆盖

            _loadingPath = null;

            if (clip == null)
            {
                SeerGoLogger.LogError($"音乐加载失败: {resourcesPath}");
                return;
            }

            audioSource.clip = clip;
            _currentBGMVolume = Mathf.Clamp01(volume);
            audioSource.volume = _currentBGMVolume;
            audioSource.time = 0f;
            audioSource.Play();
        });
    }

    private void GetAudioClipFromCache(string resourcesPath, System.Action<AudioClip> callback)
    {
        // 缓存命中：更新使用记录并直接返回
        if (audioClipCache.TryGetValue(resourcesPath, out AudioClip cached) && cached != null)
        {
            cacheUsageOrder.Remove(resourcesPath);
            cacheUsageOrder.Add(resourcesPath);
            callback?.Invoke(cached);
            return;
        }

        // 缓存未命中：使用 ExternalResourcesManager 异步加载
        ExternalResourcesManager.Instance.LoadAudioAsync(resourcesPath, (clip) =>
        {
            if (clip != null)
            {
                AddAudioClipToCache(resourcesPath, clip);
            }
            callback?.Invoke(clip);
        });
    }

    private void AddAudioClipToCache(string resourcesPath, AudioClip clip)
    {
        // LRU淘汰
        if (cacheUsageOrder.Count >= MAX_CACHE_SIZE)
        {
            string leastUsedPath = cacheUsageOrder[0];
            RemoveAudioClipFromCache(leastUsedPath);
        }

        audioClipCache[resourcesPath] = clip;
        cacheUsageOrder.Add(resourcesPath);
    }

    private void RemoveAudioClipFromCache(string resourcesPath)
    {
        if (audioClipCache.TryGetValue(resourcesPath, out AudioClip clipToRemove))
        {
            audioClipCache.Remove(resourcesPath);
            cacheUsageOrder.Remove(resourcesPath);

            // 不释放当前正在播放的音乐
            if (audioSource != null && audioSource.clip != clipToRemove && clipToRemove != null)
            {
                Destroy(clipToRemove);
            }
        }
    }

    public void SetLoop(bool loop)
    {
        if (audioSource != null)
            audioSource.loop = loop;
    }

    public void StopMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    public void PauseMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }

    public void ResumeMusic()
    {
        if (audioSource != null && audioSource.clip != null && !audioSource.isPlaying)
        {
            audioSource.UnPause();
        }
    }

    public void SetVolume(float volume)
    {
        if (audioSource != null)
            audioSource.volume = Mathf.Clamp01(volume);
    }

    /// <summary>
    /// 将存档中的音量整数（范围0-10）转换为浮点数（范围0-1）
    /// </summary>
    public static float ConvertVolumeIntToFloat(int volumeInt)
    {
        return Mathf.Clamp01(volumeInt / 10f);
    }

    /// <summary>
    /// 根据存档音量设置播放BGM音乐
    /// </summary>
    public void PlayMusicWithSaveVolume(string resourcesPath, int saveVolumeInt, bool loop = true)
    {
        float volume = ConvertVolumeIntToFloat(saveVolumeInt);
        PlayMusic(resourcesPath, volume, loop);
    }

    /// <summary>
    /// 获取当前BGM音量（0-1范围）
    /// </summary>
    public float GetCurrentBGMVolume()
    {
        return _currentBGMVolume;
    }

    /// <summary>
    /// 清空缓存（释放所有非当前播放的音乐资源）
    /// </summary>
    public void ClearCache()
    {
        AudioClip currentClip = audioSource?.clip;

        foreach (var pair in audioClipCache)
        {
            if (pair.Value != currentClip && pair.Value != null)
            {
                Destroy(pair.Value);
            }
        }

        audioClipCache.Clear();
        cacheUsageOrder.Clear();
        _loadingPath = null;

        SeerGoLogger.Log("音乐缓存已清空");
    }

    public int GetCacheCount()
    {
        return audioClipCache.Count;
    }

    public List<string> GetCacheKeys()
    {
        return new List<string>(cacheUsageOrder);
    }

    private void OnDestroy()
    {
        ClearCache();
        if (_instance == this)
            _instance = null;
    }
}
