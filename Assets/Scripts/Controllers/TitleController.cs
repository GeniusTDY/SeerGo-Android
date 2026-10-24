using ICSharpCode.SharpZipLib.Zip;
using SeerGoGame.Controller;
using SeerGoGame.Models;
using SeerGoGame.Utilities;
using SimpleFileBrowser;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

public class TitleController : BaseController
{
    [Header("Settings")]
    [SerializeField] private Button mLoginButton;
    [SerializeField] private Button importResource;// 导入资源
    [SerializeField] private Button updateResource;// 更新资源
    [SerializeField] private GameObject loadPanel;

    [SerializeField] private Button importAssetSelect;// 导入资源选择
    [SerializeField] private Button importSaveSelect;// 导入存档选择
    [SerializeField] private Button importModSelect;// 导入模组选择

    [SerializeField] private Button importSave;// 导入存档
    [SerializeField] private Button exportSave;// 导出存档
    [SerializeField] private Button BackButton;// 返回按钮
    [Header("声明")]
    [SerializeField] private GameObject DeclarationPanel;
    [SerializeField] private Button ConfirmButtom;

    [SerializeField] private Canvas UICanvas;


    void Start()
    {
        AudioManager.Instance.StopMusic();
        AudioManager.Instance.PlayMusicWithSaveVolume($"Audio/Maps/BGM_000", 10);

        mLoginButton.onClick.AddListener(OnLoginButtonClick);
        ConfirmButtom.onClick.AddListener(() => DeclarationPanel.SetActive(false));

        // ----- 导入子面板按钮监听 -----
        importResource.onClick.AddListener(() => OnImportResourceWithConfirm(false));
        if (updateResource != null)
            updateResource.onClick.AddListener(() => OnImportResourceWithConfirm(true));

        // ----- 选择面板按钮监听 -----
        importAssetSelect.onClick.AddListener(ShowAssetSubPanel);
        importSaveSelect.onClick.AddListener(ShowSaveSubPanel);
        // importModSelect 暂不实现

        // ----- 存档子面板按钮监听 -----
        importSave.onClick.AddListener(OnImportSaveClick);
        exportSave.onClick.AddListener(OnExportSaveClick);

        // ----- 返回按钮监听（两个子面板共用） -----
        BackButton.onClick.AddListener(ShowSelectPanel);

        // ----- 初始状态：显示选择面板 -----
        ShowSelectPanel();

        // 添加快捷链接：游戏数据目录
        FileBrowser.AddQuickLink("游戏数据目录", Application.persistentDataPath);
    }

    #region UI 面板切换

    private void ShowSelectPanel()
    {
        // 选择面板
        importAssetSelect.gameObject.SetActive(true);
        importSaveSelect.gameObject.SetActive(true);
        importModSelect.gameObject.SetActive(true);

        // 资源子面板
        importResource.gameObject.SetActive(false);
        updateResource.gameObject.SetActive(false);

        // 存档子面板
        importSave.gameObject.SetActive(false);
        exportSave.gameObject.SetActive(false);
        BackButton.gameObject.SetActive(false);
    }

    private void ShowAssetSubPanel()
    {
        // 选择面板
        importAssetSelect.gameObject.SetActive(false);
        importSaveSelect.gameObject.SetActive(false);
        importModSelect.gameObject.SetActive(false);

        // 资源子面板
        importResource.gameObject.SetActive(true);
        updateResource.gameObject.SetActive(true);

        // 存档子面板
        importSave.gameObject.SetActive(false);
        exportSave.gameObject.SetActive(false);
        BackButton.gameObject.SetActive(true);
    }

    private void ShowSaveSubPanel()
    {
        // 选择面板
        importAssetSelect.gameObject.SetActive(false);
        importSaveSelect.gameObject.SetActive(false);
        importModSelect.gameObject.SetActive(false);

        // 资源子面板
        importResource.gameObject.SetActive(false);
        updateResource.gameObject.SetActive(false);

        // 存档子面板
        importSave.gameObject.SetActive(true);
        exportSave.gameObject.SetActive(true);
        BackButton.gameObject.SetActive(true);
    }

    #endregion

