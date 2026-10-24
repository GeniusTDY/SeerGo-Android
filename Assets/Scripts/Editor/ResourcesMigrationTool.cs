using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 资源迁移编辑器工具 - 将 Assets/Resources 中的图片和音频迁移到外部 GameResources/ 目录
/// 
/// 迁移映射:
///   Assets/Resources/Audio/              → {ProjectRoot}/GameResources/Audio/
///   Assets/Resources/Sprites/Item/       → {ProjectRoot}/GameResources/Item/
///   Assets/Resources/Sprites/Maps/bg/    → {ProjectRoot}/GameResources/Maps/bg/
///   Assets/Resources/Sprites/Maps/Npc/   → {ProjectRoot}/GameResources/Maps/Npc/
///   Assets/Resources/Sprites/Maps/Path/  → {ProjectRoot}/GameResources/Maps/Path/
///   Assets/Resources/Sprites/PetFightImage/ → {ProjectRoot}/GameResources/PetFightImage/
///   Assets/Resources/Sprites/PetImage/   → {ProjectRoot}/GameResources/PetImage/
/// </summary>
public class ResourcesMigrationTool : EditorWindow
{
    private static readonly string[] SourceDirs = new string[]
    {
        "Assets/Resources/Audio",
        "Assets/Resources/Sprites/Item",
        "Assets/Resources/Sprites/Maps/bg",
        "Assets/Resources/Sprites/Maps/Npc",
        "Assets/Resources/Sprites/Maps/Path",
        "Assets/Resources/Sprites/PetFightImage",
        "Assets/Resources/Sprites/PetImage",
    };

    private static readonly string[] TargetDirs = new string[]
    {
        "GameResources/Audio",
        "GameResources/Item",
        "GameResources/Maps/bg",
        "GameResources/Maps/Npc",
        "GameResources/Maps/Path",
        "GameResources/PetFightImage",
        "GameResources/PetImage",
    };

    private Vector2 scrollPos;
    private string logText = "";

