using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace Prankard.FlashSpriteSheetImporter
{
    public class SpriteSheetImporterWindow : EditorWindow
    {
        private static Dictionary<SpriteDataFormat, ISpriteSheetParser> spriteParsers = new Dictionary<SpriteDataFormat, ISpriteSheetParser>()
        {
            { SpriteDataFormat.StarlingOrSparrowV2, new StarlingParser() }
        };

        [MenuItem("Window/精灵表批量生成")]
        static void Init()
        {
            SpriteSheetImporterWindow window = (SpriteSheetImporterWindow)EditorWindow.GetWindow(typeof(SpriteSheetImporterWindow));
            window.titleContent = new GUIContent("精灵表生成", "精灵表生成");
            window.Show();
        }

        private Vector2 customPivot = Vector2.zero;
        private Texture2D spriteSheet;
        private TextAsset textAsset;
        private SpriteDataFormat dataFormat = SpriteDataFormat.StarlingOrSparrowV2;
        private SpriteAlignment spriteAlignment = SpriteAlignment.TopLeft;
        private bool forcePivotOverwrite = false;
        private bool generateSpriteSheet = false;
        private bool generateAnimationController = false;
        private bool generateGameObject = false;
        private float fps = 24.0f;
        private string errorMessage = null;

        // 新增：文件夹批量处理相关字段
        private string folderPath = "";
        private bool includeSubfolders = true;
        private string fileFilter = "*.png";
        private Vector2 scrollPosition;

        void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // 原有的单文件导入界面
            GUILayout.Label("单文件导入", EditorStyles.boldLabel);
            DrawSingleFileImportSection();

            GUILayout.Space(20);

            // 新增：文件夹批量导入界面
            GUILayout.Label("文件夹批量导入", EditorStyles.boldLabel);
            DrawBatchImportSection();

            EditorGUILayout.EndScrollView();
        }

        private void DrawSingleFileImportSection()
        {
            Texture2D newSpriteSheet = (Texture2D)EditorGUILayout.ObjectField("精灵表", spriteSheet, typeof(Texture2D), false);
            if (newSpriteSheet != null)
            {
                if (spriteSheet != newSpriteSheet)
                {
                    string assetPath = AssetDatabase.GetAssetPath(newSpriteSheet);
                    foreach (ISpriteSheetParser parser in spriteParsers.Values)
                    {
                        var dataAssetPath = Path.GetDirectoryName(assetPath) + "/" + Path.GetFileNameWithoutExtension(assetPath) + "." + parser.FileExtension;
                        TextAsset searchTextAsset = AssetDatabase.LoadAssetAtPath(dataAssetPath, typeof(TextAsset)) as TextAsset;
                        if (searchTextAsset != null)
                        {
                            textAsset = searchTextAsset;
                            break;
                        }
                    }
                }
                spriteSheet = newSpriteSheet;
            }
            else
                spriteSheet = null;

            textAsset = (TextAsset)EditorGUILayout.ObjectField("数据文件", textAsset, typeof(TextAsset), false);
            dataFormat = (SpriteDataFormat)EditorGUILayout.EnumPopup("数据格式", dataFormat);

            DrawImportOptions();

            if (textAsset != null && spriteSheet != null)
            {
                if (GUILayout.Button("导入选中精灵表"))
                {
                    ImportSingleSpriteSheet();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("请选择精灵表和数据文件", MessageType.Info);
            }
        }

        private void DrawBatchImportSection()
        {
            // 文件夹选择
            EditorGUILayout.BeginHorizontal();
            folderPath = EditorGUILayout.TextField("文件夹路径", folderPath);
            if (GUILayout.Button("浏览", GUILayout.Width(60)))
            {
                // D:\Users\hzy\UnityProjects\SeerGo\Assets\Resources\Sprites\Maps\Anim
                string selectedPath = EditorUtility.OpenFolderPanel("选择包含精灵表的文件夹", "Assets\\Resources\\Sprites\\Maps\\Anim", "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    folderPath = selectedPath;
                }
            }
            EditorGUILayout.EndHorizontal();

            includeSubfolders = EditorGUILayout.Toggle("包含子文件夹", includeSubfolders);
            fileFilter = EditorGUILayout.TextField("文件过滤", fileFilter);

            DrawImportOptions();

            if (GUILayout.Button("批量导入文件夹中的精灵表", GUILayout.Height(30)))
            {
                if (Directory.Exists(folderPath))
                {
                    BatchImportSpriteSheets();
                }
                else
                {
                    errorMessage = "文件夹不存在: " + folderPath;
                    Debug.LogError(errorMessage);
                }
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
            }
        }

        private void DrawImportOptions()
        {
            GUILayout.Label("导入选项", EditorStyles.boldLabel);
            forcePivotOverwrite = EditorGUILayout.Toggle("强制覆盖轴心点", forcePivotOverwrite);

            if (forcePivotOverwrite)
            {
                spriteAlignment = (SpriteAlignment)EditorGUILayout.EnumPopup("精灵轴心", spriteAlignment);
                if (spriteAlignment == SpriteAlignment.Custom)
                {
                    customPivot = EditorGUILayout.Vector2Field("自定义轴心", customPivot);
                }
            }

            GUILayout.Label("动画选项", EditorStyles.boldLabel);
            generateSpriteSheet = EditorGUILayout.Toggle("生成动画", generateSpriteSheet);
            if (generateSpriteSheet)
            {
                fps = EditorGUILayout.FloatField("帧率", fps);
                if (generateGameObject)
                {
                    GUI.enabled = false;
                    EditorGUILayout.Toggle("创建动画控制器", true);
                    GUI.enabled = true;
                }
                else
                {
                    generateAnimationController = EditorGUILayout.Toggle("创建动画控制器", generateGameObject ? true : generateAnimationController);
                }
                generateGameObject = EditorGUILayout.Toggle("创建游戏对象", generateGameObject);
            }
        }

        private void ImportSingleSpriteSheet()
        {
            errorMessage = null;
            Vector2 size = GetOriginalSize(spriteSheet);

            if (size.x != spriteSheet.width && size.y != spriteSheet.height)
            {
                errorMessage = "精灵表尺寸不匹配。原始尺寸: " + size.x + "x" + size.y + "，当前尺寸: " + spriteSheet.width + "x" + spriteSheet.height;
                Debug.LogWarning(errorMessage);
                return;
            }

            if (spriteParsers[dataFormat].ParseAsset(spriteSheet, textAsset, forcePivotOverwrite ? PivotValue : new Vector2(0f, 1.0f), forcePivotOverwrite))
            {
                Debug.Log("成功导入精灵: " + spriteSheet.name);
                if (generateSpriteSheet)
                {
                    AnimationCreator.GenerateAnimation(spriteSheet, fps, generateAnimationController, generateGameObject);
                }
            }
            else
            {
                errorMessage = "解析资源失败: " + spriteSheet.name;
                Debug.LogError(errorMessage);
            }
        }

        private void BatchImportSpriteSheets()
        {
            errorMessage = null;
            int successCount = 0;
            int totalCount = 0;

            // 获取所有符合条件的纹理文件 [1,3](@ref)
            SearchOption searchOption = includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            string[] textureFiles = Directory.GetFiles(folderPath, fileFilter, searchOption);

            EditorUtility.DisplayProgressBar("批量导入精灵表", "搜索文件中...", 0f);

            try
            {
                foreach (string filePath in textureFiles)
                {
                    totalCount++;
                    float progress = (float)totalCount / textureFiles.Length;
                    EditorUtility.DisplayProgressBar("批量导入精灵表", "处理: " + Path.GetFileName(filePath), progress);

                    string relativePath = GetRelativeAssetPath(filePath);
                    if (string.IsNullOrEmpty(relativePath))
                        continue;

                    Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(relativePath);
                    if (texture == null)
                        continue;

                    // 查找对应的数据文件 [1](@ref)
                    TextAsset dataFile = FindDataFileForTexture(relativePath);
                    if (dataFile == null)
                    {
                        Debug.LogWarning("未找到数据文件: " + relativePath);
                        continue;
                    }

                    // 导入单个精灵表
                    if (ImportTextureWithData(texture, dataFile))
                    {
                        successCount++;
                    }
                }

                AssetDatabase.Refresh();
                errorMessage = $"批量导入完成！成功: {successCount}/{textureFiles.Length}";
                Debug.Log(errorMessage);
            }
            catch (System.Exception e)
            {
                errorMessage = "批量导入失败: " + e.Message;
                Debug.LogError(errorMessage);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private TextAsset FindDataFileForTexture(string texturePath)
        {
            string directory = Path.GetDirectoryName(texturePath);
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(texturePath);

            foreach (ISpriteSheetParser parser in spriteParsers.Values)
            {
                string dataFilePath = directory + "/" + fileNameWithoutExt + "." + parser.FileExtension;
                TextAsset dataFile = AssetDatabase.LoadAssetAtPath<TextAsset>(dataFilePath);
                if (dataFile != null)
                    return dataFile;
            }

            return null;
        }

        private bool ImportTextureWithData(Texture2D texture, TextAsset dataAsset)
        {
            try
            {
                Vector2 size = GetOriginalSize(texture);
                if (size.x != texture.width && size.y != texture.height)
                {
                    Debug.LogWarning($"跳过 {texture.name}: 尺寸不匹配");
                    return false;
                }

                if (spriteParsers[dataFormat].ParseAsset(texture, dataAsset, forcePivotOverwrite ? PivotValue : new Vector2(0f, 1.0f), forcePivotOverwrite))
                {
                    Debug.Log("成功导入精灵: " + texture.name);
                    if (generateSpriteSheet)
                    {
                        AnimationCreator.GenerateAnimation(texture, fps, generateAnimationController, generateGameObject);
                    }
                    return true;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"导入失败 {texture.name}: {e.Message}");
            }

            return false;
        }

        private string GetRelativeAssetPath(string fullPath)
        {
            fullPath = fullPath.Replace("\\", "/");
            string assetsPath = Application.dataPath.Replace("\\", "/");

            if (fullPath.StartsWith(assetsPath))
            {
                return "Assets" + fullPath.Substring(assetsPath.Length);
            }

            return null;
        }

        public static Vector2 GetOriginalSize(Texture2D texture)
        {
            string assetPath = AssetDatabase.GetAssetPath(texture);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
                return new Vector2(texture.width, texture.height);

            object[] array = new object[] { 0, 0 };
            MethodInfo mi = typeof(TextureImporter).GetMethod("GetWidthAndHeight", BindingFlags.NonPublic | BindingFlags.Instance);
            mi.Invoke(importer, array);

            return new Vector2((int)array[0], (int)array[1]);
        }

        public Vector2 PivotValue
        {
            get
            {
                switch (spriteAlignment)
                {
                    case SpriteAlignment.TopLeft:
                        return new Vector2(0f, 1f);
                    case SpriteAlignment.TopCenter:
                        return new Vector2(0.5f, 1f);
                    case SpriteAlignment.TopRight:
                        return new Vector2(1f, 1f);
                    case SpriteAlignment.LeftCenter:
                        return new Vector2(0f, 0.5f);
                    case SpriteAlignment.Center:
                        return new Vector2(0.5f, 0.5f);
                    case SpriteAlignment.RightCenter:
                        return new Vector2(1f, 0.5f);
                    case SpriteAlignment.BottomLeft:
                        return new Vector2(0f, 0f);
                    case SpriteAlignment.BottomCenter:
                        return new Vector2(0.5f, 0f);
                    case SpriteAlignment.BottomRight:
                        return new Vector2(1f, 0f);
                    case SpriteAlignment.Custom:
                        return customPivot;
                    default:
                        throw new System.NotImplementedException("未知的轴心对齐方式: " + spriteAlignment.ToString());
                }
            }
        }
    }
}