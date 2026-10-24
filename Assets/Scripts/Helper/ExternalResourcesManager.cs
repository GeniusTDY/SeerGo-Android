using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using SeerGoGame.Utilities;

/// <summary>
/// 外部资源管理器 - 从项目根目录 GameResources/ 文件夹加载图片(PNG)和音频(MP3)
/// 支持 PC 和 Android 平台路径适配，带缓存和内存释放管理
/// 
/// PC 路径: {ProjectRoot}/GameResources/
/// Android 路径: {persistentDataPath}/GameResources/
/// 
/// 资源目录结构:
///   GameResources/Audio/         - MP3音频
///   GameResources/Item/          - 物品图标PNG
///   GameResources/Maps/bg/       - 地图背景PNG
///   GameResources/Maps/Npc/      - NPC精灵PNG
///   GameResources/PetFightImage/ - 战斗精灵PNG
///   GameResources/PetImage/      - 精灵图片PNG
/// </summary>
public class ExternalResourcesManager : MonoBehaviour
{
    private static ExternalResourcesManager _instance;
    public static ExternalResourcesManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("ExternalResourcesManager");
                _instance = go.AddComponent<ExternalResourcesManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    [Header("调试设置")]
    public bool enableLogging = false;

    /// <summary>Sprite缓存: 路径 → Sprite</summary>
    private Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();
    /// <summary>AudioClip缓存: 路径 → AudioClip</summary>
    private Dictionary<string, AudioClip> _audioCache = new Dictionary<string, AudioClip>();
    /// <summary>加载顺序记录(用于LRU淘汰)</summary>
    private List<string> _spriteUsageOrder = new List<string>();
    private List<string> _audioUsageOrder = new List<string>();

    private const int MAX_SPRITE_CACHE = 200;
    private const int MAX_AUDIO_CACHE = 30;

    /// <summary>外部资源根目录(平台感知)</summary>
    private string _basePath;
    private bool _pathInitialized = false;