    /// <summary>
    /// 导入/更新资源：先弹窗确认，确认后才打开文件管理器
    /// </summary>
    private void OnImportResourceWithConfirm(bool overwrite)
    {
        string title = overwrite ? "更新资源" : "导入资源";
        string message = overwrite
            ? "该操作保留现有资源，重复内容自动覆盖，新增内容直接补充，为增量更新。"
            : "该操作会清空当前全部资源并重新导入，仅建议首次登录使用。";

        Dialog1Manager.Instance.ShowPopupOnTransform(transform, title, message, () =>
        {
            OnImportResourceClick(overwrite);
        });
    }

    /// <summary>
    /// 打开文件选择器
    /// 注意：权限管理交给 SimpleFileBrowser 内部处理（它会自动调用 CheckPermission/RequestPermission），
    /// 不要在外部做额外的权限检查，否则会阻止 FileBrowser 在 Android 上显示。
    /// 参考：Seer2-Restart-Lite 的 TitleManager.OnImportResources()
    /// </summary>
    /// <param name="overwrite">true=更新模式（覆盖已有），false=导入模式</param>
    private void OnImportResourceClick(bool overwrite)
    {
        Debug.Log($"[TitleController] 打开文件选择器... (overwrite={overwrite})");
        var filter = new FileBrowser.Filter("ZIP压缩包", ".zip");
        FileBrowser.SetFilters(false, filter);

        string actionText = overwrite ? "更新" : "导入";
        FileBrowser.ShowLoadDialog(
            onSuccess: (paths) => OnResourceSuccess(paths, overwrite),
            onCancel: OnResourceCancel,
            pickMode: FileBrowser.PickMode.FilesAndFolders,
            initialPath: Application.persistentDataPath,
            title: $"选择要{actionText}的资源（手机端请先点击左边的Browse才能浏览）",
            loadButtonText: actionText
        );
    }

    #region 存档导入/导出

    /// <summary>
    /// 获取存档目录路径
    /// </summary>
    private string GetSaveDirectory()
    {
        return Path.Combine(Application.dataPath, "../Saves/");
    }

    /// <summary>
    /// 导入存档：从外部选择 .xml 存档文件，覆盖游戏根目录的 autosave0.xml
    /// </summary>
    private void OnImportSaveClick()
    {
        Debug.Log("[TitleController] 打开存档导入文件选择器...");
        var filter = new FileBrowser.Filter("存档文件", ".xml");
        FileBrowser.SetFilters(false, filter);

        FileBrowser.ShowLoadDialog(
            onSuccess: (paths) =>
            {
                if (paths == null || paths.Length == 0 || string.IsNullOrEmpty(paths[0]))
                {
                    Debug.LogError("[TitleController] 未选择存档文件");
                    return;
                }

                string sourcePath = paths[0];
                string saveDir = GetSaveDirectory();
                if (!Directory.Exists(saveDir))
                    Directory.CreateDirectory(saveDir);

                string destPath = Path.Combine(saveDir, "autosave0.xml");
                Debug.Log($"[TitleController] 导入存档: {sourcePath} → {destPath}");

                try
                {
                    File.Copy(sourcePath, destPath, true);
                    Debug.Log("[TitleController] 存档导入成功");
                    Dialog1Manager.Instance.ShowPopupOnTransform(transform, "导入成功", "存档已导入，请重新进入游戏。");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[TitleController] 存档导入失败: {e.Message}");
                    Dialog1Manager.Instance.ShowPopupOnTransform(transform, "导入失败", e.Message);
                }
            },
            onCancel: () => Debug.Log("[TitleController] 取消存档导入"),
            pickMode: FileBrowser.PickMode.Files,
            initialPath: Application.persistentDataPath,
            title: "选择要导入的存档文件(.xml)",
            loadButtonText: "导入"
        );
    }

