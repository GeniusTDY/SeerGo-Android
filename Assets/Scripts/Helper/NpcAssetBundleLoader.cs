using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// NPC + PetWalk AssetBundle 运行时加载器
/// 
/// resId 映射规则:
///   "Prefabs/NPC_-50005_1" → NpcAnim/{Platform}/npc_-50005_1 → Asset: NPC_-50005_1
///   "Prefabs/Pet_10"       → PetWalkAnim/{Platform}/pwa_10     → Asset: Pet_10
/// 
/// 使用:
///   // 异步加载（推荐，兼容 async/await）
///   GameObject prefab = await NpcAssetBundleLoader.LoadPrefabAsync("Prefabs/NPC_-50005_1");
/// 
///   // 卸载
///   NpcAssetBundleLoader.UnloadByResId("Prefabs/NPC_-50005_1");
///   NpcAssetBundleLoader.UnloadAll();
/// </summary>
public static class NpcAssetBundleLoader
{
    // ============================================================
    // 内部状态
    // ============================================================

    /// <summary>(bundleName, AssetBundle) 缓存</summary>
    private static readonly Dictionary<string, AssetBundle> LoadedBundles = new();

    /// <summary>(resId, GameObject prefab) 缓存</summary>
    private static readonly Dictionary<string, GameObject> PrefabCache = new();

    /// <summary>正在异步加载中的 Bundle Task，防止并发重复加载</summary>
    private static readonly Dictionary<string, Task<AssetBundle>> _loadingTasks = new();

    private static string _npcRootPath;
    private static string _petWalkRootPath;
    private static string _mapRootPath;
    private static bool _inited;

    // ============================================================
    // 初始化
    // ============================================================

    /// <summary>初始化路径。首次调用任何方法时自动执行。</summary>
    public static void Initialize(string customNpcPath = null, string customPetWalkPath = null, string customMapPath = null)
    {
        if (_inited) return;

        // PC/Editor: 项目根目录下的子目录；Android: persistentDataPath
#if UNITY_ANDROID && !UNITY_EDITOR
        string basePath = Application.persistentDataPath;
#else
        string basePath = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
#endif
        _npcRootPath     = customNpcPath     ?? Path.Combine(basePath, "GameAnim/NpcAnim");
        _petWalkRootPath = customPetWalkPath ?? Path.Combine(basePath, "GameAnim/PetWalkAnim");
        _mapRootPath     = customMapPath     ?? Path.Combine(basePath, "GameAnim/MapAnim");

        _inited = true;
        Debug.Log($"[NpcABLoader] 初始化:\n  NPC: {_npcRootPath}\n  PetWalk: {_petWalkRootPath}\n  Map: {_mapRootPath}");
    }

    private static void EnsureInit() { if (!_inited) Initialize(); }

    // ============================================================
    // 核心 API：按 resId 加载
    // ============================================================

    /// <summary>
    /// 异步加载预制体（推荐）
    /// GameObject prefab = await NpcAssetBundleLoader.LoadPrefabAsync("Prefabs/NPC_-50005_1");
    /// </summary>
    public static async Task<GameObject> LoadPrefabAsync(string resId)
    {
        EnsureInit();

        // 缓存命中
        if (PrefabCache.TryGetValue(resId, out var cached) && cached != null)
            return cached;

        var info = ParseResId(resId);
        if (info == null) return null;

        // 加载 Bundle
        var bundle = await LoadBundleAsync(info.BundlePath, info.BundleName);
        if (bundle == null) return null;

        // 加载 Prefab
        var prefab = LoadPrefabFromBundle(bundle, info.BundleName, info.PrefabName);
        if (prefab != null)
            PrefabCache[resId] = prefab;

        return prefab;
    }

