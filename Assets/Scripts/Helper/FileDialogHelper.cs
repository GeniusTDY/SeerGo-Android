using System;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// 跨平台文件选择器
/// Editor: UnityEditor.EditorUtility.OpenFilePanel
/// Android: 原生文件选择 Intent
/// Windows Standalone: 原生 Win32 文件对话框
/// </summary>
public static class FileDialogHelper
{
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    // Windows Standalone: 使用原生 Win32 文件对话框
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct OpenFileName
    {
        public int lStructSize;
        public IntPtr hwndOwner;
        public IntPtr hInstance;
        public string lpstrFilter;
        public string lpstrCustomFilter;
        public int nMaxCustFilter;
        public int nFilterIndex;
        public string lpstrFile;
        public int nMaxFile;
        public string lpstrFileTitle;
        public int nMaxFileTitle;
        public string lpstrInitialDir;
        public string lpstrTitle;
        public int Flags;
        public short nFileOffset;
        public short nFileExtension;
        public string lpstrDefExt;
        public IntPtr lCustData;
        public IntPtr lpfnHook;
        public string lpTemplateName;
        public IntPtr pvReserved;
        public int dwReserved;
        public int flagsEx;
    }

    [DllImport("Comdlg32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool GetOpenFileName(ref OpenFileName ofn);

    private static string OpenFileDialogWindows(string title, string filter)
    {
        var ofn = new OpenFileName();
        ofn.lStructSize = Marshal.SizeOf(ofn);
        ofn.lpstrFilter = filter.Replace("|", "\0") + "\0\0";
        ofn.lpstrFile = new string('\0', 512);
        ofn.nMaxFile = 512;
        ofn.lpstrTitle = title;
        ofn.Flags = 0x00080000 | 0x00001000 | 0x00000800; // OFN_EXPLORER | OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST

        if (GetOpenFileName(ref ofn))
        {
            return ofn.lpstrFile.TrimEnd('\0');
        }
        return null;
    }
#endif

    // Android 回调相关字段（所有平台可见，供 FileDialogAndroidReceiver 调用）
    private static Action<string> _pendingCallback;

    /// <summary>注册文件选择回调（Android异步流程使用）</summary>
    public static void RegisterCallback(Action<string> callback)
    {
        _pendingCallback = callback;
    }

    /// <summary>文件选择完成入口（FileDialogAndroidReceiver 调用）</summary>
    public static void OnFileSelected(string filePath)
    {
        var cb = _pendingCallback;
        _pendingCallback = null;
        cb?.Invoke(filePath);
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaClass _unityPlayer;
    private static AndroidJavaObject _currentActivity;
    private const int PICK_FILE_REQUEST_CODE = 10001;

    private static void EnsureAndroidRefs()
    {
        if (_unityPlayer == null)
            _unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        if (_currentActivity == null)
            _currentActivity = _unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    }

    /// <summary>
    /// Android: 打开系统文件管理器选择zip文件，结果通过 RegisterCallback 回传
    /// </summary>
    public static void OpenFileDialogAndroid(string title, string[] mimeTypes)
    {
        EnsureAndroidRefs();

        if (_currentActivity == null)
        {
            Debug.LogError("[FileDialog] 无法获取 Android Activity");
            _pendingCallback?.Invoke(null);
            _pendingCallback = null;
            return;
        }

        try
        {
            using (var intent = new AndroidJavaObject("android.content.Intent", "android.intent.action.OPEN_DOCUMENT"))
            {
                intent.Call<AndroidJavaObject>("addCategory", "android.intent.category.OPENABLE");
                intent.Call<AndroidJavaObject>("setType", mimeTypes != null && mimeTypes.Length > 0 ? mimeTypes[0] : "*/*");

                if (mimeTypes != null && mimeTypes.Length > 1)
                {
                    using (var extraMimes = new AndroidJavaClass("java.lang.String"))
                    {
                        intent.Call<AndroidJavaObject>("putExtra", "android.intent.extra.MIME_TYPES", mimeTypes);
                    }
                }

                intent.Call<AndroidJavaObject>("putExtra", "android.intent.extra.ALLOW_MULTIPLE", false);
                _currentActivity.Call("startActivityForResult", intent, PICK_FILE_REQUEST_CODE);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[FileDialog] Android 文件选择失败: {e.Message}");
            _pendingCallback?.Invoke(null);
            _pendingCallback = null;
        }
    }
#endif

    /// <summary>
    /// 打开文件选择对话框（同步/异步适配）
    /// Editor: 同步返回路径
    /// Android: 异步，通过 callback 返回
    /// </summary>
    public static void OpenFileDialog(string title, string extension, string extensionDesc, Action<string> onFileSelected)
    {
#if UNITY_EDITOR
        // Editor: 同步
        string path = UnityEditor.EditorUtility.OpenFilePanel(title, "", extension);
        onFileSelected?.Invoke(string.IsNullOrEmpty(path) ? null : path);
#elif UNITY_ANDROID
        // Android: 异步 Intent
        RegisterCallback(onFileSelected);
        string[] mimeTypes = GetMimeTypes(extension);
        OpenFileDialogAndroid(title, mimeTypes);
#elif UNITY_STANDALONE_WIN
        // Windows Standalone: 原生对话框
        string filter = $"{extensionDesc} (*.{extension})\0*.{extension}\0";
        string winPath = OpenFileDialogWindows(title, filter);
        onFileSelected?.Invoke(winPath);
#else
        Debug.LogError($"[FileDialog] 不支持当前平台: {Application.platform}");
        onFileSelected?.Invoke(null);
#endif
    }

    private static string[] GetMimeTypes(string extension)
    {
        return extension.ToLower() switch
        {
            "zip" => new[] { "application/zip", "application/x-zip-compressed" },
            "png" => new[] { "image/png" },
            "jpg" or "jpeg" => new[] { "image/jpeg" },
            "mp3" => new[] { "audio/mpeg" },
            _ => new[] { "*/*" }
        };
    }
}
