using UnityEngine;
using System.Collections;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using UnityEngine.Networking;
using SimpleFileBrowser;

public class ZipUtility : MonoBehaviour
{
    private static ZipUtility _instance;
    public static ZipUtility Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("ZipUtility");
                _instance = go.AddComponent<ZipUtility>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    /// <summary>数据根路径（GameAnim/ 和 GameResources/ 的父目录）</summary>
    public static string GetDataRootPath()
    {
#if UNITY_ANDROID
        return Application.persistentDataPath + "/";
#else
        return Path.GetFullPath(Path.Combine(Application.dataPath, "../")) + "/";
#endif
    }

    /// <summary>GameAnim 的根路径</summary>
    public static string GetGameAnimPath()
    {
        return Path.Combine(GetDataRootPath(), "GameAnim/");
    }

    /// <summary>GameResources 的根路径</summary>
    public static string GetGameResourcesPath()
    {
        return Path.Combine(GetDataRootPath(), "GameResources/");
    }

    public static string GetBundlePath()
    {
        return Path.Combine(GetDataRootPath(), "GameAnim/PetFightAnim/");
    }

    public void Unzip(string zipFileName, System.Action<bool> callback)
    {
        StartCoroutine(UnzipRoutine(zipFileName, callback));
    }

    private IEnumerator UnzipRoutine(string zipFileName, System.Action<bool> callback)
    {
        string sourcePath = Path.Combine(Application.streamingAssetsPath, zipFileName);
        string targetPath = GetBundlePath();

        if (!Directory.Exists(targetPath))
            Directory.CreateDirectory(targetPath);

        // 加载ZIP文件
        UnityWebRequest request = UnityWebRequest.Get(sourcePath);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("加载ZIP文件失败: " + request.error);
            callback?.Invoke(false);
            yield break;
        }

        byte[] zipData = request.downloadHandler.data;
        request.Dispose();

        // 主线程解压，避免 IL2CPP 上 Task.Run 不可靠
        bool success = false;
        try
        {
            ExtractAll(zipData, targetPath);
            success = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("解压失败: " + e.Message);
        }

