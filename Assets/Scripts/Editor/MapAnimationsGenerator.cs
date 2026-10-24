using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.Linq;

public class MapAnimationsGenerator : EditorWindow
{
    private static readonly string BASE_PATH = "Resources/Sprites/Maps/Anim";
    private static readonly string ANIMATIONS_GENERATED_FOLDER = "Animations-Generated";
    private static readonly string PREFABS_FOLDER = "Prefabs";

    [MenuItem("Tools/Texturepacker生成地图序列帧动画")]
    public static void GenerateMapAnimations()
    {
        // 先刷新数据库
        AssetDatabase.Refresh();

        string rootPath = Path.Combine(Application.dataPath, BASE_PATH);

        if (!Directory.Exists(rootPath))
        {
            Debug.LogError($"路径不存在: {rootPath}");
            return;
        }

        // 获取所有序号文件夹
        string[] folderPaths = Directory.GetDirectories(rootPath, "-*");

        if (folderPaths.Length == 0)
        {
            Debug.LogWarning($"在 {rootPath} 中没有找到以'-'开头的文件夹");
            return;
        }

        int totalProcessed = 0;
        int totalSkipped = 0;

        foreach (string folderPath in folderPaths)
        {
            string folderName = Path.GetFileName(folderPath);
            Debug.Log($"开始处理文件夹: {folderName}");

            (int processed, int skipped) = ProcessFolder(folderPath, folderName);
            totalProcessed += processed;
            totalSkipped += skipped;
        }

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();

        if (totalSkipped > 0)
        {
            Debug.Log($"完成！已处理: {totalProcessed} 个, 跳过: {totalSkipped} 个(已存在)");
        }
        else
        {
            Debug.Log($"完成！已处理: {totalProcessed} 个");
        }
    }

    private static (int processed, int skipped) ProcessFolder(string folderPath, string folderName)
    {
        int processed = 0;
        int skipped = 0;

        // 在文件夹内创建Animations-Generated文件夹1
        string animGeneratedFolderPath = Path.Combine(folderPath, ANIMATIONS_GENERATED_FOLDER);
        if (!Directory.Exists(animGeneratedFolderPath))
        {
            Directory.CreateDirectory(animGeneratedFolderPath);
        }

        // 在Anim目录下创建Prefabs文件夹
        string prefabsRootPath = Path.Combine(Application.dataPath, BASE_PATH, PREFABS_FOLDER);
        if (!Directory.Exists(prefabsRootPath))
        {
            Directory.CreateDirectory(prefabsRootPath);
        }

        // 获取所有图片文件
        string pattern = $"Map_{folderName}_*.png";
        string[] imageFiles = Directory.GetFiles(folderPath, pattern);

        if (imageFiles.Length == 0)
        {
            Debug.LogWarning($"在 {folderName} 中没有找到符合命名规范 {pattern} 的图片");
            return (0, 0);
        }

        Debug.Log($"找到 {imageFiles.Length} 个图片文件");

        foreach (string imageFile in imageFiles)
        {
            if (imageFile.EndsWith(".meta"))
                continue;

            string fileName = Path.GetFileNameWithoutExtension(imageFile);
            if (ProcessImageFile(imageFile, fileName, folderPath, animGeneratedFolderPath, prefabsRootPath))
            {
                processed++;
            }
            else
            {
                skipped++;
            }
        }

        return (processed, skipped);
    }

    private static bool ProcessImageFile(string imagePath, string fileName, string folderPath,
                                        string animGeneratedFolderPath, string prefabsRootPath)
    {
        Debug.Log($"处理文件: {fileName}");

        // 相对路径
        string relativeImagePath = GetRelativeAssetPath(imagePath);

        // 生成各个文件的相对路径
        // Anim Clip 放在 Animations-Generated 文件夹
        string relativeAnimClipPath = GetRelativeAssetPath(Path.Combine(animGeneratedFolderPath, $"{fileName}.anim"));

        // Controller 放在 folderPath 目录下（-50007目录下）
        string relativeControllerPath = GetRelativeAssetPath(Path.Combine(folderPath, $"{fileName}.controller"));

        // Prefab 放在 Prefabs 文件夹
        string relativePrefabPath = GetRelativeAssetPath(Path.Combine(prefabsRootPath, $"{fileName}.prefab"));

        // 检查是否已存在
        bool animClipExists = AssetDatabase.LoadAssetAtPath<AnimationClip>(relativeAnimClipPath) != null;
        bool controllerExists = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(relativeControllerPath) != null;
        bool prefabExists = AssetDatabase.LoadAssetAtPath<GameObject>(relativePrefabPath) != null;
        //  && controllerExists && prefabExists
        if (animClipExists)
        {
            Debug.Log($"文件已存在，跳过: {fileName}");
            return false;
        }

        // 获取图集中的所有子精灵
        List<Sprite> sprites = GetSpritesFromTextureAtlas(relativeImagePath);
        if (sprites == null || sprites.Count == 0)
        {
            Debug.LogWarning($"在 {fileName} 中没有找到精灵");
            return false;
        }

        Debug.Log($"从 {fileName} 中找到 {sprites.Count} 个子精灵");

        // 创建动画Clip
        if (!animClipExists)
        {
            CreateAnimationClip(sprites, relativeAnimClipPath, fileName);
        }

        // 创建Animator Controller
        if (!controllerExists)
        {
            CreateAnimatorController(relativeAnimClipPath, relativeControllerPath, fileName);
        }

        // 创建预制体
        if (!prefabExists)
        {
            CreatePrefab(relativeImagePath, relativeControllerPath, relativePrefabPath, fileName, sprites[0]);
        }

        return true;
    }

