using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class BatchTextureMaxSizeSetter : EditorWindow
{
    private string targetFolderPath = "Assets";
    private int maxSize = 256;
    private Vector2 scrollPosition;
    private List<string> processedTextures = new List<string>();
    private int totalTextures;
    private int processedCount;

    [MenuItem("Tools/批量设置图片MaxSize")]
    public static void ShowWindow()
    {
        GetWindow<BatchTextureMaxSizeSetter>("批量设置图片MaxSize");
    }

    private void OnGUI()
    {
        GUILayout.Space(10);

        // 目标文件夹设置
        EditorGUILayout.LabelField("目标文件夹设置", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        targetFolderPath = EditorGUILayout.TextField("文件夹路径", targetFolderPath);
        if (GUILayout.Button("选择文件夹", GUILayout.Width(80)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("选择目标文件夹", "Assets", "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                // 转换为相对路径
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    targetFolderPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                }
                else
                {
                    targetFolderPath = selectedPath;
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        // Max Size 设置
        maxSize = EditorGUILayout.IntSlider("Max Size", maxSize, 32, 8192);

        GUILayout.Space(20);

        // 操作按钮
        if (GUILayout.Button("开始处理", GUILayout.Height(30)))
        {
            ProcessTexturesInFolder();
        }

        if (GUILayout.Button("仅预览不应用", GUILayout.Height(25)))
        {
            PreviewTextures();
        }

        // 显示处理结果
        if (processedTextures.Count > 0)
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField($"找到 {totalTextures} 个图片文件", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"已处理 {processedCount} 个文件");

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
            foreach (string texturePath in processedTextures)
            {
                EditorGUILayout.LabelField(texturePath);
            }
            EditorGUILayout.EndScrollView();
        }
    }

    private void ProcessTexturesInFolder()
    {
        if (!Directory.Exists(targetFolderPath))
        {
            EditorUtility.DisplayDialog("错误", "指定的文件夹不存在！", "确定");
            return;
        }

        string[] imagePaths = GetAllImageFiles(targetFolderPath);
        totalTextures = imagePaths.Length;
        processedCount = 0;
        processedTextures.Clear();

        if (totalTextures == 0)
        {
            EditorUtility.DisplayDialog("提示", "在指定文件夹中未找到图片文件！", "确定");
            return;
        }

        // 开始批量处理
        AssetDatabase.StartAssetEditing();

        try
        {
            for (int i = 0; i < imagePaths.Length; i++)
            {
                string imagePath = imagePaths[i];

                // 更新进度条
                if (EditorUtility.DisplayCancelableProgressBar("处理中",
                    $"正在处理: {Path.GetFileName(imagePath)}", (float)i / imagePaths.Length))
                {
                    break;
                }

                SetTextureMaxSize(imagePath, maxSize);
                processedTextures.Add(imagePath);
                processedCount++;
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.Refresh();
        Debug.Log($"批量处理完成！共处理 {processedCount} 个图片文件。");
    }

    private void PreviewTextures()
    {
        string[] imagePaths = GetAllImageFiles(targetFolderPath);
        totalTextures = imagePaths.Length;
        processedTextures = new List<string>(imagePaths);
        processedCount = 0;

        Debug.Log($"找到 {totalTextures} 个图片文件（预览模式，未实际修改）:");
        foreach (string path in imagePaths)
        {
            Debug.Log(path);
        }
    }

    private string[] GetAllImageFiles(string folderPath)
    {
        List<string> imageFiles = new List<string>();

        try
        {
            // 递归获取所有子文件夹
            string[] allSubfolders = Directory.GetDirectories(folderPath, "*", SearchOption.AllDirectories);
            List<string> allFolders = new List<string>(allSubfolders);
            allFolders.Insert(0, folderPath); // 包含根文件夹

            // 支持的图片格式
            string[] imageExtensions = { "*.png", "*.jpg", "*.jpeg", "*.tga", "*.bmp", "*.psd", "*.tiff", "*.exr" };

            foreach (string folder in allFolders)
            {
                foreach (string extension in imageExtensions)
                {
                    try
                    {
                        string[] files = Directory.GetFiles(folder, extension);
                        foreach (string file in files)
                        {
                            // 转换为相对路径并确保路径格式正确
                            string relativePath = file.Replace("\\", "/");
                            if (relativePath.StartsWith(Application.dataPath))
                            {
                                relativePath = "Assets" + relativePath.Substring(Application.dataPath.Length);
                            }
                            imageFiles.Add(relativePath);
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"访问文件夹 {folder} 时出错: {e.Message}");
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"遍历文件夹时出错: {e.Message}");
        }

        return imageFiles.ToArray();
    }

    private void SetTextureMaxSize(string imagePath, int newMaxSize)
    {
        TextureImporter textureImporter = AssetImporter.GetAtPath(imagePath) as TextureImporter;
        if (textureImporter != null)
        {
            // 保存原始设置以便需要时可以恢复
            int originalMaxSize = textureImporter.maxTextureSize;

            // 设置新的Max Size
            textureImporter.maxTextureSize = newMaxSize;

            // 应用更改
            textureImporter.SaveAndReimport();

            Debug.Log($"已设置 {imagePath} 的Max Size为: {newMaxSize} (原值: {originalMaxSize})");
        }
        else
        {
            Debug.LogWarning($"无法获取 {imagePath} 的TextureImporter");
        }
    }
}