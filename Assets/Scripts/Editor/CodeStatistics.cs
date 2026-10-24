using UnityEngine;
using UnityEditor;
using System.IO;

public class CodeStatistics : EditorWindow
{
    [MenuItem("Tools/代码统计")]
    public static void ShowWindow()
    {
        GetWindow<CodeStatistics>("代码统计");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("统计代码"))
        {
            CountCodeLines();
        }
    }

    private void CountCodeLines()
    {
        string assetsPath = Application.dataPath;
        var csFiles = Directory.GetFiles(assetsPath, "*.cs", SearchOption.AllDirectories);

        int totalLines = 0;
        int totalFiles = 0;

        foreach (string file in csFiles)
        {
            // 排除第三方库和Unity生成的文件
            if (file.Contains("Library") || file.Contains("PackageCache") || file.Contains("3rdParty"))
                continue;

            string[] lines = File.ReadAllLines(file);
            totalLines += lines.Length;
            totalFiles += 1;
        }

        Debug.Log($"代码统计结果：");
        Debug.Log($"- 脚本文件数：{totalFiles}");
        Debug.Log($"- 总代码行数：{totalLines}");
    }
}