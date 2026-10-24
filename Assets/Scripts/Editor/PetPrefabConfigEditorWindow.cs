using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;

// UTF-8 with BOM — 确保中文表头在Excel等工具中不出现乱码
internal static class CsvEncoding
{
    public static readonly UTF8Encoding UTF8WithBOM = new UTF8Encoding(true);
}

/// <summary>
/// 精灵预制体配置编辑器 - 动态加载精灵动画，调整每个动作的位置/大小，
/// 并保存回 pet_prefab_config.csv
/// </summary>
public class PetPrefabConfigEditorWindow : EditorWindow
{
    private int petId = 1;
    private string currentAction = "idle";
    private readonly string[] actionOptions = { "idle", "attack", "sa", "cp", "hited" };
    private int actionIndex = 0;

    // 可编辑的配置参数
    private float scale = 1.5f;
    private float positionX = -4f;
    private float positionY = 0f;
    private float flipPositionX = 4f;

    // 预览相关
    private GameObject previewGO;
    private string loadedPetId = "";
    private bool isNewType;
    private Vector2 scrollPos;

    // AB路径（Editor下）
    private string bundlePath;

    // 缓存已加载的Bundle
    private Dictionary<string, AssetBundle> editorBundles = new Dictionary<string, AssetBundle>();

    // 当前CSV中的配置状态描述（避免OnGUI中调用MonoBehaviour单例破坏GUILayout）
    private string cachedConfigStatus = "";

    [MenuItem("Tools/精灵预制体配置编辑器")]
    public static void ShowWindow()
    {
        var window = GetWindow<PetPrefabConfigEditorWindow>("精灵预制体配置编辑器");
        window.minSize = new Vector2(420, 520);
    }

    private void OnEnable()
    {
        bundlePath = Path.Combine(Application.dataPath, "StreamingAssets", "AssetBundles");
        RefreshCachedConfigStatus();
    }

    private void OnDisable()
    {
        DestroyPreview();
        UnloadAllBundles();
    }

    /// <summary>
    /// 快捷键处理：Q上一只 / E下一只，自动加载
    /// </summary>
    private void HandleKeyboardShortcuts()
    {
        Event e = Event.current;
        if (e == null || e.type != EventType.KeyDown) return;

        switch (e.keyCode)
        {
            case KeyCode.Q:
                petId = Mathf.Max(1, petId - 1);
                try { LoadPetPreview(); } catch (System.Exception ex) { Debug.LogError($"[PetPrefabConfigEditor] Q键加载异常: {ex}"); }
                e.Use();
                break;
            case KeyCode.E:
                petId = Mathf.Max(1, petId + 1);
                try { LoadPetPreview(); } catch (System.Exception ex) { Debug.LogError($"[PetPrefabConfigEditor] E键加载异常: {ex}"); }
                e.Use();
                break;
        }
    }