    /// <summary>
    /// 同步加载预制体（谨慎使用，建议用 LoadPrefabAsync）
    /// </summary>
    public static GameObject LoadPrefab(string resId)
    {
        EnsureInit();

        if (PrefabCache.TryGetValue(resId, out var cached) && cached != null)
            return cached;

        var info = ParseResId(resId);
        if (info == null) return null;

        var bundle = LoadBundle(info.BundlePath, info.BundleName);
        if (bundle == null) return null;

        var prefab = LoadPrefabFromBundle(bundle, info.BundleName, info.PrefabName);
        if (prefab != null)
            PrefabCache[resId] = prefab;

        return prefab;
    }

    /// <summary>
    /// 检查某个 resId 对应的 Bundle 文件是否存在
    /// </summary>
    public static bool ResIdExists(string resId)
    {
        EnsureInit();
        var info = ParseResId(resId);
        return info != null && File.Exists(info.BundlePath);
    }

    /// <summary>
    /// 获取某个 resId 对应的 Bundle 文件完整路径（调试用）
    /// </summary>
    public static string GetBundlePath(string resId)
    {
        EnsureInit();
        var info = ParseResId(resId);
        return info?.BundlePath ?? "";
    }

    /// <summary>
    /// 检查 Bundle 是否已加载
    /// </summary>
    public static bool IsBundleLoaded(string bundleName) => LoadedBundles.ContainsKey(bundleName);

    // ============================================================
    // 卸载
    // ============================================================

    /// <summary>按 resId 卸载</summary>
    public static void UnloadByResId(string resId, bool unloadAllObjects = false)
    {
        var info = ParseResId(resId);
        if (info == null) return;

        PrefabCache.Remove(resId);

        if (LoadedBundles.TryGetValue(info.BundleName, out var bundle))
        {
            bundle.Unload(unloadAllObjects);
            LoadedBundles.Remove(info.BundleName);
            Debug.Log($"[NpcABLoader] 卸载: {info.BundleName}");
        }
    }

    /// <summary>卸载全部</summary>
    public static void UnloadAll(bool unloadAllObjects = false)
    {
        _loadingTasks.Clear();
        foreach (var b in LoadedBundles.Values)
            b?.Unload(unloadAllObjects);
        LoadedBundles.Clear();
        PrefabCache.Clear();
        Debug.Log("[NpcABLoader] 已卸载全部");
    }

    // ============================================================
    // Map 动画特效：按 mapId 批量加载
    // ============================================================

    /// <summary>
    /// 加载指定 mapId 下所有 Map 动画预制体（异步）
    /// 扫描 MapAnim/ 目录下所有 Map_{mapId}_* 的 Bundle，加载其中的预制体
    /// </summary>
    public static async Task<List<GameObject>> LoadMapEffectsAsync(int mapId)
    {
        EnsureInit();
        var results = new List<GameObject>();
        string pattern = $"Map_{mapId}_";

        Debug.Log($"[NpcABLoader] LoadMapEffects mapId={mapId}, 目录={_mapRootPath}, 存在={Directory.Exists(_mapRootPath)}");

        if (!Directory.Exists(_mapRootPath))
        {
            Debug.LogWarning($"[NpcABLoader] MapAnim 目录不存在: {_mapRootPath}");
            return results;
        }

        var allFiles = Directory.GetFiles(_mapRootPath);
        Debug.Log($"[NpcABLoader] MapAnim 目录总文件数: {allFiles.Length}");

        foreach (var file in allFiles)
        {
            string fn = Path.GetFileName(file);
            Debug.Log($"[NpcABLoader]   检查文件: {fn}, 匹配={fn.StartsWith(pattern, System.StringComparison.OrdinalIgnoreCase)}, manifest={fn.EndsWith(".manifest")}");
            if (!fn.StartsWith(pattern, System.StringComparison.OrdinalIgnoreCase) || fn.EndsWith(".manifest")) continue;

            string resId = $"Prefabs/{fn}";
            Debug.Log($"[NpcABLoader]   加载: {resId}");
            var prefab = await LoadPrefabAsync(resId);
            if (prefab != null) results.Add(prefab);
            else Debug.LogWarning($"[NpcABLoader]   加载失败: {resId}");
        }

        Debug.Log($"[NpcABLoader] Map 特效加载完成: mapId={mapId}, 成功={results.Count}/{allFiles.Length}");
        return results;
    }

