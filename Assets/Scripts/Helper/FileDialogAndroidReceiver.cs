using UnityEngine;

/// <summary>
/// Android 文件选择回调接收器
/// 接收原生 Activity 的 onActivityResult，转换为 Unity 路径后回调 FileDialogHelper
/// </summary>
public class FileDialogAndroidReceiver : MonoBehaviour
{
    private static FileDialogAndroidReceiver _instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (_instance == null)
        {
            var go = new GameObject("FileDialogAndroidReceiver");
            _instance = go.AddComponent<FileDialogAndroidReceiver>();
            DontDestroyOnLoad(go);
        }
    }

    /// <summary>
    /// 由 Android 原生插件调用的入口
    /// 参数: contentUriString - Android content:// URI 字符串
    /// </summary>
    public void OnAndroidFileSelected(string contentUriString)
    {
        if (string.IsNullOrEmpty(contentUriString))
        {
            Debug.LogWarning("[FileDialogReceiver] 收到空的文件URI");
            FileDialogHelper.OnFileSelected(null);
            return;
        }

        Debug.Log($"[FileDialogReceiver] 收到文件URI: {contentUriString}");

        // 将 content:// URI 转换为实际文件路径并复制到可访问位置
        StartCoroutine(CopyUriToAccessiblePath(contentUriString));
    }

    private System.Collections.IEnumerator CopyUriToAccessiblePath(string contentUriString)
    {
        string resultPath = null;

#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var uriClass = new AndroidJavaClass("android.net.Uri"))
            using (var uri = uriClass.CallStatic<AndroidJavaObject>("parse", contentUriString))
            using (var resolver = activity.Call<AndroidJavaObject>("getContentResolver"))
            {
                // 获取文件名
                string fileName = "imported.zip";
                using (var cursor = resolver.Call<AndroidJavaObject>("query", uri, null, null, null, null))
                {
                    if (cursor != null && cursor.Call<bool>("moveToFirst"))
                    {
                        int nameIndex = cursor.Call<int>("getColumnIndex", "_display_name");
                        if (nameIndex >= 0)
                            fileName = cursor.Call<string>("getString", nameIndex);
                        cursor.Call("close");
                    }
                }

                // 复制到临时目录
                string tempDir = System.IO.Path.Combine(Application.persistentDataPath, "temp");
                if (!System.IO.Directory.Exists(tempDir))
                    System.IO.Directory.CreateDirectory(tempDir);

                string tempPath = System.IO.Path.Combine(tempDir, fileName);

                using (var inputStream = resolver.Call<AndroidJavaObject>("openInputStream", uri))
                {
                    if (inputStream == null)
                    {
                        Debug.LogError("[FileDialogReceiver] 无法打开输入流");
                        FileDialogHelper.OnFileSelected(null);
                        yield break;
                    }

                    byte[] buffer = new byte[8192];
                    using (var fileStream = System.IO.File.Create(tempPath))
                    {
                        int bytesRead;
                        while (true)
                        {
                            bytesRead = inputStream.Call<int>("read", buffer);
                            if (bytesRead <= 0) break;
                            fileStream.Write(buffer, 0, bytesRead);
                        }
                    }
                }

                resultPath = tempPath;
                Debug.Log($"[FileDialogReceiver] 文件已复制到: {resultPath}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FileDialogReceiver] 复制文件失败: {e.Message}");
        }
#endif

        FileDialogHelper.OnFileSelected(resultPath);
        yield return null;
    }


    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