    /// <summary>正在异步加载的协程追踪</summary>
    private Dictionary<string, Coroutine> _loadingCoroutines = new Dictionary<string, Coroutine>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeBasePath();
    }

    /// <summary>
    /// 初始化平台感知的根路径
    /// </summary>
    public void InitializeBasePath()
    {
        if (_pathInitialized) return;

#if UNITY_ANDROID && !UNITY_EDITOR
        _basePath = Path.Combine(Application.persistentDataPath, "GameResources");
#else
        // PC/Editor: 项目根目录下的 GameResources 文件夹
        _basePath = Path.GetFullPath(Path.Combine(Application.dataPath, "../GameResources"));
#endif
        _pathInitialized = true;
        Log($"外部资源根路径: {_basePath}");
    }

    /// <summary>
    /// 获取平台感知的外部资源完整路径
    /// </summary>
    public string GetFullPath(string relativePath)
    {
        InitializeBasePath();
        string fullPath = Path.Combine(_basePath, relativePath);
        fullPath = fullPath.Replace("\\", "/");
        return fullPath;
    }

    /// <summary>
    /// 检查外部资源文件是否存在
    /// </summary>
    public bool FileExists(string relativePath)
    {
        string pngPath = GetFullPath(relativePath + ".png");
        string mp3Path = GetFullPath(relativePath + ".mp3");
        string rawPath = GetFullPath(relativePath);
        return File.Exists(pngPath) || File.Exists(mp3Path) || File.Exists(rawPath);
    }

    #region Sprite 加载

    /// <summary>
    /// 同步加载外部Sprite（PNG格式）
    /// </summary>
    /// <param name="relativePath">相对路径，不含扩展名，如 "Item/1001"</param>
    /// <param name="readable">是否保持纹理可读（用于像素级访问，如遮罩采样），默认 false</param>
    /// <returns>加载的Sprite，失败返回null</returns>
    public Sprite LoadSprite(string relativePath, bool readable = false)
    {
        if (string.IsNullOrEmpty(relativePath))
            return null;

        // readable 纹理使用独立缓存键，避免与非 readable 版本冲突
        string cacheKey = readable ? $"{relativePath}__readable" : relativePath;

        // 缓存检查
        if (_spriteCache.TryGetValue(cacheKey, out Sprite cached) && cached != null)
        {
            UpdateSpriteUsage(cacheKey);
            return cached;
        }

        Sprite sprite = null;

#if UNITY_ANDROID && !UNITY_EDITOR
        sprite = LoadSpriteAndroid(relativePath, readable);
#else
        sprite = LoadSpritePC(relativePath, readable);
#endif

        if (sprite != null)
        {
            AddSpriteToCache(cacheKey, sprite);
        }

        return sprite;
    }

    /// <summary>
    /// PC端: 通过文件IO直接加载PNG
    /// </summary>
    private Sprite LoadSpritePC(string relativePath, bool readable = false)
    {
        string fullPath = GetFullPath(relativePath + ".png");

        if (!File.Exists(fullPath))
        {
            // 尝试不带扩展名的路径
            fullPath = GetFullPath(relativePath);
            if (!File.Exists(fullPath))
            {
                LogError($"Sprite文件不存在: {_basePath}/{relativePath}.png");
                return null;
            }
        }

        try
        {
            byte[] bytes = File.ReadAllBytes(fullPath);
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (tex.LoadImage(bytes))
            {
                tex.filterMode = FilterMode.Bilinear;
                tex.wrapMode = TextureWrapMode.Clamp;

                // readable: 保持CPU端像素数据可访问（用于遮罩采样等）
                if (readable)
                {
                    tex.Apply(false, false); // makeNoLongerReadable = false → 保持可读
                    Log($"加载可读Sprite成功: {relativePath} ({tex.width}x{tex.height})");
                }

                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                sprite.name = Path.GetFileNameWithoutExtension(fullPath);
                Log($"加载Sprite成功: {relativePath}");
                return sprite;
            }
            else
            {
                LogError($"加载Sprite失败(Texture2D.LoadImage): {relativePath}");
                DestroyImmediate(tex);
            }
        }
        catch (Exception e)
        {
            LogError($"加载Sprite异常: {relativePath}, {e.Message}");
        }

        return null;
    }

    /// <summary>
    /// Android端: 先从StreamingAssets复制到persistentDataPath后加载
    /// 或直接通过UnityWebRequest加载
    /// </summary>
    private Sprite LoadSpriteAndroid(string relativePath, bool readable = false)
    {
        string fullPath = GetFullPath(relativePath + ".png");

        if (File.Exists(fullPath))
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(fullPath);
                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (tex.LoadImage(bytes))
                {
                    tex.filterMode = FilterMode.Bilinear;
                    tex.wrapMode = TextureWrapMode.Clamp;

                    if (readable)
                    {
                        tex.Apply(false, false);
                    }

                    Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                    sprite.name = Path.GetFileNameWithoutExtension(fullPath);
                    return sprite;
                }
                DestroyImmediate(tex);
            }
            catch (Exception e)
            {
                LogError($"Android加载Sprite异常: {relativePath}, {e.Message}");
            }
        }

        return null;
    }

    /// <summary>
    /// 异步加载Sprite（使用协程）
    /// </summary>
    /// <param name="readable">是否保持纹理可读</param>
    public void LoadSpriteAsync(string relativePath, Action<Sprite> callback, bool readable = false)
    {
        string cacheKey = readable ? $"{relativePath}__readable" : relativePath;
        LoadSpriteAsyncInternal(relativePath, cacheKey, callback, readable);
    }

    private void LoadSpriteAsyncInternal(string relativePath, string cacheKey, Action<Sprite> callback, bool readable)
    {
        if (_spriteCache.TryGetValue(cacheKey, out Sprite cached) && cached != null)
        {
            UpdateSpriteUsage(cacheKey);
            callback?.Invoke(cached);
            return;
        }

        StartCoroutine(LoadSpriteCoroutine(relativePath, cacheKey, callback, readable));
    }

    private IEnumerator LoadSpriteCoroutine(string relativePath, string cacheKey, Action<Sprite> callback, bool readable)
    {
        // 防止重复加载
        if (_loadingCoroutines.ContainsKey(cacheKey))
        {
            yield return _loadingCoroutines[cacheKey];
            if (_spriteCache.TryGetValue(cacheKey, out Sprite cached))
                callback?.Invoke(cached);
            else
                callback?.Invoke(null);
            yield break;
        }

        string fullPath = GetFullPath(relativePath + ".png");

#if UNITY_ANDROID && !UNITY_EDITOR
        if (!File.Exists(fullPath))
        {
            LogError($"异步加载Sprite文件不存在: {fullPath}");
            callback?.Invoke(null);
            yield break;
        }

        // Android同步加载
        Sprite sprite = LoadSpriteAndroid(relativePath, readable);
        if (sprite != null)
            AddSpriteToCache(cacheKey, sprite);
        callback?.Invoke(sprite);
#else
        if (!File.Exists(fullPath))
        {
            fullPath = GetFullPath(relativePath);
            if (!File.Exists(fullPath))
            {
                LogError($"异步加载Sprite文件不存在: {relativePath}");
                callback?.Invoke(null);
                yield break;
            }
        }

        // 后台线程读取文件
        byte[] bytes = null;
        bool loadSuccess = false;
        var task = System.Threading.Tasks.Task.Run(() =>
        {
            try
            {
                bytes = File.ReadAllBytes(fullPath);
                loadSuccess = true;
            }
            catch { }
        });

        while (!task.IsCompleted)
            yield return null;

        if (!loadSuccess || bytes == null)
        {
            callback?.Invoke(null);
            yield break;
        }

        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (tex.LoadImage(bytes))
        {
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            if (readable)
                tex.Apply(false, false);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            sprite.name = Path.GetFileNameWithoutExtension(fullPath);
            AddSpriteToCache(cacheKey, sprite);
            callback?.Invoke(sprite);
        }
        else
        {
            DestroyImmediate(tex);
            callback?.Invoke(null);
        }
#endif
    }

    #endregion

    #region Audio 加载

    /// <summary>
    /// 同步加载外部AudioClip（MP3格式）- 推荐使用异步版本
    /// </summary>
    public AudioClip LoadAudio(string relativePath)
    {
        if (_audioCache.TryGetValue(relativePath, out AudioClip cached) && cached != null)
        {
            UpdateAudioUsage(relativePath);
            return cached;
        }

        LogError("同步加载AudioClip请使用 LoadAudioAsync，这里返回null");
        return null;
    }

    /// <summary>
    /// 异步加载AudioClip（使用UnityWebRequest）
    /// </summary>
    public void LoadAudioAsync(string relativePath, Action<AudioClip> callback)
    {
        if (_audioCache.TryGetValue(relativePath, out AudioClip cached) && cached != null)
        {
            UpdateAudioUsage(relativePath);
            callback?.Invoke(cached);
            return;
        }

        StartCoroutine(LoadAudioCoroutine(relativePath, callback));
    }

    private IEnumerator LoadAudioCoroutine(string relativePath, Action<AudioClip> callback)
    {
        string fullPath = GetFullPath(relativePath + ".mp3");

#if UNITY_ANDROID && !UNITY_EDITOR
        // Android: 使用 file:// 协议
        string url = "file://" + fullPath;
#else
        // PC: 使用 file:// 协议
        string url = "file:///" + fullPath;
#endif

        Log($"加载Audio: {url}");

        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                if (clip != null)
                {
                    clip.name = Path.GetFileNameWithoutExtension(relativePath);
                    AddAudioToCache(relativePath, clip);
                    Log($"加载Audio成功: {relativePath}");
                    callback?.Invoke(clip);
                }
                else
                {
                    LogError($"加载Audio失败(空clip): {relativePath}");
                    callback?.Invoke(null);
                }
            }
            else
            {
                LogError($"加载Audio失败: {relativePath}, 错误: {request.error}");
                callback?.Invoke(null);
            }
        }
    }

    #endregion

    #region 缓存管理

    private void AddSpriteToCache(string path, Sprite sprite)
    {
        // LRU淘汰
        while (_spriteUsageOrder.Count >= MAX_SPRITE_CACHE)
        {
            string oldest = _spriteUsageOrder[0];
            RemoveSpriteFromCache(oldest);
        }

        _spriteCache[path] = sprite;
        _spriteUsageOrder.Add(path);
    }

    private void AddAudioToCache(string path, AudioClip clip)
    {
        while (_audioUsageOrder.Count >= MAX_AUDIO_CACHE)
        {
            string oldest = _audioUsageOrder[0];
            RemoveAudioFromCache(oldest);
        }

        _audioCache[path] = clip;
        _audioUsageOrder.Add(path);
    }

    private void UpdateSpriteUsage(string path)
    {
        _spriteUsageOrder.Remove(path);
        _spriteUsageOrder.Add(path);
    }

    private void UpdateAudioUsage(string path)
    {
        _audioUsageOrder.Remove(path);
        _audioUsageOrder.Add(path);
    }

    private void RemoveSpriteFromCache(string path)
    {
        if (_spriteCache.TryGetValue(path, out Sprite sprite))
        {
            if (sprite != null && sprite.texture != null)
            {
                // Destroy（延迟销毁），避免 DestroyImmediate 打断正在使用该纹理的渲染
                Destroy(sprite.texture);
            }
            if (sprite != null)
            {
                Destroy(sprite);
            }
            _spriteCache.Remove(path);
        }
        _spriteUsageOrder.Remove(path);
    }

    private void RemoveAudioFromCache(string path)
    {
        if (_audioCache.TryGetValue(path, out AudioClip clip))
        {
            if (clip != null)
            {
                Destroy(clip);
            }
            _audioCache.Remove(path);
        }
        _audioUsageOrder.Remove(path);
    }

    /// <summary>
    /// 释放指定Sprite缓存
    /// </summary>
    public void ReleaseSprite(string relativePath)
    {
        RemoveSpriteFromCache(relativePath);
    }

    /// <summary>
    /// 释放指定Audio缓存
    /// </summary>
    public void ReleaseAudio(string relativePath)
    {
        RemoveAudioFromCache(relativePath);
    }

    /// <summary>
    /// 释放所有Sprite缓存
    /// </summary>
    public void ReleaseAllSprites()
    {
        foreach (var key in new List<string>(_spriteCache.Keys))
        {
            RemoveSpriteFromCache(key);
        }
        _spriteCache.Clear();
        _spriteUsageOrder.Clear();
        Log("所有Sprite缓存已释放");
    }

    /// <summary>
    /// 释放所有Audio缓存
    /// </summary>
    public void ReleaseAllAudio()
    {
        foreach (var key in new List<string>(_audioCache.Keys))
        {
            RemoveAudioFromCache(key);
        }
        _audioCache.Clear();
        _audioUsageOrder.Clear();
        Log("所有Audio缓存已释放");
    }

    /// <summary>
    /// 释放所有缓存资源
    /// </summary>
    public void ReleaseAll()
    {
        ReleaseAllSprites();
        ReleaseAllAudio();
        StartCoroutine(UnloadUnusedAssetsCoroutine());
    }

    private IEnumerator UnloadUnusedAssetsCoroutine()
    {
        AsyncOperation op = Resources.UnloadUnusedAssets();
        while (!op.isDone)
            yield return null;
        System.GC.Collect();
        Log("未使用资源卸载完成");
    }

    /// <summary>
    /// 按前缀批量释放Sprite缓存（用于场景切换时释放地图相关资源）
    /// </summary>
    public void ReleaseSpritesByPrefix(string prefix)
    {
        var keysToRemove = new List<string>();
        foreach (var key in _spriteCache.Keys)
        {
            if (key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                keysToRemove.Add(key);
        }
        foreach (var key in keysToRemove)
        {
            RemoveSpriteFromCache(key);
        }
        if (keysToRemove.Count > 0)
            Log($"按前缀释放Sprite: {prefix}, 数量: {keysToRemove.Count}");
    }

    #endregion

    #region 调试与工具

    /// <summary>
    /// 预热加载（异步批量预加载）
    /// </summary>
    public void PreloadSpritesAsync(List<string> relativePaths, Action<int, int> onProgress = null, Action onComplete = null)
    {
        StartCoroutine(PreloadSpritesCoroutine(relativePaths, onProgress, onComplete));
    }

    private IEnumerator PreloadSpritesCoroutine(List<string> paths, Action<int, int> onProgress, Action onComplete)
    {
        int total = paths.Count;
        int loaded = 0;

        foreach (var path in paths)
        {
            if (!_spriteCache.ContainsKey(path))
            {
                Sprite sprite = LoadSprite(path);
                if (sprite != null) loaded++;
            }
            else
            {
                loaded++;
            }
            onProgress?.Invoke(loaded, total);
            yield return null;
        }

        onComplete?.Invoke();
    }

    /// <summary>
    /// 获取缓存统计信息
    /// </summary>
    public string GetCacheInfo()
    {
        return $"Sprite缓存: {_spriteCache.Count}/{MAX_SPRITE_CACHE}, Audio缓存: {_audioCache.Count}/{MAX_AUDIO_CACHE}, 根路径: {_basePath}";
    }

    /// <summary>
    /// 获取平台信息
    /// </summary>
    public string GetPlatformInfo()
    {
        return $"平台: {Application.platform}, 根路径: {_basePath}";
    }

    public int SpriteCacheCount => _spriteCache.Count;
    public int AudioCacheCount => _audioCache.Count;

    #endregion

    #region 日志

    private void Log(string msg)
    {
        if (enableLogging)
            SeerGoLogger.Log($"[ExtResMgr] {msg}");
    }

    private void LogError(string msg)
    {
        SeerGoLogger.LogError($"[ExtResMgr] {msg}");
    }

    #endregion

    private void OnDestroy()
    {
        if (_instance == this)
        {
            ReleaseAll();
            _instance = null;
        }
    }
}