    /// <summary>
    /// 导出存档：将 autosave0.xml 导出到平台固定路径（PC→D:\重启号\，安卓→/storage/emulated/0/重启号/）
    /// </summary>
    private void OnExportSaveClick()
    {
        string sourcePath = Path.Combine(GetSaveDirectory(), "autosave0.xml");
        if (!File.Exists(sourcePath))
        {
            Dialog1Manager.Instance.ShowPopupOnTransform(transform, "", "存档文件不存在，无法导出。\n请先开始游戏创建存档。");
            return;
        }

        string exportDir;
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        exportDir = @"D:\重启号\";
#elif UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX
        exportDir = Path.Combine(Application.persistentDataPath, "重启号/");
#elif UNITY_ANDROID
        exportDir = "/storage/emulated/0/重启号/";
#else
        exportDir = Path.Combine(Application.dataPath, "../重启号/");
#endif

        try
        {
            if (!Directory.Exists(exportDir))
                Directory.CreateDirectory(exportDir);

            string destPath = Path.Combine(exportDir, "autosave0.xml");
            File.Copy(sourcePath, destPath, true);
            Debug.Log($"[TitleController] 存档导出成功: {exportDir}");

            Dialog1Manager.Instance.ShowPopupOnTransform(transform, "导出成功", $"存档已导出到：\n{exportDir}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[TitleController] 存档导出失败: {e.Message}");
            Dialog1Manager.Instance.ShowPopupOnTransform(transform, "导出失败", e.Message);
        }
    }

    #endregion

