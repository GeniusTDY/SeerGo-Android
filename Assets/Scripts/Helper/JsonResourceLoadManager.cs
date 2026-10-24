using SeerGoGame.Utilities;
using System;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 资源加载结果封装
/// </summary>
/// <typeparam name="T">解析后的数据类型</typeparam>
public class ResourceLoadResult<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
    public string ErrorMessage { get; set; }
    public TextAsset LoadedTextAsset { get; set; }
}

/// <summary>
/// 资源加载管理器
/// </summary>
public static class JsonResourceLoadManager
{
    /// <summary>
    /// 异步加载并解析资源配置文件 (async/await模式)
    /// </summary>
    /// <typeparam name="T">目标解析类型</typeparam>
    /// <param name="relativePath">相对Resources的路径（不含后缀）</param>
    /// <param name="autoUnload">是否自动释放TextAsset资源</param>
    /// <returns>加载结果</returns>
    public static async Task<ResourceLoadResult<T>> LoadConfigAsync<T>(string relativePath, bool autoUnload = true) where T : class
    {
        var result = new ResourceLoadResult<T>();
        TextAsset textAsset = null;

        try
        {
            // 异步加载TextAsset
            var request = Resources.LoadAsync<TextAsset>(relativePath);

            // 等待加载完成（支持进度查询）
            while (!request.isDone)
            {
                await Task.Yield(); // 每帧检查一次，避免阻塞主线程
            }

            textAsset = request.asset as TextAsset;
            result.LoadedTextAsset = textAsset;

            if (textAsset == null)
            {
                result.Success = false;
                result.ErrorMessage = $"找不到配置文件: {relativePath}";
                SeerGoLogger.LogError($"ResourceLoadManager: {result.ErrorMessage}");
                return result;
            }

            // 解析JSON数据
            T parsedData = JsonUtility.FromJson<T>(textAsset.text);
            if (parsedData == null)
            {
                result.Success = false;
                result.ErrorMessage = $"JSON解析失败: {relativePath}";
                SeerGoLogger.LogError($"ResourceLoadManager: {result.ErrorMessage}");
                return result;
            }

            result.Success = true;
            result.Data = parsedData;

           // SeerGoLogger.Log($"ResourceLoadManager: 成功加载并解析配置文件: {relativePath}");

            return result;
        }
        catch (System.Exception e)
        {
            result.Success = false;
            result.ErrorMessage = $"加载配置文件时发生异常: {e.Message}";
            SeerGoLogger.LogError($"ResourceLoadManager: {result.ErrorMessage}");
            return result;
        }
        finally
        {
            // 根据参数决定是否释放TextAsset资源
            if (autoUnload && textAsset != null)
            {
                Resources.UnloadAsset(textAsset);
               // SeerGoLogger.Log($"ResourceLoadManager: 已释放TextAsset资源: {relativePath}");
            }
        }
    }

    /// <summary>
    /// 异步加载并解析资源配置文件 (回调模式)
    /// </summary>
    /// <typeparam name="T">目标解析类型</typeparam>
    /// <param name="relativePath">相对Resources的路径（不含后缀）</param>
    /// <param name="onComplete">完成回调</param>
    /// <param name="autoUnload">是否自动释放TextAsset资源</param>
    public static async void LoadConfigAsync<T>(string relativePath, Action<ResourceLoadResult<T>> onComplete, bool autoUnload = true) where T : class
    {
        var result = await LoadConfigAsync<T>(relativePath, autoUnload);
        onComplete?.Invoke(result);
    }

    /// <summary>
    /// 手动释放TextAsset资源（用于autoUnload=false的情况）
    /// </summary>
    /// <param name="textAsset">要释放的TextAsset资源</param>
    public static void UnloadTextAsset(TextAsset textAsset)
    {
        if (textAsset != null)
        {
            Resources.UnloadAsset(textAsset);
            SeerGoLogger.Log("ResourceLoadManager: 手动释放TextAsset资源");
        }
    }
}