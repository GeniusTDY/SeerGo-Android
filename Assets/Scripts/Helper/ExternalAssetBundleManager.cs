using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SeerGoGame.Utilities;
using System.Text.RegularExpressions;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ExternalAssetBundleManager : MonoBehaviour
{
    private static ExternalAssetBundleManager _instance;
    public static ExternalAssetBundleManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("ExternalAssetBundleManager");
                _instance = go.AddComponent<ExternalAssetBundleManager>();
            }
            return _instance;
        }
    }

    [System.Serializable]
    public class PlatformPaths
    {
        public string windowsPath = "";
        public string androidPath = "";
        public string defaultPath = "";
    }

    [Header("AssetBundle路径配置")]
    public PlatformPaths bundlePaths = new PlatformPaths();

    [Header("调试设置")]
    public bool enableLogging = true;

    private Dictionary<string, AssetBundle> loadedBundles = new Dictionary<string, AssetBundle>();
    private Dictionary<string, GameObject> loadedPrefabs = new Dictionary<string, GameObject>();
    private Dictionary<string, List<GameObject>> instantiatedObjects = new Dictionary<string, List<GameObject>>();
    private string currentPlatformPath = "";

    // 运行时加载事件
    public System.Action<string> OnBundleLoaded;
    public System.Action<string> OnBundleUnloaded;
    public System.Action<string, GameObject> OnPrefabInstantiated;
    public System.Action<List<string>, int, int, int> OnBatchLoadProgress;
    public System.Action<List<string>, bool> OnBatchLoadComplete;

    public void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            InitializePaths();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 初始化路径
    /// </summary>
    private void InitializePaths()
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        currentPlatformPath = string.IsNullOrEmpty(bundlePaths.windowsPath) ?
            GetDefaultWindowsPath() : bundlePaths.windowsPath;
#elif UNITY_ANDROID
        currentPlatformPath = string.IsNullOrEmpty(bundlePaths.androidPath) ? 
            GetDefaultAndroidPath() : bundlePaths.androidPath;
#else
        currentPlatformPath = string.IsNullOrEmpty(bundlePaths.defaultPath) ? 
            Application.streamingAssetsPath : bundlePaths.defaultPath;