    /// <summary>
    /// 卸载指定 mapId 下的所有 Map 动画 Bundle
    /// </summary>
    public static void UnloadMapEffects(int mapId, bool unloadAllObjects = false)
    {
        string pattern = $"Map_{mapId}_";
        var toUnload = new List<string>();

        foreach (var key in LoadedBundles.Keys)
        {
            if (key.StartsWith(pattern, System.StringComparison.OrdinalIgnoreCase))
                toUnload.Add(key);
        }

        foreach (var key in toUnload)
        {
            if (LoadedBundles.TryGetValue(key, out var bundle))
            {
                bundle.Unload(unloadAllObjects);
                LoadedBundles.Remove(key);
                // 清理相关 PrefabCache
                var cacheKeys = new List<string>(PrefabCache.Keys);
                foreach (var ck in cacheKeys)
                    if (ck.Contains(key))
                        PrefabCache.Remove(ck);
            }
        }

        if (toUnload.Count > 0)
            Debug.Log($"[NpcABLoader] 卸载 Map 特效: mapId={mapId}, {toUnload.Count} 个 Bundle");
    }

    // ============================================================
    // 内部：resId 解析
    // ============================================================

    /// <summary>
    /// 解析 resId，返回 Bundle 信息
    /// "Prefabs/NPC_-50005_1" → NpcAnim/npc_-50005_1       → Asset: NPC_-50005_1
    /// "Prefabs/Pet_10"       → PetWalkAnim/pwa_10         → Asset: Pet_10
    /// "Prefabs/Map_-50001_1" → MapAnim/Map_-50001_1       → Asset: Map_-50001_1
    /// </summary>
    private static ResIdInfo ParseResId(string resId)
    {
        if (string.IsNullOrEmpty(resId)) return null;

        const string prefix = "Prefabs/";
        if (!resId.StartsWith(prefix)) return null;

        string rest = resId.Substring(prefix.Length);

        if (rest.StartsWith("NPC_"))
        {
            var m = System.Text.RegularExpressions.Regex.Match(rest, @"^NPC_(-?\d+)_(\d+)$");
            if (!m.Success) return null;
            string bundleName = $"npc_{m.Groups[1].Value}_{m.Groups[2].Value}";
            return new ResIdInfo
            {
                BundleName = bundleName,
                BundlePath = Path.Combine(_npcRootPath, bundleName),
                PrefabName = rest,
                Type = ResType.Npc
            };
        }
        else if (rest.StartsWith("Pet_"))
        {
            var m = System.Text.RegularExpressions.Regex.Match(rest, @"^Pet_(\d+)$");
            if (!m.Success) return null;
            string bundleName = $"pwa_{m.Groups[1].Value}";
            return new ResIdInfo
            {
                BundleName = bundleName,
                BundlePath = Path.Combine(_petWalkRootPath, bundleName),
                PrefabName = rest,
                Type = ResType.PetWalk
            };
        }
        else if (rest.StartsWith("Map_", System.StringComparison.OrdinalIgnoreCase))
        {
            var m = System.Text.RegularExpressions.Regex.Match(rest, @"^Map_(-?\d+)[_-](\d+)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (!m.Success) return null;
            string bundleName = rest; // bundle 名 = 预制体名，如 Map_-50001_1
            return new ResIdInfo
            {
                BundleName = bundleName,
                BundlePath = Path.Combine(_mapRootPath, bundleName),
                PrefabName = rest,
                Type = ResType.Map
            };
        }

        return null;
    }

    // ============================================================
    // 内部：Bundle 加载
    // ============================================================

    private static AssetBundle LoadBundle(string path, string bundleName)
    {
        // 缓存
        if (LoadedBundles.TryGetValue(bundleName, out var cached) && cached != null)
            return cached;

        // 检查已加载
        var existing = FindExistingBundle(bundleName);
        if (existing != null)
        {
            LoadedBundles[bundleName] = existing;
            return existing;
        }

        // 文件加载
        if (!File.Exists(path))
        {
            Debug.LogError($"[NpcABLoader] Bundle 不存在: {path}");
            return null;
        }

        var bundle = AssetBundle.LoadFromFile(path);
        if (bundle != null)
        {
            LoadedBundles[bundleName] = bundle;
            Debug.Log($"[NpcABLoader] 加载 Bundle: {bundleName} ({path})");
        }
        return bundle;
    }

    private static async Task<AssetBundle> LoadBundleAsync(string path, string bundleName)
    {
        // 1. 已缓存
        if (LoadedBundles.TryGetValue(bundleName, out var cached) && cached != null)
            return cached;

        // 2. 检查 Unity 全局已加载
        var existing = FindExistingBundle(bundleName);
        if (existing != null)
        {
            LoadedBundles[bundleName] = existing;
            return existing;
        }

        // 3. 正在加载中 → 等待同一个 Task
        if (_loadingTasks.TryGetValue(bundleName, out var loadingTask))
        {
            Debug.Log($"[NpcABLoader] 等待并发加载: {bundleName}");
            return await loadingTask;
        }

        // 4. 文件加载
        if (!File.Exists(path))
        {
            Debug.LogError($"[NpcABLoader] Bundle 不存在: {path}");
            return null;
        }

        var tcs = new TaskCompletionSource<AssetBundle>();
        _loadingTasks[bundleName] = tcs.Task;

        try
        {
            var request = AssetBundle.LoadFromFileAsync(path);
            request.completed += _ => tcs.TrySetResult(request.assetBundle);
            var bundle = await tcs.Task;

            if (bundle != null)
            {
                LoadedBundles[bundleName] = bundle;
                Debug.Log($"[NpcABLoader] 异步加载 Bundle: {bundleName} ({path})");
            }
            return bundle;
        }
        finally
        {
            _loadingTasks.Remove(bundleName);
        }
    }

    // ============================================================
    // 内部：Prefab 加载
    // ============================================================

    private static GameObject LoadPrefabFromBundle(AssetBundle bundle, string bundleName, string prefabName)
    {
        if (bundle == null) return null;

        // 1. 直接用名字加载
        var prefab = bundle.LoadAsset<GameObject>(prefabName);
        if (prefab != null) return prefab;

        // 2. 带 .prefab 扩展名
        prefab = bundle.LoadAsset<GameObject>(prefabName + ".prefab");
        if (prefab != null) return prefab;

        // 3. 搜索 bundle 中所有资源
        foreach (var name in bundle.GetAllAssetNames())
        {
            string fileName = Path.GetFileNameWithoutExtension(name);
            if (fileName == prefabName || name.Contains(prefabName))
            {
                prefab = bundle.LoadAsset<GameObject>(name);
                if (prefab != null) return prefab;
            }
        }

        Debug.LogError($"[NpcABLoader] Bundle '{bundleName}' 中未找到预制体: {prefabName}");
        return null;
    }

    private static AssetBundle FindExistingBundle(string bundleName)
    {
        foreach (var b in AssetBundle.GetAllLoadedAssetBundles())
        {
            if (b.name == bundleName || Path.GetFileNameWithoutExtension(b.name) == bundleName)
                return b;
        }
        return null;
    }

    // ============================================================
    // 数据结构
    // ============================================================

    private enum ResType { Npc, PetWalk, Map }

    private class ResIdInfo
    {
        public string BundleName;
        public string BundlePath;
        public string PrefabName;
        public ResType Type;
    }
}