        callback?.Invoke(success);
        yield break;
    }

    /// <summary>
    /// 从指定文件路径解压到目标目录
    /// </summary>
    public void UnzipFromPath(string zipFilePath, string targetPath, System.Action<bool> callback)
    {
        StartCoroutine(UnzipFromPathRoutine(zipFilePath, targetPath, callback));
    }

    private IEnumerator UnzipFromPathRoutine(string zipFilePath, string targetPath, System.Action<bool> callback)
    {
        Debug.Log($"[ZipUtility] Step1-开始解压: {zipFilePath} → {targetPath}");

        // 使用 FileBrowserHelpers 替代 System.IO.File，兼容 Android SAF（参考 SaveSystem）
        if (!FileBrowserHelpers.FileExists(zipFilePath))
        {
            Debug.LogError($"[ZipUtility] Step1-失败: ZIP文件不存在: {zipFilePath}");
            callback?.Invoke(false);
            yield break;
        }
        Debug.Log($"[ZipUtility] Step1-文件存在校验通过");

        if (!Directory.Exists(targetPath))
            Directory.CreateDirectory(targetPath);

        // 使用 FileBrowserHelpers.ReadBytesFromFile 读取，Android 上自动走 SAF 通道
        byte[] zipData = null;
        try
        {
            Debug.Log($"[ZipUtility] Step2-开始读取ZIP...");
            zipData = FileBrowserHelpers.ReadBytesFromFile(zipFilePath);
            Debug.Log($"[ZipUtility] Step2-ZIP读取成功: {zipData.Length} bytes ({zipData.Length / 1024} KB)");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ZipUtility] Step2-读取ZIP文件失败: {e.Message}\n{e.StackTrace}");
            callback?.Invoke(false);
            yield break;
        }

        if (zipData == null || zipData.Length == 0)
        {
            Debug.LogError("[ZipUtility] Step2-失败: ZIP数据为空");
            callback?.Invoke(false);
            yield break;
        }

        // 主线程解压（避免 IL2CPP/Android 上 Task.Run 不可靠）
        bool success = false;
        int entryCount = 0;

        try
        {
            Debug.Log($"[ZipUtility] Step3-开始解压到 {targetPath} ...");
            using (var stream = new MemoryStream(zipData))
            using (var zipStream = new ZipInputStream(stream))
            {
                ZipEntry entry;
                while ((entry = zipStream.GetNextEntry()) != null)
                {
                    if (entry.IsDirectory)
                    {
                        string dirPath = Path.Combine(targetPath, entry.Name);
                        if (!Directory.Exists(dirPath))
                            Directory.CreateDirectory(dirPath);
                        continue;
                    }

                    string filePath = Path.Combine(targetPath, entry.Name);
                    string directory = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    using (var fileStream = File.Create(filePath))
                    {
                        byte[] buffer = new byte[65536];
                        int size;
                        while ((size = zipStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fileStream.Write(buffer, 0, size);
                        }
                    }

                    entryCount++;
                }
            }
            success = true;
            Debug.Log($"[ZipUtility] Step3-解压完成: {entryCount} 个文件");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ZipUtility] Step3-解压异常: {e.Message}\n{e.StackTrace}");
        }

        yield return null;
        Debug.Log($"[ZipUtility] Step4-回调: success={success}");
        callback?.Invoke(success);
    }

    /// <summary>
    /// 从字节数组解压（一次读取，消除重复IO；分帧执行防止卡死）
    /// </summary>
    public void UnzipFromBytes(byte[] zipData, string targetPath, System.Action<bool> callback,
        System.Action<int, int> onProgress = null)
    {
        StartCoroutine(UnzipFromBytesRoutine(zipData, targetPath, callback, onProgress));
    }

    private const int BUFFER_SIZE = 256 * 1024;       // 256KB 缓冲区
    private const int YIELD_INTERVAL = 80;             // 每80个文件 yield 一帧

    private IEnumerator UnzipFromBytesRoutine(byte[] zipData, string targetPath,
        System.Action<bool> callback, System.Action<int, int> onProgress)
    {
        if (zipData == null || zipData.Length == 0)
        {
            Debug.LogError("[ZipUtility] zipData 为空");
            callback?.Invoke(false);
            yield break;
        }

        if (!Directory.Exists(targetPath))
            Directory.CreateDirectory(targetPath);

        Debug.Log($"[ZipUtility] 开始解压 {zipData.Length / 1024 / 1024}MB 到 {targetPath}");

        bool success = false;
        int entryCount = 0;
        int fileCount = 0;
        int yieldCounter = 0;
        string firstError = null;
        byte[] buffer = new byte[BUFFER_SIZE];  // 预分配，不重复 new

        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

        // 不能用 try-catch 包裹 yield return（C# 限制），用标志位追踪错误
        using (var stream = new MemoryStream(zipData))
        using (var zipStream = new ZipInputStream(stream))
        {
            ZipEntry entry;
            while ((entry = zipStream.GetNextEntry()) != null && firstError == null)
            {
                entryCount++;

                // 每 YIELD_INTERVAL 个条目分一帧，防止 UI 卡死
                if (++yieldCounter >= YIELD_INTERVAL)
                {
                    yieldCounter = 0;
                    onProgress?.Invoke(fileCount, entryCount);
                    yield return null;
                }

                if (entry.IsDirectory)
                {
                    try
                    {
                        string dirPath = Path.Combine(targetPath, entry.Name);
                        if (!Directory.Exists(dirPath))
                            Directory.CreateDirectory(dirPath);
                    }
                    catch (System.Exception e) { firstError = e.Message; }
                    continue;
                }

                try
                {
                    string filePath = Path.Combine(targetPath, entry.Name);
                    string directory = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    using (var fileStream = File.Create(filePath))
                    {
                        int size;
                        while ((size = zipStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fileStream.Write(buffer, 0, size);
                        }
                    }

                    fileCount++;
                }
                catch (System.Exception e)
                {
                    firstError = e.Message;
                }
            }
        }

        sw.Stop();

        if (firstError != null)
        {
            Debug.LogError($"[ZipUtility] 解压异常(耗时{sw.Elapsed.TotalSeconds:F1}s, 已解压{fileCount}个文件): {firstError}");
        }
        else
        {
            success = true;
            Debug.Log($"[ZipUtility] 解压完成！{fileCount}个文件 / {entryCount}个条目, 耗时 {sw.Elapsed.TotalSeconds:F1}s");
        }

        onProgress?.Invoke(fileCount, entryCount);
        yield return null;
        callback?.Invoke(success);
    }

    private void ExtractAll(byte[] zipData, string extractPath)
    {
        using (MemoryStream stream = new MemoryStream(zipData))
        using (ZipInputStream zipStream = new ZipInputStream(stream))
        {
            ZipEntry entry;
            byte[] buffer = new byte[BUFFER_SIZE];
            while ((entry = zipStream.GetNextEntry()) != null)
            {
                if (entry.IsDirectory)
                {
                    string dirPath = Path.Combine(extractPath, entry.Name);
                    if (!Directory.Exists(dirPath))
                        Directory.CreateDirectory(dirPath);
                    continue;
                }

                string filePath = Path.Combine(extractPath, entry.Name);
                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                using (FileStream fileStream = File.Create(filePath))
                {
                    int size;
                    while ((size = zipStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fileStream.Write(buffer, 0, size);
                    }
                }
            }
        }
    }

    public static void UnzipFile(string zipFileName, System.Action<bool> callback)
    {
        Instance.Unzip(zipFileName, callback);
    }
}