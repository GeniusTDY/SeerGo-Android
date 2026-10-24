using SeerGoGame.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class ResourcesManager : MonoBehaviour
{
    // 单例实例
    private static ResourcesManager _instance;
    public static ResourcesManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("ResourcesManager");
                _instance = go.AddComponent<ResourcesManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    // 资源缓存字典
    private Dictionary<string, UnityEngine.Object> resourceCache = new Dictionary<string, UnityEngine.Object>();

    // 已加载的资源路径列表
    private List<string> loadedResourcePaths = new List<string>();

    /// <summary>
    /// 已迁移到外部文件系统的资源路径前缀映射
    /// 旧Resources路径 → 新外部路径
    /// </summary>
    private static readonly Dictionary<string, string> ExternalPathMappings = new Dictionary<string, string>()
    {
        { "Sprites/Item/", "Item/" },
        { "Sprites/Maps/bg/", "Maps/bg/" },
        { "Sprites/Maps/Npc/", "Maps/Npc/" },
        { "Sprites/PetFightImage/", "PetFightImage/" },
        { "Sprites/PetImage/", "PetImage/" },
        { "Sprites/Maps/Path/", "Maps/Path/" },
    };

    /// <summary>
    /// 新外部路径前缀集合（无需映射，直接使用）
    /// </summary>
    private static readonly HashSet<string> ExternalPathPrefixes = new HashSet<string>()
    {
        "Item/", "Maps/", "PetFightImage/", "PetImage/", "Audio/"
    };

    /// <summary>
    /// 尝试将路径映射到外部路径，或直接判断是否为外部路径
    /// </summary>
    private static string MapToExternalPath(string path)
    {
        // 1. 检查旧路径映射
        foreach (var kvp in ExternalPathMappings.OrderByDescending(k => k.Key.Length))
        {
            if (path.StartsWith(kvp.Key, StringComparison.OrdinalIgnoreCase))
            {
                return kvp.Value + path.Substring(kvp.Key.Length);
            }
        }
        // 2. 检查是否直接为新外部路径
        foreach (var prefix in ExternalPathPrefixes)
        {
            if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return path; // 已为新路径，直接使用
        }
        return null;
    }

    /// <summary>
    /// 加载资源（带缓存）
    /// 对于迁移到外部的Sprite资源，自动路由到 ExternalResourcesManager
    /// </summary>
    public T Load<T>(string path) where T : UnityEngine.Object
    {
        // 检查缓存
        if (resourceCache.ContainsKey(path))
        {
            return resourceCache[path] as T;
        }

        // Sprite类型：检查是否为已迁移的外部资源
        if (typeof(T) == typeof(Sprite))
        {
            string externalPath = MapToExternalPath(path);
            if (externalPath != null)
            {
                Sprite sprite = ExternalResourcesManager.Instance.LoadSprite(externalPath);
                if (sprite != null)
                {
                    resourceCache[path] = sprite;
                    if (!loadedResourcePaths.Contains(path))
                        loadedResourcePaths.Add(path);
                    return sprite as T;
                }
                Debug.LogError($"[ResourcesManager] 外部资源加载失败: {path} → {externalPath}");
                return null;
            }
        }

        // 加载资源
        T resource = Resources.Load<T>(path);
        if (resource != null)
        {
            resourceCache[path] = resource;
            if (!loadedResourcePaths.Contains(path))
            {
                loadedResourcePaths.Add(path);
            }
            return resource;
        }
        else
        {
            Debug.LogError($"资源加载失败: {path}");
            return null;
        }
    }

    /// <summary>
    /// 加载资源（不带缓存，手动管理释放）
    /// 对于迁移到外部的Sprite资源，自动路由到 ExternalResourcesManager
    /// </summary>
    public T Load<T>(string path, bool cache) where T : UnityEngine.Object
    {
        if (cache)
        {
            return Load<T>(path);
        }
        else
        {
            // Sprite类型：检查是否为已迁移的外部资源
            if (typeof(T) == typeof(Sprite))
            {
                string externalPath = MapToExternalPath(path);
                if (externalPath != null)
                {
                    Sprite sprite = ExternalResourcesManager.Instance.LoadSprite(externalPath);
                    if (sprite == null)
                        Debug.LogError($"资源加载失败: {path}");
                    return sprite as T;
                }
            }

            T resource = Resources.Load<T>(path);
            if (resource == null)
            {
                Debug.LogError($"资源加载失败: {path}");
            }
            return resource;
        }
    }

    /// <summary>
    /// 异步加载资源
    /// 对于迁移到外部的Sprite资源，自动路由到 ExternalResourcesManager
    /// </summary>
    public void LoadAsync<T>(string path, Action<T> callback) where T : UnityEngine.Object
    {
        // Sprite类型：检查是否为已迁移的外部资源
        if (typeof(T) == typeof(Sprite))
        {
            string externalPath = MapToExternalPath(path);
            if (externalPath != null)
            {
                // 检查缓存
                if (resourceCache.ContainsKey(path))
                {
                    callback?.Invoke(resourceCache[path] as T);
                    return;
                }

                ExternalResourcesManager.Instance.LoadSpriteAsync(externalPath, (sprite) =>
                {
                    if (sprite != null)
                    {
                        resourceCache[path] = sprite;
                        if (!loadedResourcePaths.Contains(path))
                            loadedResourcePaths.Add(path);
                        callback?.Invoke(sprite as T);
                    }
                    else
                    {
                        Debug.LogError($"[ResourcesManager] 异步外部资源加载失败: {path}");
                        callback?.Invoke(null);
                    }
                });
                return;
            }
        }

        StartCoroutine(LoadAsyncCoroutine(path, callback));
    }

    private System.Collections.IEnumerator LoadAsyncCoroutine<T>(string path, Action<T> callback) where T : UnityEngine.Object
    {
        // 检查缓存
        if (resourceCache.ContainsKey(path))
        {
            callback?.Invoke(resourceCache[path] as T);
            yield break;
        }

        ResourceRequest request = Resources.LoadAsync<T>(path);
        yield return request;

        if (request.asset != null)
        {
            T resource = request.asset as T;
            resourceCache[path] = resource;
            if (!loadedResourcePaths.Contains(path))
            {
                loadedResourcePaths.Add(path);
            }
            callback?.Invoke(resource);
        }
        else
        {
            Debug.LogError($"异步资源加载失败: {path}");
            callback?.Invoke(null);
        }
    }

    /// <summary>
    /// 安全的卸载单个资源
    /// 外部资源只清除 ResourcesManager 自己的引用，实际对象由 ExternalResourcesManager LRU 管理
    /// </summary>
    public void Release(string path, bool destroyAsset = true)
    {
        if (resourceCache.ContainsKey(path))
        {
            var resource = resourceCache[path];

            if (destroyAsset && resource != null)
            {
                // 外部资源：不销毁实际对象，只清除 ResourcesManager 的引用缓存
                string externalPath = MapToExternalPath(path);
                if (externalPath == null)
                {
                    // 内部 Resources 资源：卸载
                    if (CanUnloadAsset(resource))
                    {
                        Resources.UnloadAsset(resource);
                    }
                    else
                    {
                        SeerGoLogger.LogWarning($"跳过卸载不允许的资源类型: {path}, 类型: {resource.GetType()}");
                    }
                }
                // 外部资源不操作，让 ExternalResourcesManager 的 LRU 自动管理生命周期
            }

            resourceCache.Remove(path);
            loadedResourcePaths.Remove(path);
        }
    }

    /// <summary>
    /// 检查资源是否可以使用 Resources.UnloadAsset
    /// </summary>
    private bool CanUnloadAsset(UnityEngine.Object asset)
    {
        // Resources.UnloadAsset 不能用于以下类型：
        // 1. GameObject (预制体)
        // 2. Component (脚本组件)
        // 3. AssetBundle
        // 4. SceneAsset
        // 可以用于：
        // 1. Texture, Sprite
        // 2. Material
        // 3. AudioClip
        // 4. Mesh
        // 5. TextAsset
        // 6. 其他非GameObject/Component的资源

        var type = asset.GetType();

        // 不允许的类型
        if (asset is GameObject || asset is Component || asset is AssetBundle)
        {
            return false;
        }

        // 如果是脚本资产，也不允许
        if (asset.GetType().Name.Contains("ScriptableObject"))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 释放所有缓存的资源
    /// 外部资源（图片/音频）只清 ResourcesManager 自己的引用缓存，
    /// 实际对象由 ExternalResourcesManager 的 LRU 缓存统一管理生命周期
    /// </summary>
    public void ReleaseAll(bool destroyAssets = true)
    {
        if (destroyAssets)
        {
            // 卸载可以安全卸载的内部 Resources 资源
            foreach (var kvp in resourceCache)
            {
                // 跳过外部资源（由 ExternalResourcesManager LRU 管理，不清除实际对象）
                if (MapToExternalPath(kvp.Key) != null)
                    continue;

                if (kvp.Value != null && CanUnloadAsset(kvp.Value))
                {
                    Resources.UnloadAsset(kvp.Value);
                }
            }
        }

        // 清空引用缓存（外部资源的实际对象仍在 ExternalResourcesManager 中）
        resourceCache.Clear();
        loadedResourcePaths.Clear();

        // 清理所有未使用的资源
        StartCoroutine(UnloadUnusedAssetsCoroutine());
    }

    /// <summary>
    /// 异步卸载未使用的资源
    /// </summary>
    private System.Collections.IEnumerator UnloadUnusedAssetsCoroutine()
    {
        //yield return null; // 等待一帧

        AsyncOperation asyncOperation = Resources.UnloadUnusedAssets();

        while (!asyncOperation.isDone)
        {
            yield return null;
        }

        SeerGoLogger.Log("未使用资源卸载完成");
    }

    /// <summary>
    /// 获取已加载资源数量
    /// </summary>
    public int GetLoadedResourceCount()
    {
        return loadedResourcePaths.Count;
    }

    /// <summary>
    /// 获取已加载资源路径列表
    /// </summary>
    public List<string> GetLoadedResourcePaths()
    {
        return new List<string>(loadedResourcePaths);
    }

    /// <summary>
    /// 切换场景时调用，释放所有资源
    /// </summary>
    public void OnSceneChanged()
    {
        SeerGoLogger.Log($"场景切换，释放资源管理器中的 {GetLoadedResourceCount()} 个资源");
        ReleaseAll(true);
    }

    /// <summary>
    /// 手动卸载未使用的资源
    /// </summary>
    public void UnloadUnusedAssets()
    {
        StartCoroutine(UnloadUnusedAssetsCoroutine());
    }

    private void OnDestroy()
    {
        ReleaseAll(true);
    }
}