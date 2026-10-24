using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FTRuntime;

/// <summary>
/// 批量扫描 Resources/PetWalk 目录下的 .fla.pet 文件（全词匹配，排除其他文件），
/// 生成带有 PetWalkController 的预制体，统一输出到同级目录。
///
/// 目录结构:
///   输入:  Assets/Resources/PetWalk/{name}.fla.pet   (如 1.fla.pet, 2.fla.pet)
///   输出:  Assets/GeneratedWalkPrefabs/Pet_{name}.prefab
///
/// 每个预制体自动挂载:
///   - SwfClip (引用对应 .fla.pet 资产)
///   - SwfClipController (autoPlay=false, loop=true)
///   - SwfClipFrameEvents
///   - PetWalkController
/// </summary>
public class PetWalkBatchGenerator : EditorWindow
{
    // =========================================================================
    // EditorWindow 字段
    // =========================================================================

    private string _sourceDir  = "Assets/Resources/PetWalk";
    private string _outputDir  = "Assets/GeneratedWalkPrefabs";
    private float  _walkSpeed  = 2f;
    private float  _walkRadius = 3f;
    private Vector2 _scrollPos;

    private List<WalkFileInfo> _scannedFiles;

    private struct WalkFileInfo
    {
        public string Name;       // 如 "1"（从 "1.fla.pet" 提取）
        public string AssetPath;  // 如 "Assets/Resources/PetWalk/1.fla.pet"
        public SwfClipAsset Asset;
        public string[] Sequences;
    }

    // =========================================================================
    // EditorWindow
    // =========================================================================