#endif

        Log($"AssetBundle路径初始化为: {currentPlatformPath}");
    }

    private string GetDefaultWindowsPath()
    {
#if UNITY_EDITOR
        return Path.Combine(Application.dataPath, "StreamingAssets", "AssetBundles");
#else
        return Path.Combine(Application.dataPath, "..", "AssetBundles");
#endif
    }

    private string GetDefaultAndroidPath()
    {
        return Path.Combine(Application.persistentDataPath, "AssetBundles");
    }

    /// <summary>
    /// 设置自定义路径
    /// </summary>
    public void SetCustomPath(string customPath)
    {
        if (!string.IsNullOrEmpty(customPath))
        {
            currentPlatformPath = customPath;
            Log($"设置自定义路径: {currentPlatformPath}");
        }
    }

    /// <summary>
    /// 获取指定bundle的完整路径
    /// </summary>
    public string GetBundleFilePath(string bundleName, string customPath = "")
    {
        string basePath = string.IsNullOrEmpty(customPath) ? currentPlatformPath : customPath;
        string filePath = Path.Combine(basePath, bundleName);
        filePath = filePath.Replace("\\", "/");
        return filePath;
    }

    /// <summary>
    /// 检查路径是否存在
    /// </summary>
    public bool CheckPathExists(string customPath = "")
    {
        string pathToCheck = string.IsNullOrEmpty(customPath) ? currentPlatformPath : customPath;
        bool exists = Directory.Exists(pathToCheck);
        Log($"检查路径 '{pathToCheck}': {(exists ? "存在" : "不存在")}");
        return exists;
    }

    /// <summary>
    /// 动态加载AssetBundle
    /// </summary>
    public bool LoadAssetBundle(string bundleName, string customPath = "")
    {
        if (string.IsNullOrEmpty(bundleName))
        {
            LogError("Bundle名称为空");
            return false;
        }

        if (loadedBundles.ContainsKey(bundleName))
        {
            Log($"Bundle已加载: {bundleName}");
            return true;
        }

        // 检查Unity内部是否已加载同名Bundle
        if (CheckBundleAlreadyLoadedInUnity(bundleName, out var existingBundle))
        {
            // 增加Addressables Bundle检测
            if (IsAddressablesBundle(existingBundle))
            {
                Log($"跳过Addressables管理的Bundle: {bundleName}");
                return false;
            }

            Log($"发现Unity内部已加载同名Bundle: {existingBundle.name}，重用");
            loadedBundles[bundleName] = existingBundle;
            return true;
        }

        string bundlePath = GetBundleFilePath(bundleName, customPath);

        if (!File.Exists(bundlePath))
        {
            LogError($"AssetBundle文件不存在: {bundlePath}");
            return false;
        }

        Log($"开始加载Bundle: {bundlePath}");
        AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);

        if (bundle == null)
        {
            LogError($"加载AssetBundle失败: {bundleName}");
            return false;
        }

        loadedBundles[bundleName] = bundle;
        Log($"成功加载Bundle: {bundleName}");

        // 列出bundle中的所有资源
        string[] allAssets = bundle.GetAllAssetNames();
        Log($"Bundle {bundleName} 中包含 {allAssets.Length} 个资源:");
        foreach (string asset in allAssets)
        {
            Log($"  - {asset}");
        }

        OnBundleLoaded?.Invoke(bundleName);
        return true;
    }

    /// <summary>
    /// 检查Unity内部是否已加载同名Bundle
    /// </summary>
    private bool CheckBundleAlreadyLoadedInUnity(string bundleName, out AssetBundle existingBundle)
    {
        existingBundle = null;
        var allLoadedBundles = AssetBundle.GetAllLoadedAssetBundles();

        foreach (var bundle in allLoadedBundles)
        {
            string bundleFileName = Path.GetFileNameWithoutExtension(bundle.name);
            string targetFileName = Path.GetFileNameWithoutExtension(bundleName);

            if (bundleFileName == targetFileName || bundle.name == bundleName)
            {
                existingBundle = bundle;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 判断是否为Addressables管理的Bundle
    /// </summary>
    private bool IsAddressablesBundle(AssetBundle bundle)
    {
        if (bundle == null) return false;

        string bundleName = bundle.name;

        // Addressables Bundle的识别特征
        bool isAddressables =
            bundleName.Contains("[UnityAddressables]") ||
            bundleName.StartsWith("archive_") ||
            bundleName.StartsWith("sharedassets_") ||
            bundleName.StartsWith("assets_") ||
            bundleName.Contains("aa_") ||
            bundleName.Contains(".bundle") && bundleName.Contains("assets/") ||
            Regex.IsMatch(bundleName, @"^[a-f0-9]{16}$") ||
            Regex.IsMatch(bundleName, @"^[a-f0-9]{32}$") ||
            Regex.IsMatch(bundleName, @"^assets_[a-f0-9]{16}$") ||
            Regex.IsMatch(bundleName, @"^[a-f0-9]{8}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{4}-[a-f0-9]{12}");

        if (isAddressables)
        {
            Log($"识别为Addressables Bundle: {bundleName}");
        }

        return isAddressables;
    }

    /// <summary>
    /// 批量加载AssetBundles（同步）
    /// </summary>
    public bool LoadAssetBundles(List<string> bundleNames, string customPath = "")
    {
        if (bundleNames == null || bundleNames.Count == 0)
        {
            LogError("Bundle名称列表为空");
            return false;
        }

        int totalCount = bundleNames.Count;
        int successCount = 0;
        int skipCount = 0;
        int failCount = 0;
        List<string> failedBundles = new List<string>();

        Log($"开始批量加载 {totalCount} 个Bundle");

        for (int i = 0; i < bundleNames.Count; i++)
        {
            string bundleName = bundleNames[i];

            OnBatchLoadProgress?.Invoke(bundleNames, i + 1, totalCount, successCount);

            if (string.IsNullOrEmpty(bundleName))
            {
                LogError("Bundle名称为空，跳过");
                failCount++;
                failedBundles.Add(bundleName);
                continue;
            }

            if (loadedBundles.ContainsKey(bundleName))
            {
                Log($"Bundle已加载，跳过: {bundleName}");
                skipCount++;
                successCount++;
                continue;
            }

            // 检查Unity内部是否已加载
            if (CheckBundleAlreadyLoadedInUnity(bundleName, out var existingBundle))
            {
                if (IsAddressablesBundle(existingBundle))
                {
                    Log($"跳过Addressables管理的Bundle: {bundleName}");
                    failCount++;
                    failedBundles.Add(bundleName);
                    continue;
                }

                Log($"发现Unity内部已加载同名Bundle: {existingBundle.name}，重用");
                loadedBundles[bundleName] = existingBundle;
                successCount++;
                continue;
            }

            string bundlePath = GetBundleFilePath(bundleName, customPath);

            if (!File.Exists(bundlePath))
            {
                LogError($"AssetBundle文件不存在: {bundlePath}");
                failCount++;
                failedBundles.Add(bundleName);
                continue;
            }

            AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);

            if (bundle == null)
            {
                LogError($"加载AssetBundle失败: {bundleName}");
                failCount++;
                failedBundles.Add(bundleName);
                continue;
            }

            loadedBundles[bundleName] = bundle;
            successCount++;
            Log($"成功加载Bundle ({i + 1}/{totalCount}): {bundleName}");

            OnBundleLoaded?.Invoke(bundleName);
        }

        bool allSuccess = (failCount == 0);
        Log($"批量加载完成: 总数={totalCount}, 成功={successCount}, 跳过={skipCount}, 失败={failCount}");

        if (failCount > 0)
        {
            LogError($"失败的Bundle: {string.Join(", ", failedBundles)}");
        }

        OnBatchLoadComplete?.Invoke(bundleNames, allSuccess);
        return allSuccess;
    }

    /// <summary>
    /// 批量加载AssetBundles（异步）
    /// </summary>
    public IEnumerator LoadAssetBundlesAsync(List<string> bundleNames, System.Action<bool> onComplete = null, string customPath = "")
    {
        if (bundleNames == null || bundleNames.Count == 0)
        {
            LogError("Bundle名称列表为空");
            onComplete?.Invoke(false);
            yield break;
        }

        int totalCount = bundleNames.Count;
        int successCount = 0;
        int skipCount = 0;
        int failCount = 0;
        List<string> failedBundles = new List<string>();

        Log($"开始异步批量加载 {totalCount} 个Bundle");

        for (int i = 0; i < bundleNames.Count; i++)
        {
            string bundleName = bundleNames[i];

            OnBatchLoadProgress?.Invoke(bundleNames, i + 1, totalCount, successCount);

            if (string.IsNullOrEmpty(bundleName))
            {
                LogError("Bundle名称为空，跳过");
                failCount++;
                failedBundles.Add(bundleName);
                continue;
            }

            if (loadedBundles.ContainsKey(bundleName))
            {
                Log($"Bundle已加载，跳过: {bundleName}");
                skipCount++;
                successCount++;
                continue;
            }

            // 检查Unity内部是否已加载
            if (CheckBundleAlreadyLoadedInUnity(bundleName, out var existingBundle))
            {
                if (IsAddressablesBundle(existingBundle))
                {
                    Log($"跳过Addressables管理的Bundle: {bundleName}");
                    failCount++;
                    failedBundles.Add(bundleName);
                    continue;
                }

                Log($"发现Unity内部已加载同名Bundle: {existingBundle.name}，重用");
                loadedBundles[bundleName] = existingBundle;
                successCount++;
                continue;
            }

            string bundlePath = GetBundleFilePath(bundleName, customPath);

            if (!File.Exists(bundlePath))
            {
                LogError($"AssetBundle文件不存在: {bundlePath}");
                failCount++;
                failedBundles.Add(bundleName);
                continue;
            }

            // 异步加载Bundle
            AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(bundlePath);
            yield return bundleRequest;

            if (bundleRequest.assetBundle == null)
            {
                LogError($"异步加载AssetBundle失败: {bundleName}");
                failCount++;
                failedBundles.Add(bundleName);
                continue;
            }

            loadedBundles[bundleName] = bundleRequest.assetBundle;
            successCount++;
            Log($"成功加载Bundle ({i + 1}/{totalCount}): {bundleName}");

            OnBundleLoaded?.Invoke(bundleName);

            // 可选的：每帧加载一个Bundle，避免卡顿
            yield return null;
        }

        bool allSuccess = (failCount == 0);
        Log($"异步批量加载完成: 总数={totalCount}, 成功={successCount}, 跳过={skipCount}, 失败={failCount}");

        if (failCount > 0)
        {
            LogError($"失败的Bundle: {string.Join(", ", failedBundles)}");
        }

        OnBatchLoadComplete?.Invoke(bundleNames, allSuccess);
        onComplete?.Invoke(allSuccess);
    }

    /// <summary>
    /// 从已加载的Bundle中加载预制体
    /// </summary>
    public GameObject LoadPrefabFromBundle(string bundleName, string prefabName)
    {
        string prefabKey = $"{bundleName}_{prefabName}";

        if (loadedPrefabs.TryGetValue(prefabKey, out GameObject cachedPrefab) && cachedPrefab != null)
        {
            Log($"从缓存加载预制体: {prefabName}");
            return cachedPrefab;
        }

        if (!loadedBundles.ContainsKey(bundleName))
        {
            LogError($"Bundle未加载: {bundleName}");
            return null;
        }

        AssetBundle bundle = loadedBundles[bundleName];
        GameObject prefab = bundle.LoadAsset<GameObject>(prefabName);

        if (prefab == null)
        {
            string[] possibleNames = {
                prefabName,
                prefabName + ".prefab",
                Path.GetFileNameWithoutExtension(prefabName)
            };

            foreach (string name in possibleNames)
            {
                prefab = bundle.LoadAsset<GameObject>(name);
                if (prefab != null) break;
            }

            if (prefab == null)
            {
                LogError($"无法在Bundle {bundleName} 中找到预制体: {prefabName}");
                return null;
            }
        }

        loadedPrefabs[prefabKey] = prefab;
        Log($"成功从Bundle加载预制体: {prefabName}");
        return prefab;
    }

    /// <summary>
    /// 同步加载预制体
    /// </summary>
    public GameObject LoadPetPrefab(string bundleName, string prefabName, string customPath = "")
    {
        string prefabKey = $"{bundleName}_{prefabName}";

        if (loadedPrefabs.TryGetValue(prefabKey, out GameObject cachedPrefab) && cachedPrefab != null)
        {
            Log($"从缓存加载预制体: {prefabName}");
            return cachedPrefab;
        }

        GameObject existingPrefab = FindExistingPrefabInScene(bundleName, prefabName);
        if (existingPrefab != null)
        {
            Log($"发现场景中已存在预制体实例: {prefabName}，使用缓存");
            loadedPrefabs[prefabKey] = existingPrefab;
            return existingPrefab;
        }

        if (!loadedBundles.ContainsKey(bundleName))
        {
            if (!LoadAssetBundle(bundleName, customPath))
            {
                return null;
            }
        }

        return LoadPrefabFromBundle(bundleName, prefabName);
    }

    /// <summary>
    /// 检查场景中是否已存在相同的预制体实例
    /// </summary>
    private GameObject FindExistingPrefabInScene(string bundleName, string prefabName)
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        string targetName = prefabName;

        if (targetName.EndsWith("(Clone)"))
        {
            targetName = targetName.Replace("(Clone)", "").Trim();
        }

        foreach (GameObject obj in allObjects)
        {
            if (obj.name == prefabName || obj.name == targetName)
            {
                Log($"在场景中找到已存在的预制体: {obj.name}，可能来自Bundle: {bundleName}");
                return obj;
            }
        }

        return null;
    }

    /// <summary>
    /// 异步加载预制体
    /// </summary>
    public IEnumerator LoadPetPrefabAsync(string bundleName, string prefabName, System.Action<GameObject> onComplete, string customPath = "")
    {
        string prefabKey = $"{bundleName}_{prefabName}";

        // 1. 先检查缓存
        if (loadedPrefabs.TryGetValue(prefabKey, out GameObject cachedPrefab) && cachedPrefab != null)
        {
            onComplete?.Invoke(cachedPrefab);
            yield break;
        }

        // 2. 检查场景中是否已存在
        GameObject existingPrefab = FindExistingPrefabInScene(bundleName, prefabName);
        if (existingPrefab != null)
        {
            loadedPrefabs[prefabKey] = existingPrefab;
            onComplete?.Invoke(existingPrefab);
            yield break;
        }

        yield return LoadFromAssetBundleAsync(bundleName, prefabName, onComplete, customPath);
    }

    private IEnumerator LoadFromAssetBundleAsync(string bundleName, string prefabName, System.Action<GameObject> onComplete, string customPath)
    {
        string prefabKey = $"{bundleName}_{prefabName}";
        string bundlePath = GetBundleFilePath(bundleName, customPath);

        if (!File.Exists(bundlePath))
        {
            LogError($"AssetBundle文件不存在: {bundlePath}");
            onComplete?.Invoke(null);
            yield break;
        }

        AssetBundle bundle = null;
        if (loadedBundles.ContainsKey(bundleName))
        {
            bundle = loadedBundles[bundleName];
        }
        else
        {
            AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(bundlePath);
            yield return bundleRequest;

            if (bundleRequest.assetBundle == null)
            {
                LogError($"异步加载Bundle失败: {bundleName}");
                onComplete?.Invoke(null);
                yield break;
            }

            bundle = bundleRequest.assetBundle;
            loadedBundles[bundleName] = bundle;
        }

        AssetBundleRequest assetRequest = bundle.LoadAssetAsync<GameObject>(prefabName);
        yield return assetRequest;

        if (assetRequest.asset == null)
        {
            LogError($"异步加载预制体失败: {prefabName}");
            onComplete?.Invoke(null);
            yield break;
        }

        GameObject prefab = assetRequest.asset as GameObject;
        loadedPrefabs[prefabKey] = prefab;
        onComplete?.Invoke(prefab);
    }

    /// <summary>
    /// 实例化预制体
    /// </summary>
    public GameObject InstantiatePet(string bundleName, string prefabName, Vector3 position, Quaternion rotation, string customPath = "")
    {
        string instanceKey = $"{bundleName}_{prefabName}";

        if (instantiatedObjects.TryGetValue(instanceKey, out var objectList) && objectList != null)
        {
            foreach (var obj in objectList)
            {
                if (obj != null && !obj.activeInHierarchy)
                {
                    Log($"重用已实例化的预制体: {prefabName}");
                    obj.transform.position = position;
                    obj.transform.rotation = rotation;
                    obj.SetActive(true);
                    OnPrefabInstantiated?.Invoke(prefabName, obj);
                    return obj;
                }
            }
        }

        GameObject prefab = LoadPetPrefab(bundleName, prefabName, customPath);
        if (prefab != null)
        {
            GameObject instance = Instantiate(prefab, position, rotation);

            if (!instantiatedObjects.ContainsKey(instanceKey))
            {
                instantiatedObjects[instanceKey] = new List<GameObject>();
            }
            instantiatedObjects[instanceKey].Add(instance);

            Log($"实例化预制体: {prefabName}");
            OnPrefabInstantiated?.Invoke(prefabName, instance);
            return instance;
        }
        return null;
    }

    /// <summary>
    /// 安全卸载所有Bundle
    /// </summary>
    /// <summary>
    /// 安全卸载所有Bundle
    /// </summary>
    public IEnumerator SafeUnloadAllBundles(bool unloadAllLoadedObjects = false)
    {
        Log("开始安全卸载所有Bundle...");

        // 1. 先清理所有实例化的对象
        int destroyedCount = 0;
        foreach (var objectList in instantiatedObjects.Values)
        {
            foreach (var obj in objectList)
            {
                if (obj != null)
                {
                    UnityEngine.Object.Destroy(obj);
                    destroyedCount++;
                }
            }
        }
        instantiatedObjects.Clear();
        Log($"销毁了{destroyedCount}个实例化对象");

        // 2. 等待一帧，确保所有Destroy完成
        yield return null;

        // 3. 清理预制体缓存
        loadedPrefabs.Clear();

        // 4. 只卸载我们管理的Bundle，不碰Unity内部的其他Bundle
        int bundleCount = loadedBundles.Count;
        int unloadedCount = 0;
        int skippedCount = 0;
        var bundleKeys = new List<string>(loadedBundles.Keys);

        foreach (var bundleName in bundleKeys)
        {
            if (loadedBundles.TryGetValue(bundleName, out var bundle))
            {
                // 检查是否为Addressables管理的Bundle
                if (IsAddressablesBundle(bundle))
                {
                    Log($"跳过Addressables管理的Bundle: {bundleName}");
                    skippedCount++;
                    continue; // 跳过卸载
                }

                try
                {
                    bundle.Unload(unloadAllLoadedObjects);
                    unloadedCount++;
                    Log($"卸载Bundle: {bundleName}");
                }
                catch (System.Exception e)
                {
                    LogError($"卸载Bundle失败 {bundleName}: {e.Message}");
                }
            }
        }
        loadedBundles.Clear();

        // 5. 不清理Unity内部可能残留的Bundle，避免影响Addressables等其他系统
        // 只清理我们自己的资源
        yield return new WaitForEndOfFrame();
        Resources.UnloadUnusedAssets();
        System.GC.Collect();

        Log($"安全卸载完成: 清理了{unloadedCount}个Bundle，跳过了{skippedCount}个Addressables Bundle");
    }

    /// <summary>
    /// 手动卸载指定的Bundle
    /// </summary>
    public void UnloadBundle(string bundleName, bool unloadAllLoadedObjects = false)
    {
        if (loadedBundles.ContainsKey(bundleName))
        {
            // 检查是否为Addressables Bundle
            if (IsAddressablesBundle(loadedBundles[bundleName]))
            {
                SeerGoLogger.LogWarning($"尝试卸载Addressables管理的Bundle，跳过: {bundleName}");
                return;
            }

            loadedBundles[bundleName].Unload(unloadAllLoadedObjects);
            loadedBundles.Remove(bundleName);

            // 清理相关预制体缓存
            var keysToRemove = new List<string>();
            foreach (var key in loadedPrefabs.Keys)
            {
                if (key.StartsWith(bundleName + "_"))
                    keysToRemove.Add(key);
            }
            foreach (var key in keysToRemove)
            {
                loadedPrefabs.Remove(key);
            }

            // 清理实例化的对象
            var instanceKeysToRemove = new List<string>();
            foreach (var key in instantiatedObjects.Keys)
            {
                if (key.StartsWith(bundleName + "_"))
                {
                    foreach (var obj in instantiatedObjects[key])
                    {
                        if (obj != null)
                            UnityEngine.Object.Destroy(obj);
                    }
                    instanceKeysToRemove.Add(key);
                }
            }
            foreach (var key in instanceKeysToRemove)
            {
                instantiatedObjects.Remove(key);
            }

            Log($"卸载Bundle: {bundleName}");
            OnBundleUnloaded?.Invoke(bundleName);
        }
        else
        {
            LogError($"Bundle未加载，无法卸载: {bundleName}");
        }
    }

    /// <summary>
    /// 清理无效引用
    /// </summary>
    public void CleanupDeadReferences()
    {
        int removedCount = 0;

        // 清理预制体缓存中的空引用
        var prefabKeysToRemove = new List<string>();
        foreach (var pair in loadedPrefabs)
        {
            if (pair.Value == null)
                prefabKeysToRemove.Add(pair.Key);
        }
        foreach (var key in prefabKeysToRemove)
        {
            loadedPrefabs.Remove(key);
            removedCount++;
        }

        // 清理实例化对象列表中的空引用
        var instanceKeysToRemove = new List<string>();
        foreach (var pair in instantiatedObjects)
        {
            pair.Value.RemoveAll(obj => obj == null);
            if (pair.Value.Count == 0)
                instanceKeysToRemove.Add(pair.Key);
        }
        foreach (var key in instanceKeysToRemove)
        {
            instantiatedObjects.Remove(key);
            removedCount++;
        }

        if (removedCount > 0)
        {
            Log($"清理了{removedCount}个无效引用");
        }
    }

    /// <summary>
    /// 获取已加载的Bundle列表
    /// </summary>
    public List<string> GetLoadedBundles()
    {
        return new List<string>(loadedBundles.Keys);
    }

    /// <summary>
    /// 获取已加载的预制体列表
    /// </summary>
    public List<string> GetLoadedPrefabs()
    {
        return new List<string>(loadedPrefabs.Keys);
    }

    /// <summary>
    /// 获取调试信息
    /// </summary>
    public string GetDebugInfo()
    {
        return $"已加载Bundle: {loadedBundles.Count}\n" +
               $"已缓存预制体: {loadedPrefabs.Count}\n" +
               $"当前路径: {currentPlatformPath}";
    }

    /// <summary>
    /// 获取当前平台信息
    /// </summary>
    public string GetPlatformInfo()
    {
        return $"平台: {Application.platform}, 路径: {currentPlatformPath}, 数据路径: {Application.dataPath}";
    }

    // 日志方法
    private void Log(string message)
    {
        if (enableLogging)
            SeerGoLogger.Log($"[ExternalAssetBundleManager] {message}");
    }

    private void LogError(string message)
    {
        SeerGoLogger.LogError($"[ExternalAssetBundleManager] {message}");
    }

    private void OnDestroy()
    {
        if (this == _instance)
        {
            foreach (var bundle in loadedBundles.Values)
            {
                if (bundle != null)
                {
                    bundle.Unload(true);
                }
            }
            loadedBundles.Clear();
            _instance = null;
        }
    }
}