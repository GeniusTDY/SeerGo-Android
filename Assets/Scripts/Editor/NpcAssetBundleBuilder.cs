using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// NPC + PetWalk + Map 动画 AssetBundle 批量打包工具
/// 
/// 输出结构:
///   {ProjectRoot}/GameAnim/NpcAnim/npc_{mapId}_{variant}     -- NPC 动画 AB
///   {ProjectRoot}/GameAnim/PetWalkAnim/pwa_{num}             -- 精灵走路 AB
///   {ProjectRoot}/GameAnim/MapAnim/Map_{mapId}_{variant}     -- 地图动画 AB
/// 
/// 运行时 resId 对应:
///   "Prefabs/NPC_-50005_1" → GameAnim/NpcAnim/npc_-50005_1       → 预制体 NPC_-50005_1
///   "Prefabs/Pet_10"       → GameAnim/PetWalkAnim/pwa_10         → 预制体 Pet_10
///   "Prefabs/Map_-50001_1" → GameAnim/MapAnim/Map_-50001_1       → 预制体 Map_-50001_1
/// </summary>
public static class NpcAssetBundleBuilder
{
    private const string NpcResourceDir     = "Assets/NpcResource";
    private const string NpcPrefabsDir      = "Assets/NpcResource/Prefabs";
    private const string PetWalkPrefabsDir  = "Assets/GeneratedWalkPrefabs";
    private const string MapPrefabsDir      = "Assets/Resources_moved/Prefabs";
    private const string NpcOutputSubDir    = "GameAnim/NpcAnim";
    private const string PetWalkOutputSubDir = "GameAnim/PetWalkAnim";
    private const string MapOutputSubDir    = "GameAnim/MapAnim";

    // ============================================================
    // 菜单
    // ============================================================

    [MenuItem("Assets/Build NpcAssetBundles/构建全部 (Windows)")]
    public static void BuildAll_Win() => BuildAll(BuildTarget.StandaloneWindows);
    [MenuItem("Assets/Build NpcAssetBundles/构建全部 (Android)")]
    public static void BuildAll_Android() => BuildAll(BuildTarget.Android);

    [MenuItem("Assets/Build NpcAssetBundles/Windows/仅构建 NPC AB")]
    public static void BuildNpc_Win() => BuildNpcBundles(BuildTarget.StandaloneWindows);
    [MenuItem("Assets/Build NpcAssetBundles/Windows/仅构建 PetWalk AB")]
    public static void BuildPet_Win() => BuildPetWalkBundles(BuildTarget.StandaloneWindows);
    [MenuItem("Assets/Build NpcAssetBundles/Windows/仅构建 Map AB")]
    public static void BuildMap_Win() => BuildMapBundles(BuildTarget.StandaloneWindows);

    [MenuItem("Assets/Build NpcAssetBundles/Android/仅构建 NPC AB")]
    public static void BuildNpc_Android() => BuildNpcBundles(BuildTarget.Android);
    [MenuItem("Assets/Build NpcAssetBundles/Android/仅构建 PetWalk AB")]
    public static void BuildPet_Android() => BuildPetWalkBundles(BuildTarget.Android);
    [MenuItem("Assets/Build NpcAssetBundles/Android/仅构建 Map AB")]
    public static void BuildMap_Android() => BuildMapBundles(BuildTarget.Android);

    [MenuItem("Assets/Build NpcAssetBundles/列出全部清单")]
    public static void ListAllIds()
    {
        Debug.Log("=== NPC ===");
        foreach (var e in ScanNpcDirectories())
            Debug.Log($"  {e.DirName} → npc_{e.MapId}_{e.Variant}");
        Debug.Log("=== PetWalk ===");
        foreach (var e in ScanPetWalkPrefabs())
            Debug.Log($"  {e.PrefabName} → pwa_{e.Number}");
        Debug.Log("=== Map ===");
        foreach (var e in ScanMapPrefabs())
            Debug.Log($"  {e.PrefabName} → {e.PrefabName}");
    }

    // ============================================================
    // 路径工具
    // ============================================================

    /// <summary>归一化项目根路径，解决 Path.Combine + ".." 不归一的问题</summary>
    private static string ProjectRoot => Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

    private static string GetOutputDir(string subDir) =>
        Path.GetFullPath(Path.Combine(Application.dataPath, "..", subDir));

    // ============================================================
    // 全部构建
    // ============================================================

    private static void BuildAll(BuildTarget target)
    {
        BuildNpcBundles(target);
        BuildPetWalkBundles(target);
        BuildMapBundles(target);
    }

