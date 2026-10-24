using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NpcAssetBundle 运行时测试脚本（resId 版本）
/// 使用方式: 挂到场景中任意 GameObject 上，Play 后操作
///
/// 按键:
///   L - 加载全部
///   U - 卸载全部
///   1-9 - 依次加载
///   R - 加载 Inspector 中配置的 resId
///   P - 打印状态
/// </summary>
public class NpcAssetBundleTest : MonoBehaviour
{
    [Header("测试配置")]
    [Tooltip("如 Prefabs/NPC_-50005_1 或 Prefabs/Pet_10")]
    public string testResId = "Prefabs/NPC_-50005_1";

    [Tooltip("加载后是否直接实例化到场景")]
    public bool autoInstantiate = true;

    [Tooltip("实例化位置")]
    public Vector3 spawnPosition = Vector3.zero;

    [Header("状态")]
    [SerializeField] private int loadedCount;
    [SerializeField] private string lastLog = "";

    private readonly List<string> _testResIds = new List<string>();
    private readonly List<GameObject> _spawnedInstances = new List<GameObject>();

    private void Start()
    {
        Debug.Log("=== NpcAssetBundle 测试启动 ===");
        Debug.Log("L=加载全部, U=卸载全部, 1-9=按序加载, R=Inspector参数, P=状态");

        NpcAssetBundleLoader.Initialize();

        // 从 GameAnim/NpcAnim 目录扫描 npc_ 开头的 bundle，生成测试 resId
        string projectRoot = System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, ".."));
        string npcDir = System.IO.Path.Combine(projectRoot, "GameAnim/NpcAnim");
        if (System.IO.Directory.Exists(npcDir))
        {
            foreach (var f in System.IO.Directory.GetFiles(npcDir, "npc_*"))
            {
                string fn = System.IO.Path.GetFileName(f);
                if (fn.EndsWith(".manifest")) continue;
                // npc_-50001_1 → Prefabs/NPC_-50001_1
                string name = fn.StartsWith("npc_") ? fn.Substring(4) : fn;
                _testResIds.Add($"Prefabs/NPC_{name}");
            }
        }

        // 从 GameAnim/PetWalkAnim 目录扫描 pwa_ 开头的 bundle
        string pwaDir = System.IO.Path.Combine(projectRoot, "GameAnim/PetWalkAnim");
        if (System.IO.Directory.Exists(pwaDir))
        {
            foreach (var f in System.IO.Directory.GetFiles(pwaDir, "pwa_*"))
            {
                string fn = System.IO.Path.GetFileName(f);
                if (fn.EndsWith(".manifest")) continue;
                string num = fn.Replace("pwa_", "");
                _testResIds.Add($"Prefabs/Pet_{num}");
            }
        }

        // 从 GameAnim/MapAnim 目录扫描 Map_ 开头的 bundle
        string mapDir = System.IO.Path.Combine(projectRoot, "GameAnim/MapAnim");
        if (System.IO.Directory.Exists(mapDir))
        {
            foreach (var f in System.IO.Directory.GetFiles(mapDir, "Map_*"))
            {
                string fn = System.IO.Path.GetFileName(f);
                if (fn.EndsWith(".manifest")) continue;
                _testResIds.Add($"Prefabs/{fn}");
            }
        }

        Debug.Log($"可用 resId 总数: {_testResIds.Count}");
        foreach (var id in _testResIds) Debug.Log($"  {id}");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L)) StartCoroutine(LoadAll());
        if (Input.GetKeyDown(KeyCode.U)) UnloadAllTest();
        if (Input.GetKeyDown(KeyCode.P)) PrintStatus();
        if (Input.GetKeyDown(KeyCode.R)) StartCoroutine(LoadSingle(testResId));

        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                StartCoroutine(LoadAt(i));
        }
    }

    private IEnumerator LoadAll()
    {
        Debug.Log($"<color=yellow>开始加载全部 {_testResIds.Count} 个...</color>");
        int ok = 0, fail = 0;
        for (int i = 0; i < _testResIds.Count; i++)
        {
            var task = NpcAssetBundleLoader.LoadPrefabAsync(_testResIds[i]);
            while (!task.IsCompleted) yield return null;
            if (task.Result != null) ok++;
            else fail++;
            if ((i + 1) % 5 == 0)
                Debug.Log($"<color=cyan>进度: {i + 1}/{_testResIds.Count} (OK:{ok} FAIL:{fail})</color>");
            yield return null;
        }
        Debug.Log($"<color=green>完成! OK:{ok} FAIL:{fail}</color>");
    }

    private IEnumerator LoadSingle(string resId)
    {
        Debug.Log($"<color=yellow>加载: {resId}</color>");
        var task = NpcAssetBundleLoader.LoadPrefabAsync(resId);
        while (!task.IsCompleted) yield return null;
        if (task.Result != null)
        {
            Debug.Log($"<color=green>成功: {task.Result.name}</color>");
            if (autoInstantiate) Spawn(task.Result, resId);
        }
        else Debug.LogError($"<color=red>失败: {resId}</color>");
    }

    private IEnumerator LoadAt(int index)
    {
        if (index >= _testResIds.Count)
        {
            Debug.LogWarning($"序号 {index + 1} 超出范围 (共 {_testResIds.Count})");
            yield break;
        }
        yield return LoadSingle(_testResIds[index]);
    }

    private void Spawn(GameObject prefab, string resId)
    {
        var pos = spawnPosition + Vector3.right * _spawnedInstances.Count * 2f;
        var inst = Instantiate(prefab, pos, Quaternion.identity);
        inst.name = $"{prefab.name}_Test";
        _spawnedInstances.Add(inst);
        lastLog = $"实例化: {resId} → {inst.name}";
    }

    private void UnloadAllTest()
    {
        foreach (var inst in _spawnedInstances)
            if (inst != null) Destroy(inst);
        _spawnedInstances.Clear();
        NpcAssetBundleLoader.UnloadAll();
        lastLog = "已卸载全部";
        Debug.Log(lastLog);
    }

    private void PrintStatus()
    {
        Debug.Log($"=== 状态: resId总数={_testResIds.Count}, 实例={_spawnedInstances.Count}, 最后:{lastLog}");
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 320, 400));
        GUILayout.Label($"=== NpcAB Test === (resId数:{_testResIds.Count})");
        GUILayout.Label($"最后: {lastLog}");

        GUILayout.Space(5);
        GUILayout.Label("Test ResId:");
        testResId = GUILayout.TextField(testResId);

        if (GUILayout.Button($"加载 ({testResId})"))
            StartCoroutine(LoadSingle(testResId));
        if (GUILayout.Button("加载全部")) StartCoroutine(LoadAll());
        if (GUILayout.Button("卸载全部")) UnloadAllTest();

        GUILayout.Space(10);
        GUILayout.Label("快速加载:");
        int max = Mathf.Min(9, _testResIds.Count);
        for (int i = 0; i < max; i++)
        {
            int idx = i;
            if (GUILayout.Button($"{i + 1}: {_testResIds[idx]}"))
                StartCoroutine(LoadAt(idx));
        }
        GUILayout.EndArea();
    }
#endif
}