    private static List<Sprite> GetSpritesFromTextureAtlas(string texturePath)
    {
        // 确保路径使用正斜杠
        texturePath = texturePath.Replace("\\", "/");

        // 加载所有子对象
        Object[] allObjects = AssetDatabase.LoadAllAssetsAtPath(texturePath);

        if (allObjects == null || allObjects.Length == 0)
        {
            Debug.LogError($"无法加载资源: {texturePath}");
            return new List<Sprite>();
        }

        // 筛选出所有Sprite对象
        List<Sprite> sprites = new List<Sprite>();
        foreach (Object obj in allObjects)
        {
            if (obj is Sprite sprite)
            {
                sprites.Add(sprite);
            }
        }

        // 移除主纹理（如果有）
        sprites.RemoveAll(s => s.name == Path.GetFileNameWithoutExtension(texturePath));

        // 按名称排序
        sprites = sprites.OrderBy(s => s.name).ToList();

        return sprites;
    }

    private static void CreateAnimationClip(List<Sprite> sprites, string animClipPath, string clipName)
    {
        if (sprites == null || sprites.Count == 0)
        {
            Debug.LogError($"无法创建动画，没有找到精灵");
            return;
        }

        // 确保路径使用正斜杠
        animClipPath = animClipPath.Replace("\\", "/");

        // 创建动画剪辑
        AnimationClip clip = new AnimationClip();
        clip.name = clipName;
        clip.frameRate = 10f;

        // 创建动画曲线
        EditorCurveBinding spriteBinding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite"
        };

        // 创建关键帧
        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Count];
        for (int i = 0; i < sprites.Count; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = i / clip.frameRate,
                value = sprites[i]
            };
        }

        // 设置动画曲线
        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes);

        // 设置动画循环
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        // 保存
        AssetDatabase.CreateAsset(clip, animClipPath);
        Debug.Log($"动画剪辑已保存: {animClipPath}");
    }

    private static void CreateAnimatorController(string animClipPath, string controllerPath, string controllerName)
    {
        // 确保路径使用正斜杠
        animClipPath = animClipPath.Replace("\\", "/");
        controllerPath = controllerPath.Replace("\\", "/");

        // 加载动画剪辑
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(animClipPath);
        if (clip == null)
        {
            Debug.LogError($"无法加载动画剪辑: {animClipPath}");
            return;
        }

        // 创建Animator Controller
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

        // 添加动画状态
        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        AnimatorState state = stateMachine.AddState(clip.name);
        state.motion = clip;
        stateMachine.defaultState = state;

        Debug.Log($"Animator Controller已保存: {controllerPath}");
    }

    private static void CreatePrefab(string imageAssetPath, string controllerPath,
                                   string prefabPath, string prefabName, Sprite defaultSprite)
    {
        // 确保路径使用正斜杠
        imageAssetPath = imageAssetPath.Replace("\\", "/");
        controllerPath = controllerPath.Replace("\\", "/");
        prefabPath = prefabPath.Replace("\\", "/");

        // 创建GameObject
        GameObject go = new GameObject(prefabName);

        // 添加SpriteRenderer
        SpriteRenderer spriteRenderer = go.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = defaultSprite;

        // 添加Animator
        Animator animator = go.AddComponent<Animator>();

        // 加载Animator Controller
        RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
        if (controller != null)
        {
            animator.runtimeAnimatorController = controller;
        }
        else
        {
            Debug.LogWarning($"无法加载Animator Controller: {controllerPath}");
        }

        // 保存预制体
        PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        Object.DestroyImmediate(go);

        Debug.Log($"预制体已保存: {prefabPath}");
    }

    private static string GetRelativeAssetPath(string fullPath)
    {
        if (fullPath.StartsWith(Application.dataPath))
        {
            return "Assets" + fullPath.Substring(Application.dataPath.Length);
        }
        return fullPath;
    }

    [MenuItem("Tools/检查图集状态")]
    public static void CheckAtlasStatus()
    {
        string rootPath = Path.Combine(Application.dataPath, BASE_PATH);

        if (!Directory.Exists(rootPath))
        {
            Debug.LogError($"路径不存在: {rootPath}");
            return;
        }

        // 获取所有序号文件夹
        string[] folderPaths = Directory.GetDirectories(rootPath, "-*");

        foreach (string folderPath in folderPaths)
        {
            string folderName = Path.GetFileName(folderPath);
            string pattern = $"Map_{folderName}_*.png";
            string[] imageFiles = Directory.GetFiles(folderPath, pattern);

            Debug.Log($"检查文件夹: {folderName}");

            foreach (string imageFile in imageFiles)
            {
                if (imageFile.EndsWith(".meta"))
                    continue;

                string relativePath = GetRelativeAssetPath(imageFile);
                TextureImporter importer = AssetImporter.GetAtPath(relativePath) as TextureImporter;

                if (importer != null)
                {
                    // 加载所有精灵
                    Object[] allObjects = AssetDatabase.LoadAllAssetsAtPath(relativePath);
                    int spriteCount = 0;
                    foreach (Object obj in allObjects)
                    {
                        if (obj is Sprite)
                            spriteCount++;
                    }

                    Debug.Log($"  {Path.GetFileName(imageFile)}: " +
                             $"Type={importer.textureType}, " +
                             $"Mode={importer.spriteImportMode}, " +
                             $"精灵数量={spriteCount}");
                }
            }
        }

        Debug.Log("检查完成");
    }
}