    // ============================================================
    // NPC
    // ============================================================

    private static void BuildNpcBundles(BuildTarget target)
    {
        AssetDatabase.RemoveUnusedAssetBundleNames();
        var entries = ScanNpcDirectories();
        var builds = new List<AssetBundleBuild>();
        var names  = new List<string>();

        foreach (var e in entries)
        {
            string bundleName = $"npc_{e.MapId}_{e.Variant}";
            string prefabPath = $"{NpcPrefabsDir}/{e.DirName}.prefab";
            if (!File.Exists(prefabPath)) { Debug.LogWarning($"[Builder] NPC 缺 Prefab: {prefabPath}"); continue; }
            builds.Add(new AssetBundleBuild { assetBundleName = bundleName, assetNames = new[] { prefabPath } });
            names.Add(bundleName);
        }
        if (builds.Count == 0) { Debug.LogWarning("[Builder] 无 NPC"); return; }

        string outDir = GetOutputDir(NpcOutputSubDir);
        Directory.CreateDirectory(outDir);
        var m = BuildPipeline.BuildAssetBundles(outDir, builds.ToArray(), BuildAssetBundleOptions.ChunkBasedCompression, target);
        if (m != null) { Debug.Log($"[Builder] NPC: {m.GetAllAssetBundles().Length} → {outDir}"); WriteIndex(outDir, names); CleanFiles(outDir, "npc_"); }
        else Debug.LogError("[Builder] NPC 失败");
        AssetDatabase.Refresh();
    }

    // ============================================================
    // PetWalk
    // ============================================================

    private static void BuildPetWalkBundles(BuildTarget target)
    {
        AssetDatabase.RemoveUnusedAssetBundleNames();
        var entries = ScanPetWalkPrefabs();
        var builds = new List<AssetBundleBuild>();
        var names  = new List<string>();

        foreach (var e in entries)
        {
            string bundleName = $"pwa_{e.Number}";
            string prefabPath = $"{PetWalkPrefabsDir}/{e.PrefabName}.prefab";
            if (!File.Exists(prefabPath)) { Debug.LogWarning($"[Builder] PetWalk 缺 Prefab: {prefabPath}"); continue; }
            builds.Add(new AssetBundleBuild { assetBundleName = bundleName, assetNames = new[] { prefabPath } });
            names.Add(bundleName);
        }
        if (builds.Count == 0) { Debug.LogWarning("[Builder] 无 PetWalk"); return; }

        string outDir = GetOutputDir(PetWalkOutputSubDir);
        Directory.CreateDirectory(outDir);
        var m = BuildPipeline.BuildAssetBundles(outDir, builds.ToArray(), BuildAssetBundleOptions.ChunkBasedCompression, target);
        if (m != null) { Debug.Log($"[Builder] PetWalk: {m.GetAllAssetBundles().Length} → {outDir}"); WriteIndex(outDir, names); CleanFiles(outDir, "pwa_"); }
        else Debug.LogError("[Builder] PetWalk 失败");
        AssetDatabase.Refresh();
    }

    // ============================================================
    // Map 动画 (NEW)
    // ============================================================

    private static void BuildMapBundles(BuildTarget target)
    {
        AssetDatabase.RemoveUnusedAssetBundleNames();
        var entries = ScanMapPrefabs();
        var builds = new List<AssetBundleBuild>();
        var names  = new List<string>();

        foreach (var e in entries)
        {
            string bundleName = e.PrefabName;
            string prefabPath = $"{MapPrefabsDir}/{e.PrefabName}.prefab";
            string fullPath = Path.Combine(Application.dataPath, prefabPath.Replace("Assets/", ""));

            if (!File.Exists(prefabPath) && !File.Exists(fullPath))
            {
                Debug.LogWarning($"[Builder] Map 缺 Prefab: {prefabPath} / {fullPath}");
                continue;
            }

            builds.Add(new AssetBundleBuild { assetBundleName = bundleName, assetNames = new[] { prefabPath } });
            names.Add(bundleName);
            Debug.Log($"[Builder] Map: {bundleName} ← {prefabPath}");
        }

        if (builds.Count == 0) { Debug.LogWarning("[Builder] 无 Map"); return; }

        string outDir = GetOutputDir(MapOutputSubDir);
        Debug.Log($"[Builder] Map 输出目录(归一化): {outDir}");
        Directory.CreateDirectory(outDir);

        var m = BuildPipeline.BuildAssetBundles(outDir, builds.ToArray(),
            BuildAssetBundleOptions.ChunkBasedCompression, target);

        if (m != null)
        {
            Debug.Log($"[Builder] Map 构建成功: {m.GetAllAssetBundles().Length} 个 → {outDir}");
            WriteIndex(outDir, names);
            // 验证: 列出目录文件
            Debug.Log($"[Builder] Map 目录文件列表:");
            foreach (var f in Directory.GetFiles(outDir))
                Debug.Log($"    {Path.GetFileName(f)} ({new FileInfo(f).Length} bytes)");
        }
        else Debug.LogError("[Builder] Map 构建失败");
    }