    [MenuItem("Tools/资源迁移/打开迁移工具窗口")]
    public static void ShowWindow()
    {
        var window = GetWindow<ResourcesMigrationTool>("资源迁移工具");
        window.minSize = new Vector2(600, 400);
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("资源迁移工具", EditorStyles.boldLabel);
        GUILayout.Label("将 Assets/Resources 中的图片/音频迁移到项目根目录 Resources/ 文件夹");
        GUILayout.Space(10);

        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        GUILayout.Label($"项目根目录: {projectRoot}");
        GUILayout.Label($"目标根目录: {Path.Combine(projectRoot, "GameResources")}");
        GUILayout.Space(10);

        if (GUILayout.Button("预览迁移计划", GUILayout.Height(30)))
        {
            PreviewMigration();
        }

        GUILayout.Space(5);

        GUI.enabled = EditorApplication.isPlaying ? false : true;
        if (GUILayout.Button("执行迁移（复制文件到外部 Resources/）", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("确认迁移",
                "将会把以下目录的文件复制到项目根目录下的 Resources/ 文件夹:\n\n" +
                "• Audio/ (MP3音频)\n" +
                "• Sprites/Item/ (物品图标)\n" +
                "• Sprites/Maps/bg/ (地图背景)\n" +
                "• Sprites/Maps/Npc/ (NPC精灵)\n" +
                "• Sprites/Maps/Path/ (地图遮罩)\n" +
                "• Sprites/PetFightImage/ (战斗精灵)\n" +
                "• Sprites/PetImage/ (精灵图片)\n\n" +
                "迁移后请注意验证代码中的引用是否正常。\n" +
                "是否继续？", "确认迁移", "取消"))
            {
                ExecuteMigration();
            }
        }
        GUI.enabled = true;

        GUILayout.Space(5);

        if (GUILayout.Button("删除 Assets/Resources 中的已迁移资源（谨慎！）", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("危险操作！",
                "此操作将删除 Assets/Resources/ 中已被迁移到外部的原始文件。\n" +
                "请确保已经在外部分目录验证文件完整无误。\n\n" +
                "是否继续？", "确认删除", "取消"))
            {
                DeleteMigratedResources();
            }
        }

        GUILayout.Space(10);
        GUILayout.Label("迁移日志:", EditorStyles.boldLabel);
        scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));
        GUILayout.Label(logText, EditorStyles.wordWrappedLabel);
        GUILayout.EndScrollView();
    }

    private void PreviewMigration()
    {
        logText = "=== 预览迁移计划 ===\n\n";
        int totalFiles = 0;
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        for (int i = 0; i < SourceDirs.Length; i++)
        {
            string sourceDir = Path.Combine(projectRoot, SourceDirs[i].Replace("/", "\\"));
            string targetDir = Path.Combine(projectRoot, TargetDirs[i].Replace("/", "\\"));

            AppendLog($"来源: {SourceDirs[i]}");
            AppendLog($"目标: {TargetDirs[i]}");

            if (Directory.Exists(sourceDir))
            {
                var files = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);
                // 排除 .meta 文件
                int fileCount = 0;
                long totalSize = 0;
                foreach (var f in files)
                {
                    if (f.EndsWith(".meta")) continue;
                    var fi = new FileInfo(f);
                    fileCount++;
                    totalSize += fi.Length;
                }
                AppendLog($"  文件数量: {fileCount}, 总大小: {FormatSize(totalSize)}");
                totalFiles += fileCount;
            }
            else
            {
                AppendLog($"  [警告] 来源目录不存在!");
            }
            AppendLog("");
        }

        AppendLog($"总计文件数: {totalFiles}");
        AppendLog($"目标路径: {Path.Combine(projectRoot, "GameResources")}");
        AppendLog("\n注意: 此操作仅复制文件，不会删除原始文件。");
    }

    private void ExecuteMigration()
    {
        logText = "=== 执行迁移 ===\n\n";
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        int totalCopied = 0;
        int totalSkipped = 0;
        int totalErrors = 0;

        EditorUtility.DisplayProgressBar("资源迁移", "正在复制文件...", 0f);

        try
        {
            for (int i = 0; i < SourceDirs.Length; i++)
            {
                string sourceDir = Path.Combine(projectRoot, SourceDirs[i].Replace("/", "\\"));
                string targetDir = Path.Combine(projectRoot, TargetDirs[i].Replace("/", "\\"));

                EditorUtility.DisplayProgressBar("资源迁移",
                    $"正在迁移: {SourceDirs[i]}", (float)i / SourceDirs.Length);

                if (!Directory.Exists(sourceDir))
                {
                    AppendLog($"[跳过] 来源目录不存在: {SourceDirs[i]}");
                    totalSkipped++;
                    continue;
                }

                // 确保目标目录存在
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                var files = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);
                foreach (var sourceFile in files)
                {
                    if (sourceFile.EndsWith(".meta")) continue;

                    // 计算相对路径保持目录结构
                    string relativePath = Path.GetRelativePath(sourceDir, sourceFile);
                    string destFile = Path.Combine(targetDir, relativePath);

                    // 确保目标子目录存在
                    string destDir = Path.GetDirectoryName(destFile);
                    if (!Directory.Exists(destDir))
                    {
                        Directory.CreateDirectory(destDir);
                    }

                    try
                    {
                        if (!File.Exists(destFile) || new FileInfo(sourceFile).LastWriteTime > new FileInfo(destFile).LastWriteTime)
                        {
                            File.Copy(sourceFile, destFile, true);
                            totalCopied++;
                        }
                        else
                        {
                            totalSkipped++;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        AppendLog($"[错误] 复制失败: {relativePath} - {ex.Message}");
                        totalErrors++;
                    }
                }

                AppendLog($"[完成] {SourceDirs[i]} → {TargetDirs[i]}");
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        AppendLog("");
        AppendLog($"=== 迁移完成 ===");
        AppendLog($"复制: {totalCopied}, 跳过(已存在): {totalSkipped}, 错误: {totalErrors}");
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("迁移完成",
            $"文件迁移完成！\n\n复制: {totalCopied}\n跳过: {totalSkipped}\n错误: {totalErrors}\n\n" +
            $"文件已复制到: {Path.Combine(projectRoot, "GameResources")}",
            "确定");
    }

    private void DeleteMigratedResources()
    {
        logText = "=== 删除已迁移资源 ===\n\n";
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        int totalDeleted = 0;

        EditorUtility.DisplayProgressBar("删除资源", "正在删除...", 0f);

        try
        {
            for (int i = 0; i < SourceDirs.Length; i++)
            {
                string sourceDir = Path.Combine(projectRoot, SourceDirs[i].Replace("/", "\\"));

                EditorUtility.DisplayProgressBar("删除资源",
                    $"正在删除: {SourceDirs[i]}", (float)i / SourceDirs.Length);

                if (!Directory.Exists(sourceDir))
                {
                    AppendLog($"[跳过] 目录不存在: {SourceDirs[i]}");
                    continue;
                }

                // 删除所有非 .meta 文件
                var files = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    if (file.EndsWith(".meta")) continue;
                    try
                    {
                        File.Delete(file);
                        totalDeleted++;
                    }
                    catch (System.Exception ex)
                    {
                        AppendLog($"[错误] 删除失败: {file} - {ex.Message}");
                    }
                }

                // 删除空目录
                DeleteEmptyDirectories(sourceDir);

                AppendLog($"[完成] 已删除: {SourceDirs[i]}");
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        AppendLog($"\n共删除 {totalDeleted} 个文件");
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("删除完成",
            $"已删除 {totalDeleted} 个文件。\n\n" +
            "注意: .meta 文件已被保留，如需彻底清理请在 Unity 中手动删除对应目录。",
            "确定");
    }

    private void DeleteEmptyDirectories(string startDir)
    {
        foreach (var dir in Directory.GetDirectories(startDir))
        {
            DeleteEmptyDirectories(dir);
        }

        if (Directory.GetFiles(startDir).Length == 0 &&
            Directory.GetDirectories(startDir).Length == 0)
        {
            Directory.Delete(startDir);
        }
    }

    [MenuItem("Tools/资源迁移/直接执行迁移（无确认）")]
    private static void QuickMigrate()
    {
        var window = GetWindow<ResourcesMigrationTool>();
        window.ExecuteMigration();
    }

    private void AppendLog(string msg)
    {
        logText += msg + "\n";
        Debug.Log($"[资源迁移] {msg}");
    }

    private static string FormatSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024.0):F1} MB";
        return $"{bytes / (1024.0 * 1024.0 * 1024.0):F2} GB";
    }
}