    [MenuItem("Tools/PetWalk 批量生成行走预制体")]
    public static void ShowWindow()
    {
        var window = GetWindow<PetWalkBatchGenerator>("PetWalk 批量生成");
        window.minSize = new Vector2(520, 460);
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("批量扫描 PetWalk 下的 .fla.pet.asset，生成行走预制体", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // -- 源目录 --
        EditorGUILayout.LabelField("源目录:", EditorStyles.boldLabel);
        _sourceDir = EditorGUILayout.TextField(_sourceDir);

        // -- 输出目录 --
        EditorGUILayout.LabelField("输出目录:", EditorStyles.boldLabel);
        _outputDir = EditorGUILayout.TextField(_outputDir);

        EditorGUILayout.Space();

        // -- 默认参数 --
        EditorGUILayout.LabelField("预制体默认参数:", EditorStyles.boldLabel);
        _walkSpeed  = EditorGUILayout.FloatField("行走速度", _walkSpeed);
        _walkRadius = EditorGUILayout.FloatField("行走半径", _walkRadius);

        EditorGUILayout.Space();

        // -- 操作 --
        EditorGUILayout.LabelField("操作:", EditorStyles.boldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("🔍 扫描文件", GUILayout.Height(30)))
                _scannedFiles = ScanFiles();

            GUI.enabled = _scannedFiles != null && _scannedFiles.Count > 0;
            if (GUILayout.Button("▶ 批量生成", GUILayout.Height(30)))
                GenerateAll();
            GUI.enabled = true;
        }

        EditorGUILayout.Space();

        // -- 预览 --
        if (_scannedFiles != null)
        {
            var total = _scannedFiles.Count;
            var hasSeqs = _scannedFiles.Count(f => f.Sequences != null && f.Sequences.Length > 0);
            EditorGUILayout.LabelField($"扫描结果: {total} 个文件, {hasSeqs} 个包含序列", EditorStyles.boldLabel);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(180));
            foreach (var info in _scannedFiles)
            {
                var seqStr = (info.Sequences != null && info.Sequences.Length > 0)
                    ? string.Join(", ", info.Sequences.Take(5))
                    : "(无序列)";
                EditorGUILayout.LabelField(
                    $"  [{info.Name}] → Pet_{info.Name}  序列: {seqStr}",
                    info.Sequences != null && info.Sequences.Length > 0
                        ? EditorStyles.label
                        : EditorStyles.helpBox);
            }
            EditorGUILayout.EndScrollView();
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "批量流程:\n" +
            "1. 扫描 Assets/Resources/PetWalk/ 下所有 .fla.pet 文件（全词匹配，排除其他文件）\n" +
            "2. 每个文件创建一个 Pet_{序号} 预制体\n" +
            "3. PetWalkController 自动处理方向映射和随机行走\n" +
            "4. 输出到同级目录",
            MessageType.Info);
    }

    // =========================================================================
    // 扫描
    // =========================================================================

    private List<WalkFileInfo> ScanFiles()
    {
        var result = new List<WalkFileInfo>();
        var absDir = ToAbsPath(_sourceDir);

        if (!Directory.Exists(absDir))
        {
            Debug.LogWarning($"[PetWalkBatch] 源目录不存在: {absDir}");
            return result;
        }

        // 全词匹配：只扫描 *.fla.pet 文件（盘上可能是 .fla.pet 或 .fla.pet.asset）
        var allFiles = Directory.GetFiles(absDir, "*.fla.pet*", SearchOption.TopDirectoryOnly);
        foreach (var f in allFiles)
        {
            var fileName = Path.GetFileName(f);
            // 全词匹配：排除 .meta 等无关文件
            if (!fileName.EndsWith(".fla.pet") && !fileName.EndsWith(".fla.pet.asset"))
                continue;

            // 提取序号: "1.fla.pet" 或 "1.fla.pet.asset" → "1"
            var name = fileName;
            if (name.EndsWith(".fla.pet.asset")) name = name.Substring(0, name.Length - ".fla.pet.asset".Length);
            else if (name.EndsWith(".fla.pet"))  name = name.Substring(0, name.Length - ".fla.pet".Length);
            else continue;

            var assetPath = ToAssetPath(f);
            var asset     = AssetDatabase.LoadAssetAtPath<SwfClipAsset>(assetPath);

            if (asset == null) continue; // 不是有效的 SwfClipAsset，跳过

            var sequences = (asset.Sequences != null)
                ? asset.Sequences.Select(s => s.Name).ToArray()
                : System.Array.Empty<string>();

            result.Add(new WalkFileInfo
            {
                Name      = name,
                AssetPath = assetPath,
                Asset     = asset,
                Sequences = sequences,
            });
        }

        // 按序号数值排序
        result.Sort((a, b) =>
        {
            if (long.TryParse(a.Name, out var na) && long.TryParse(b.Name, out var nb))
                return na.CompareTo(nb);
            return string.CompareOrdinal(a.Name, b.Name);
        });

        Debug.Log($"[PetWalkBatch] 扫描完成: {result.Count} 个有效文件");
        return result;
    }

    // =========================================================================
    // 批量生成
    // =========================================================================

    private void GenerateAll()
    {
        var files = _scannedFiles;
        if (files == null || files.Count == 0)
        {
            files = ScanFiles();
            if (files == null || files.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "未找到 .fla.pet.asset 文件", "确定");
                return;
            }
        }

        if (!EditorUtility.DisplayDialog("确认批量生成",
            $"将生成 {files.Count} 个行走预制体:\n\n" +
            $"源目录: {_sourceDir}\n输出目录: {_outputDir}\n\n" +
            $"确认开始?", "开始", "取消"))
            return;

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var successList = new List<string>();
        var failList    = new List<string>();

        AssetDatabase.StartAssetEditing();

        try
        {
            for (int i = 0; i < files.Count; i++)
            {
                var info = files[i];

                if (i % 3 == 0 || i == files.Count - 1)
                {
                    if (EditorUtility.DisplayCancelableProgressBar(
                        $"PetWalk 批量生成 ({i + 1}/{files.Count})",
                        $"处理 [{info.Name}]...",
                        (float)i / files.Count))
                    {
                        throw new System.OperationCanceledException("用户取消");
                    }
                }

                try
                {
                    CreateWalkPrefab(info);
                    successList.Add(info.Name);
                }
                catch (System.Exception ex)
                {
                    failList.Add($"{info.Name}: {ex.Message}");
                    Debug.LogError($"[PetWalkBatch] [{info.Name}] 失败: {ex.Message}");
                }

                if (i % 3 == 0)
                    System.GC.Collect();
            }
        }
        catch (System.OperationCanceledException)
        {
            Debug.LogWarning($"[PetWalkBatch] 用户取消，已完成 {successList.Count}");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            System.GC.Collect();
        }

        sw.Stop();

        if (successList.Count > 0)
        {
            var summary = $"批量生成完成!\n\n" +
                          $"✅ 成功: {successList.Count} 个\n" +
                          $"❌ 失败: {failList.Count} 个\n\n" +
                          $"耗时: {sw.Elapsed.TotalSeconds:F1} 秒\n" +
                          $"输出目录: {_outputDir}/";

            if (failList.Count > 0)
                summary += $"\n\n失败详情:\n{string.Join("\n", failList.Select(f => $"  • {f}"))}";

            Debug.Log($"[PetWalkBatch] {summary.Replace("\n", " | ")}");
            EditorUtility.DisplayDialog("批量生成完成", summary, "确定");
        }
    }

    // =========================================================================
    // 单个预制体创建
    // =========================================================================

    private void CreateWalkPrefab(WalkFileInfo info)
    {
        var name       = info.Name;
        var prefabName = "Pet_" + name;
        var prefabPath = Path.Combine(_outputDir, prefabName + ".prefab").Replace("\\", "/");

        EnsureDirExists(_outputDir);

        // 1. 创建 GameObject
        var go = new GameObject(prefabName);

        // 2. 添加 SwfClip（引用 pet asset）
        var swfClip = go.AddComponent<SwfClip>();
        var soClip  = new SerializedObject(swfClip);
        soClip.FindProperty("_clip").objectReferenceValue = info.Asset;
        // 设置默认序列为第一个序列
        if (info.Sequences != null && info.Sequences.Length > 0)
            soClip.FindProperty("_sequence").stringValue = info.Sequences[0];
        soClip.ApplyModifiedProperties();

        // 3. 添加 SwfClipController
        var controller = go.AddComponent<SwfClipController>();
        var soCtrl = new SerializedObject(controller);
        soCtrl.FindProperty("_autoPlay").boolValue      = false;     // PetWalkController 自己控制播放
        soCtrl.FindProperty("_playMode").enumValueIndex = (int)SwfClipController.PlayModes.Forward;
        soCtrl.FindProperty("_loopMode").enumValueIndex = (int)SwfClipController.LoopModes.Loop;
        soCtrl.ApplyModifiedProperties();

        // 4. 添加 SwfClipFrameEvents
        go.AddComponent<SwfClipFrameEvents>();

        // 5. 添加 PetWalkController（配置默认参数）
        var walker = go.AddComponent<PetWalkController>();
        var soWalker = new SerializedObject(walker);
        soWalker.FindProperty("walkSpeed").floatValue  = _walkSpeed;
        soWalker.FindProperty("walkRadius").floatValue  = _walkRadius;
        soWalker.ApplyModifiedProperties();

        // 6. 保存预制体
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existing)
            PrefabUtility.SaveAsPrefabAssetAndConnect(go, prefabPath, InteractionMode.AutomatedAction);
        else
            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);

        DestroyImmediate(go);

        Debug.Log($"[PetWalkBatch] [{name}] ✓ → {prefabPath}");
    }

    // =========================================================================
    // 工具方法
    // =========================================================================

    private static string ToAssetPath(string absolutePath)
    {
        if (string.IsNullOrEmpty(absolutePath)) return "";
        var dataPath = Application.dataPath;
        if (absolutePath.StartsWith(dataPath))
            return "Assets" + absolutePath.Substring(dataPath.Length);
        return absolutePath;
    }

    private static string ToAbsPath(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath)) return "";
        if (assetPath.StartsWith("Assets"))
            return Path.Combine(
                Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length),
                assetPath);
        return assetPath;
    }

    private static void EnsureDirExists(string path)
    {
        path = path.Replace("\\", "/");
        if (AssetDatabase.IsValidFolder(path)) return;
        var parts = path.Split('/');
        var current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            var next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