    private static List<MapEntry> ScanMapPrefabs()
    {
        var result = new List<MapEntry>();
        string fullDir = Path.Combine(Application.dataPath, "..", MapPrefabsDir.Replace("Assets/", ""));
        // 也尝试相对路径
        if (!Directory.Exists(fullDir)) fullDir = Path.Combine(Application.dataPath, MapPrefabsDir.Replace("Assets/", ""));
        if (!Directory.Exists(fullDir)) fullDir = MapPrefabsDir;

        Debug.Log($"[Builder] 扫描 Map 目录: {fullDir} 存在={Directory.Exists(fullDir)}");

        if (!Directory.Exists(fullDir)) return result;

        var allFiles = Directory.GetFiles(fullDir, "Map_*.prefab");
        Debug.Log($"[Builder] 找到 {allFiles.Length} 个 Map_*.prefab 文件");

        foreach (var file in allFiles)
        {
            string name = Path.GetFileNameWithoutExtension(file);
            var m = System.Text.RegularExpressions.Regex.Match(name, @"^Map_(-?\d+)[_-](\d+)$");
            if (m.Success)
                result.Add(new MapEntry { PrefabName = name, MapId = m.Groups[1].Value, Variant = m.Groups[2].Value });
            else
                Debug.LogWarning($"[Builder] Map 名称不匹配规则，跳过: {name}");
        }
        Debug.Log($"[Builder] 扫描 Map: {result.Count} 个匹配");
        return result;
    }

    // ============================================================
    // 公用辅助
    // ============================================================

    private static void WriteIndex(string path, List<string> names)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# Total: {names.Count}");
        foreach (var n in names) sb.AppendLine(n);
        File.WriteAllText(Path.Combine(path, "bundle_index.txt"), sb.ToString());
    }

    /// <summary>只保留 prefix 开头的文件和 index</summary>
    private static void CleanFiles(string path, string prefix)
    {
        foreach (var f in Directory.GetFiles(path))
        {
            string fn = Path.GetFileName(f);
            if (!fn.StartsWith(prefix) && fn != "bundle_index.txt")
                File.Delete(f);
        }
    }

    // ============================================================
    // 本地扫描
    // ============================================================

    private static List<NpcEntry> ScanNpcDirectories()
    {
        var r = new List<NpcEntry>();
        if (!Directory.Exists(NpcResourceDir)) return r;
        foreach (var d in Directory.GetDirectories(NpcResourceDir))
        {
            string n = Path.GetFileName(d);
            if (n == "Prefabs") continue;
            var m = System.Text.RegularExpressions.Regex.Match(n, @"^NPC_(-?\d+)_(\d+)$");
            if (m.Success) r.Add(new NpcEntry { DirName = n, MapId = m.Groups[1].Value, Variant = m.Groups[2].Value });
        }
        Debug.Log($"[Builder] NPC: {r.Count}");
        return r;
    }

    private static List<PetWalkEntry> ScanPetWalkPrefabs()
    {
        var r = new List<PetWalkEntry>();
        if (!Directory.Exists(PetWalkPrefabsDir)) return r;
        foreach (var f in Directory.GetFiles(PetWalkPrefabsDir, "Pet_*.prefab"))
        {
            string n = Path.GetFileNameWithoutExtension(f);
            var m = System.Text.RegularExpressions.Regex.Match(n, @"^Pet_(\d+)$");
            if (m.Success) r.Add(new PetWalkEntry { PrefabName = n, Number = int.Parse(m.Groups[1].Value) });
        }
        Debug.Log($"[Builder] PetWalk: {r.Count}");
        return r;
    }

    internal struct NpcEntry   { public string DirName, MapId, Variant; }
    internal struct PetWalkEntry { public string PrefabName; public int Number; }
    internal struct MapEntry     { public string PrefabName, MapId, Variant; }
}