    /// <summary>
    /// 资源选择成功回调（参考 Seer2-Restart-Lite 的 OnResourceSuccess 模式）
    /// </summary>
    private void OnResourceSuccess(string[] paths, bool overwrite)
    {
        // 详细日志：确认回调被触发
        Debug.Log($"[TitleController] OnResourceSuccess 触发！paths数量={paths.Length}, overwrite={overwrite}");
        for (int i = 0; i < paths.Length; i++)
            Debug.Log($"[TitleController]   paths[{i}]={paths[i]}");

        if (paths == null || paths.Length == 0 || string.IsNullOrEmpty(paths[0]))
        {
            Debug.LogError("[TitleController] paths为空！无法继续");
            return;
        }

        string selectedPath = paths[0];
        Debug.Log($"[TitleController] 选中路径: {selectedPath}");
        Debug.Log($"[TitleController] 是ZIP? {selectedPath.EndsWith(".zip", System.StringComparison.OrdinalIgnoreCase)}");

        // 路径校验：不能选择游戏自身数据目录（参考 SaveSystem.TryImportResources）
        if (selectedPath == Application.persistentDataPath ||
            FileBrowserHelpers.IsPathDescendantOfAnother(selectedPath, Application.persistentDataPath))
        {
            Debug.LogWarning("[TitleController] 不能选择游戏自身数据目录");
            SeerGoLogger.LogWarning("需选择外部文件，而非游戏数据目录内的文件");
            return;
        }

        string action = overwrite ? "更新" : "导入";
        Debug.Log($"[TitleController] 操作类型: {action}");

        // 处理ZIP文件
        if (selectedPath.EndsWith(".zip", System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log("[TitleController] → 进入 ZIP 导入分支");
            HandleZipImport(selectedPath, overwrite);
            return;
        }

        // 处理文件夹导入
        Debug.Log("[TitleController] → 进入文件夹导入分支");
        HandleFolderImport(selectedPath, action, overwrite);
    }

    /// <summary>
    /// ZIP内容类型枚举（用于导入后的提示）
    /// </summary>
    private enum ZipContentType
    {
        Unknown,
        GameAnim,
        GameResources,
        Both
    }

    /// <summary>
    /// 处理ZIP文件导入（优化版：一次读取字节，检测内容+解压共用，消除重复IO）
    /// </summary>
    private void HandleZipImport(string zipPath, bool overwrite)
    {
        string action = overwrite ? "更新" : "导入";
        Debug.Log($"[TitleController] 开始处理ZIP: {zipPath}");

        // 一次读取ZIP字节（之前 DetectZipContent 和 UnzipFromPath 各读了一遍）
        byte[] zipData = null;
        try
        {
            Debug.Log("[TitleController] 读取ZIP文件...");
            zipData = FileBrowserHelpers.ReadBytesFromFile(zipPath);
            Debug.Log($"[TitleController] ZIP读取完成: {zipData.Length / 1024 / 1024}MB");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[TitleController] 读取ZIP失败: {e.Message}");
            SeerGoLogger.LogError("读取ZIP文件失败: " + e.Message);
            return;
        }

        if (zipData == null || zipData.Length == 0)
        {
            Debug.LogError("[TitleController] ZIP数据为空");
            SeerGoLogger.LogError("ZIP文件为空");
            return;
        }

        // 从内存检测内容类型（不额外读文件）
        var contentType = DetectZipContentFromBytes(zipData);
        Debug.Log($"[TitleController] ZIP内容类型: {contentType}");

        loadPanel.SetActive(true);

        string targetPath = ZipUtility.GetDataRootPath();
        Debug.Log($"[TitleController] 解压目标: {targetPath}");

        // 导入模式：先删除已有文件夹再全新解压
        // 更新模式：不删除，解压时 File.Create 自然覆盖同名 + 新增不存在的
        if (!overwrite)
        {
            TryDeleteExistingFolders(targetPath, contentType);
        }

        // 用已读取的字节解压（不再重复读文件）
        ZipUtility.Instance.UnzipFromBytes(zipData, targetPath, (success) =>
        {
            loadPanel.SetActive(false);

            if (success)
            {
                string msg = $"{action}成功！";
                switch (contentType)
                {
                    case ZipContentType.GameAnim:
                        msg += "\n已导入游戏动画资源（GameAnim）";
                        break;
                    case ZipContentType.GameResources:
                        msg += "\n已导入游戏基础资源（GameResources）";
                        break;
                    case ZipContentType.Both:
                        msg += "\n已导入游戏动画和基础资源";
                        break;
                    default:
                        msg += "\n资源已解压到游戏数据目录";
                        break;
                }
                Debug.Log($"[TitleController] {msg}");
                Dialog1Manager.Instance.ShowPopupOnTransform(transform, $"{action}成功", msg);
            }
            else
            {
                string errMsg = $"资源{action}失败，请检查ZIP文件是否完整";
                Debug.LogError($"[TitleController] {errMsg}");
                Dialog1Manager.Instance.ShowPopupOnTransform(transform, $"{action}失败", errMsg);
            }
        });
    }

    /// <summary>
    /// 从内存字节检测ZIP内容类型（避免额外读文件）
    /// </summary>
    private ZipContentType DetectZipContentFromBytes(byte[] zipData)
    {
        bool hasGameAnim = false;
        bool hasGameResources = false;

        try
        {
            using (var stream = new MemoryStream(zipData))
            using (var zipStream = new ZipInputStream(stream))
            {
                ZipEntry entry;
                while ((entry = zipStream.GetNextEntry()) != null)
                {
                    string name = entry.Name.Replace('\\', '/');
                    if (name.StartsWith("GameAnim/", System.StringComparison.OrdinalIgnoreCase) ||
                        name.Equals("GameAnim", System.StringComparison.OrdinalIgnoreCase))
                        hasGameAnim = true;
                    if (name.StartsWith("GameResources/", System.StringComparison.OrdinalIgnoreCase) ||
                        name.Equals("GameResources", System.StringComparison.OrdinalIgnoreCase))
                        hasGameResources = true;

                    if (hasGameAnim && hasGameResources)
                        break;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[TitleController] ZIP内容检测失败: {e.Message}");
            return ZipContentType.Unknown;
        }

        if (hasGameAnim && hasGameResources) return ZipContentType.Both;
        if (hasGameAnim) return ZipContentType.GameAnim;
        if (hasGameResources) return ZipContentType.GameResources;
        return ZipContentType.Unknown;
    }

    /// <summary>
    /// 处理文件夹导入（使用FileBrowserHelpers跨平台API）
    /// 导入与更新行为一致：先删除目标目录再全量复制（CopyDirectory 不支持单文件增量）
    /// </summary>
    private void HandleFolderImport(string sourcePath, string action, bool overwrite)
    {
        string folderName = FileBrowserHelpers.GetFilename(sourcePath);
        string destPath = Path.Combine(ZipUtility.GetDataRootPath(), folderName);

        Debug.Log($"[TitleController] {action}文件夹: {sourcePath} -> {destPath}");

        loadPanel.SetActive(true);

        try
        {
            // 删除已有目录（导入/更新都删，CopyDirectory 是目录级复制不支持增量）
            if (FileBrowserHelpers.DirectoryExists(destPath))
            {
                Debug.Log($"[TitleController] {action}模式：删除已有目录 {folderName}");
                FileBrowserHelpers.DeleteDirectory(destPath);
            }

            FileBrowserHelpers.CopyDirectory(sourcePath, destPath);

            loadPanel.SetActive(false);

            string msg = $"{action}成功！已{action}文件夹: {folderName}";
            Debug.Log($"[TitleController] {msg}");
            Dialog1Manager.Instance.ShowPopupOnTransform(transform, $"{action}成功", msg);
        }
        catch (System.Exception e)
        {
            loadPanel.SetActive(false);

            string msg = $"{action}失败: {e.Message}";
            Debug.LogError($"[TitleController] {msg}");
            Dialog1Manager.Instance.ShowPopupOnTransform(transform, $"{action}失败", msg);
        }
    }

    /// <summary>
    /// 导入模式：先删除已有目录再全新解压（参考 SaveSystem.TryUnzipFile 的 DeleteDirectory 逻辑）
    /// </summary>
    private void TryDeleteExistingFolders(string targetPath, ZipContentType contentType)
    {
        try
        {
            if (contentType == ZipContentType.GameAnim || contentType == ZipContentType.Both)
            {
                string animPath = ZipUtility.GetGameAnimPath();
                if (FileBrowserHelpers.DirectoryExists(animPath))
                {
                    Debug.Log($"[TitleController] 导入模式：删除已有 GameAnim");
                    FileBrowserHelpers.DeleteDirectory(animPath);
                }
            }
            if (contentType == ZipContentType.GameResources || contentType == ZipContentType.Both)
            {
                string resPath = ZipUtility.GetGameResourcesPath();
                if (FileBrowserHelpers.DirectoryExists(resPath))
                {
                    Debug.Log($"[TitleController] 导入模式：删除已有 GameResources");
                    FileBrowserHelpers.DeleteDirectory(resPath);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[TitleController] 删除已有目录时异常: {e.Message}");
        }
    }

    /// <summary>
    /// 用户取消选择回调
    /// </summary>
    private void OnResourceCancel()
    {
        Debug.Log("[TitleController] 用户取消选择");
    }

    public void OnLoginButtonClick()
    {
        string animPath = ZipUtility.GetGameAnimPath();
        string resPath = ZipUtility.GetGameResourcesPath();

        Debug.Log($"[TitleController] 检查资源: anim={animPath}, res={resPath}");

        bool hasAnim = Directory.Exists(animPath) && Directory.GetFiles(animPath, "*", SearchOption.AllDirectories).Length > 0;
        bool hasRes = Directory.Exists(resPath) && Directory.GetFiles(resPath, "*", SearchOption.AllDirectories).Length > 0;

        Debug.Log($"[TitleController] hasAnim={hasAnim}(exists={Directory.Exists(animPath)}), hasRes={hasRes}(exists={Directory.Exists(resPath)})");

        if (!hasAnim || !hasRes)
        {
            string msg = "";
            if (!hasAnim) msg += "GameAnim 目录为空或不存在\n";
            if (!hasRes) msg += "GameResources 目录为空或不存在\n";
            msg += "请点击\"导入资源\"按钮导入 GameAnim.zip 和 GameResources.zip";
            SeerGoLogger.LogWarning(msg);
            return;
        }

        FirstLoginHelper.SetStatus(true);
        SceneManager.LoadScene("Maps");
    }

    private void OnApplicationQuit()
    {
        // this.GetSystem<IArchiveSystem>().SaveGame("autosave", CollectCurrentData());
    }

    private ArchiveData CollectCurrentData()
    {
        return new ArchiveData
        {
            Nickname = "小赛尔",
            PetTotalExp = 999999,
            ItemStorage =
        {
            new ItemData { Id = 1, Num = 5000 },
            new ItemData { Id = 2, Num = 5000 }
        }
        };
    }
}