    private void OnGUI()
    {
        // 快捷键：Q上一只 / E下一只（窗口聚焦时生效）
        HandleKeyboardShortcuts();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        // ========== 标题与ID输入 ==========
        GUILayout.Space(10);
        EditorGUILayout.LabelField("精灵预制体配置编辑器", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("加载精灵动画后，通过下方参数调整位置与缩放，实时预览，确认后保存到CSV。", MessageType.Info);

        GUILayout.Space(10);

        // 精灵ID输入
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("精灵ID", GUILayout.Width(50));
        petId = EditorGUILayout.IntField(petId, GUILayout.Width(60));
        GUILayout.Space(10);

        // 当前配置提示（使用缓存字符串，不在OnGUI中调用MonoBehaviour单例）
        if (!string.IsNullOrEmpty(cachedConfigStatus))
        {
            EditorGUILayout.LabelField(cachedConfigStatus, EditorStyles.miniLabel);
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);

        // 动作类型选择
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("动作类型", GUILayout.Width(50));
        actionIndex = EditorGUILayout.Popup(actionIndex, actionOptions);
        currentAction = actionOptions[actionIndex];
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        // ========== 加载/卸载按钮 ==========
        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = new Color(0.5f, 0.8f, 0.5f);
        if (GUILayout.Button("加载精灵", GUILayout.Height(30)))
        {
            try { LoadPetPreview(); }
            catch (System.Exception e) { Debug.LogError($"[PetPrefabConfigEditor] 加载异常: {e}"); }
        }
        GUI.backgroundColor = Color.white;

        if (GUILayout.Button("刷新配置导入", GUILayout.Height(30)))
        {
            try { RefreshCachedConfigStatus(); }
            catch (System.Exception e) { Debug.LogError($"[PetPrefabConfigEditor] 刷新异常: {e}"); }
        }

        GUI.backgroundColor = new Color(1f, 0.6f, 0.6f);
        if (GUILayout.Button("卸载预览", GUILayout.Height(30)))
        {
            try { DestroyPreview(); }
            catch (System.Exception e) { Debug.LogError($"[PetPrefabConfigEditor] 卸载异常: {e}"); }
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        // ========== 可编辑参数区 ==========
        EditorGUILayout.LabelField("位置与缩放参数", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();

        scale = EditorGUILayout.FloatField("缩放 (scale)", scale);
        positionX = EditorGUILayout.FloatField("位置 X (position_x)", positionX);
        positionY = EditorGUILayout.FloatField("位置 Y (position_y)", positionY);
        flipPositionX = EditorGUILayout.FloatField("镜像位置 X (flip_position_x)", flipPositionX);

        if (EditorGUI.EndChangeCheck())
        {
            ApplyToPreview();
        }

        GUILayout.Space(5);

        // 快捷按钮行
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("我方预览"))
        {
            try { ApplyPlayerPreview(); } catch (System.Exception e) { Debug.LogError($"[PetPrefabConfigEditor] 预览异常: {e}"); }
        }
        if (GUILayout.Button("敌方预览(镜像)"))
        {
            try { ApplyEnemyPreview(); } catch (System.Exception e) { Debug.LogError($"[PetPrefabConfigEditor] 预览异常: {e}"); }
        }
        if (GUILayout.Button("重置位置"))
        {
            try
            {
                positionX = -4f;
                positionY = 0f;
                flipPositionX = 4f;
                scale = 1.5f;
                ApplyToPreview();
            }
            catch (System.Exception e) { Debug.LogError($"[PetPrefabConfigEditor] 重置异常: {e}"); }
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        // ========== 保存按钮 ==========
        EditorGUILayout.BeginHorizontal();
        GUI.backgroundColor = new Color(0.3f, 0.7f, 1f);
        if (GUILayout.Button("保存到CSV", GUILayout.Height(40)))
        {
            try { SaveConfigToCSV(); }
            catch (System.Exception e) { Debug.LogError($"[PetPrefabConfigEditor] 保存异常: {e}"); }
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        // ========== 预览信息 ==========
        if (previewGO != null)
        {
            EditorGUILayout.LabelField("预览状态", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"当前精灵ID: {loadedPetId}");
            EditorGUILayout.LabelField($"动画类型: {(isNewType ? "新类型 (SwfClip)" : "旧类型 (Animator)")}");
            EditorGUILayout.LabelField($"当前动作: {currentAction}");
            EditorGUILayout.LabelField($"实际Transform: pos={previewGO.transform.position}, scale={previewGO.transform.localScale}");

            GUILayout.Space(5);
            // 显示预览对象的组件信息
            if (isNewType)
            {
                var swfClip = previewGO.GetComponent<FTRuntime.SwfClip>();
                if (swfClip != null)
                {
                    string clipInfo = swfClip.clip != null ? swfClip.clip.name : "未设置";
                    EditorGUILayout.LabelField($"SwfClip: {clipInfo}, 当前帧: {swfClip.currentFrame}");
                }
            }
            else
            {
                var animator = previewGO.GetComponent<Animator>();
                if (animator != null && animator.runtimeAnimatorController != null)
                    EditorGUILayout.LabelField($"Animator: {animator.runtimeAnimatorController.name}");
            }
        }

        EditorGUILayout.EndScrollView();
    }

    #region 加载与预览

    private void LoadPetPreview()
    {
        DestroyPreview();

        string bundleName = $"pfa_{petId}";
        string prefabName = $"{petId}_{currentAction}";

        GameObject prefab = LoadPrefabFromAB(bundleName, prefabName);
        if (prefab == null)
        {
            EditorUtility.DisplayDialog("加载失败",
                $"无法加载预制体:\nBundle: {bundleName}\nPrefab: {prefabName}\n\n请确认AB文件存在于 StreamingAssets/AssetBundles/ 目录下。",
                "确定");
            return;
        }

        // 在场景中实例化（AB中的对象非Unity Prefab，使用Instantiate），隐藏于层级视图
        previewGO = (GameObject)Instantiate(prefab);
        if (previewGO == null)
        {
            Debug.LogError($"[PetPrefabConfigEditor] Instantiate返回null: {bundleName}/{prefabName}");
            return;
        }
        previewGO.hideFlags = HideFlags.HideAndDontSave;
        previewGO.name = $"[Preview] {prefabName}";

        // 判断动画类型
        isNewType = previewGO.GetComponent<SwfClipFrameEvents>() != null;

        loadedPetId = petId.ToString();

        // 从CSV加载当前配置
        LoadConfigFromCSV();

        // 默认以"我方"模式展示
        ApplyPlayerPreview();

        // 聚焦场景视图
        Selection.activeGameObject = previewGO;
        SceneView.FrameLastActiveSceneView();

        Debug.Log($"[PetPrefabConfigEditor] 加载成功: {bundleName}/{prefabName}, 类型: {(isNewType ? "SwfClip" : "Animator")}");
    }

    private GameObject LoadPrefabFromAB(string bundleName, string prefabName)
    {
        // 尝试使用 ExternalAssetBundleManager（运行时）
        if (Application.isPlaying && ExternalAssetBundleManager.Instance != null)
        {
            return ExternalAssetBundleManager.Instance.LoadPetPrefab(bundleName, prefabName, "");
        }

        // 编辑器下直接加载AB
        string bundleFilePath = Path.Combine(bundlePath, bundleName);
        if (!File.Exists(bundleFilePath))
        {
            Debug.LogWarning($"[PetPrefabConfigEditor] AB文件不存在: {bundleFilePath}");
            return null;
        }

        AssetBundle bundle;
        if (!editorBundles.TryGetValue(bundleName, out bundle) || bundle == null)
        {
            bundle = AssetBundle.LoadFromFile(bundleFilePath);
            if (bundle != null)
            {
                editorBundles[bundleName] = bundle;
            }
            else
            {
                Debug.LogError($"[PetPrefabConfigEditor] 无法加载AB: {bundleName}");
                return null;
            }
        }

        var prefab = bundle.LoadAsset<GameObject>(prefabName);
        if (prefab == null)
        {
            Debug.LogWarning($"[PetPrefabConfigEditor] AB中找不到资源: {prefabName}");
        }
        return prefab;
    }

    private void ApplyToPreview()
    {
        if (previewGO == null) return;

        Vector3 pos = new Vector3(positionX, positionY, 0);
        Vector3 scl = new Vector3(scale, scale, scale);

        if (isNewType)
        {
            // 新类型：直接设置 transform
            previewGO.transform.position = pos;
            previewGO.transform.localScale = scl;
        }
        else
        {
            // 旧类型：使用 SpritePivotAligner
            var aligner = previewGO.GetComponent<SpritePivotAligner>();
            if (aligner != null)
            {
                aligner.InitializeSync(pos, scl);
            }
            else
            {
                previewGO.transform.position = pos;
                previewGO.transform.localScale = scl;
            }
        }

        // 刷新场景视图
        SceneView.RepaintAll();
    }

    private void ApplyPlayerPreview()
    {
        if (previewGO == null) return;

        Vector3 pos = new Vector3(positionX, positionY, 0);
        Vector3 scl = new Vector3(scale, scale, scale);
        ApplyTransform(pos, scl);
        SceneView.RepaintAll();
    }

    private void ApplyEnemyPreview()
    {
        if (previewGO == null) return;

        Vector3 pos = new Vector3(flipPositionX, positionY, 0);
        Vector3 scl = new Vector3(-scale, scale, scale); // 镜像翻转
        ApplyTransform(pos, scl);
        SceneView.RepaintAll();
    }

    private void ApplyTransform(Vector3 pos, Vector3 scl)
    {
        if (isNewType)
        {
            previewGO.transform.position = pos;
            previewGO.transform.localScale = scl;
        }
        else
        {
            var aligner = previewGO.GetComponent<SpritePivotAligner>();
            if (aligner != null)
            {
                aligner.InitializeSync(pos, scl);
            }
            else
            {
                previewGO.transform.position = pos;
                previewGO.transform.localScale = scl;
            }
        }
    }

    private void DestroyPreview()
    {
        if (previewGO != null)
        {
            DestroyImmediate(previewGO);
            previewGO = null;
        }
        loadedPetId = "";
        isNewType = false;
    }

    private void UnloadAllBundles()
    {
        foreach (var kvp in editorBundles)
        {
            if (kvp.Value != null)
                kvp.Value.Unload(true);
        }
        editorBundles.Clear();
    }

    #endregion

    #region CSV读写

    /// <summary>
    /// 刷新缓存的配置状态字符串（安全调用，不依赖MonoBehaviour单例）
    /// </summary>
    private void RefreshCachedConfigStatus()
    {
        LoadConfigFromCSV();
        Repaint();
    }

    /// <summary>
    /// 从CSV直接读取当前petId的配置到编辑字段（不依赖运行时MonoBehaviour单例）
    /// </summary>
    private void LoadConfigFromCSV()
    {
        string csvPath = Path.Combine(Application.dataPath, "Resources", "Data", "Pets", "pet_prefab_config.csv");

        if (!File.Exists(csvPath))
        {
            cachedConfigStatus = "(CSV文件未找到)";
            return;
        }

        try
        {
            string[] lines = File.ReadAllLines(csvPath, CsvEncoding.UTF8WithBOM);
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] values = line.Split(',');
                if (values.Length >= 4 && int.TryParse(values[0], out int csvId) && csvId == petId)
                {
                    scale = float.Parse(values[1]);
                    positionX = float.Parse(values[2]);
                    positionY = float.Parse(values[3]);
                    flipPositionX = float.Parse(values[4]);

                    cachedConfigStatus = $"(当前: scale={scale}, pos=({positionX},{positionY}), flipX={flipPositionX})";
                    Debug.Log($"[PetPrefabConfigEditor] 从CSV加载配置: petId={petId}, scale={scale}, pos=({positionX},{positionY}), flipX={flipPositionX}");
                    return;
                }
            }

            cachedConfigStatus = "(该ID暂无配置)";
            Debug.Log($"[PetPrefabConfigEditor] petId={petId} 在CSV中无配置，使用默认值");
        }
        catch (System.Exception e)
        {
            cachedConfigStatus = $"(CSV读取失败: {e.Message})";
            Debug.LogError($"[PetPrefabConfigEditor] 读取CSV失败: {e.Message}");
        }
    }

    /// <summary>
    /// 保存当前配置到CSV文件
    /// </summary>
    private void SaveConfigToCSV()
    {
        string csvPath = Path.Combine(Application.dataPath, "Resources", "Data", "Pets", "pet_prefab_config.csv");

        if (!File.Exists(csvPath))
        {
            EditorUtility.DisplayDialog("错误", $"CSV文件不存在:\n{csvPath}", "确定");
            return;
        }

        // 读取所有行
        string[] lines;
        try
        {
            lines = File.ReadAllLines(csvPath, CsvEncoding.UTF8WithBOM);
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("错误", $"读取CSV失败:\n{e.Message}", "确定");
            return;
        }

        if (lines.Length == 0)
        {
            EditorUtility.DisplayDialog("错误", "CSV文件为空", "确定");
            return;
        }

        // 解析CSV，查找并更新行
        bool found = false;
        string newLine = $"{petId},{scale},{positionX},{positionY},{flipPositionX}";

        for (int i = 1; i < lines.Length; i++) // 跳过表头
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] values = line.Split(',');
            if (values.Length >= 4 && int.TryParse(values[0], out int csvId) && csvId == petId)
            {
                lines[i] = newLine;
                found = true;
                break;
            }
        }

        if (!found)
        {
            // 追加新行
            var newLines = new List<string>(lines);
            newLines.Add(newLine);
            lines = newLines.ToArray();
        }

        // 写回文件（UTF-8 with BOM，确保中文不乱码）
        try
        {
            File.WriteAllLines(csvPath, lines, CsvEncoding.UTF8WithBOM);
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("错误", $"写入CSV失败:\n{e.Message}", "确定");
            return;
        }

        AssetDatabase.Refresh();

        // 更新缓存状态
        cachedConfigStatus = $"(已保存: scale={scale}, pos=({positionX},{positionY}), flipX={flipPositionX})";

        string action = found ? "更新" : "新增";
        EditorUtility.DisplayDialog("保存成功",
            $"精灵 {petId} 的配置已{action}:\n" +
            $"scale={scale}, pos=({positionX},{positionY}), flipX={flipPositionX}",
            "确定");

        Debug.Log($"[PetPrefabConfigEditor] 保存配置: petId={petId}, {newLine}");
    }

    #endregion
}
