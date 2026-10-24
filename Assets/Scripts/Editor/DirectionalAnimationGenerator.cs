using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class PetAnimationGenerator : EditorWindow
{
    private string petsRootPath = "Assets/PetsResource/Pets";
    private string outputPath = "Assets/PetsResource/Generated";

    [MenuItem("Tools/宠物动画批量生成器")]
    public static void ShowWindow()
    {
        GetWindow<PetAnimationGenerator>("宠物动画生成器");
    }

    private void OnGUI()
    {
        GUILayout.Label("宠物动画批量生成工具", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        petsRootPath = EditorGUILayout.TextField("宠物资源路径:", petsRootPath);
        outputPath = EditorGUILayout.TextField("输出路径:", outputPath);

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("文件结构: Generated/Animations/, Generated/Controllers/, Generated/Prefabs/", MessageType.Info);
        EditorGUILayout.Space();

        if (GUILayout.Button("生成所有宠物动画和预制体", GUILayout.Height(30)))
        {
            GenerateAllPetAnimations();
        }
    }

    private void GenerateAllPetAnimations()
    {
        if (!Directory.Exists(petsRootPath))
        {
            Debug.LogError($"宠物资源路径不存在: {petsRootPath}");
            EditorUtility.DisplayDialog("错误", $"宠物资源路径不存在: {petsRootPath}", "确定");
            return;
        }

        // 创建分类文件夹
        string animationsPath = Path.Combine(outputPath, "Animations");
        string controllersPath = Path.Combine(outputPath, "Controllers");
        string prefabsPath = Path.Combine(outputPath, "Prefabs");

        if (!Directory.Exists(animationsPath)) Directory.CreateDirectory(animationsPath);
        if (!Directory.Exists(controllersPath)) Directory.CreateDirectory(controllersPath);
        if (!Directory.Exists(prefabsPath)) Directory.CreateDirectory(prefabsPath);

        string[] petFolders = Directory.GetDirectories(petsRootPath, "Pet_*");

        if (petFolders.Length == 0)
        {
            Debug.LogWarning($"在路径 {petsRootPath} 下未找到Pet_开头的文件夹");
            EditorUtility.DisplayDialog("警告", "未找到任何Pet_开头的文件夹", "确定");
            return;
        }

        Debug.Log($"找到 {petFolders.Length} 个宠物文件夹，开始生成...");

        int successCount = 0;
        foreach (string petFolder in petFolders)
        {
            if (GeneratePetAnimations(petFolder, animationsPath, controllersPath, prefabsPath))
            {
                successCount++;
            }

            // 每个宠物生成后立即刷新，确保资源可用
            AssetDatabase.Refresh();
        }

        // 最终刷新确保所有资源都正确加载
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("完成", $"成功生成 {successCount}/{petFolders.Length} 个宠物的动画和预制体", "确定");
        Debug.Log($"宠物动画生成完成: {successCount}/{petFolders.Length} 成功");
    }

    private bool GeneratePetAnimations(string petFolderPath, string animationsPath, string controllersPath, string prefabsPath)
    {
        string petId = Path.GetFileName(petFolderPath);
        Debug.Log($"开始处理宠物: {petId}");

        try
        {
            var directions = new Dictionary<string, Vector2>
            {
                { "up", new Vector2(0, 1) },
                { "down", new Vector2(0, -1) },
                { "left", new Vector2(-1, 0) },
                { "right", new Vector2(1, 0) },
                { "leftup", new Vector2(-0.707f, 0.707f) },
                { "leftdown", new Vector2(-0.707f, -0.707f) },
                { "rightup", new Vector2(0.707f, 0.707f) },
                { "rightdown", new Vector2(0.707f, -0.707f) }
            };

            Dictionary<string, AnimationClip> directionClips = new Dictionary<string, AnimationClip>();
            Sprite defaultSprite = null;
            int validClipCount = 0;

            // 第一步：创建所有动画剪辑
            foreach (var direction in directions)
            {
                string directionPath = Path.Combine(petFolderPath, direction.Key);
                if (Directory.Exists(directionPath))
                {
                    if (direction.Key == "down")
                    {
                        defaultSprite = GetFirstSpriteFromDirection(directionPath);
                    }

                    AnimationClip clip = CreateDirectionAnimation(directionPath, $"{petId}_{direction.Key}");
                    if (clip != null)
                    {
                        string clipPath = Path.Combine(animationsPath, $"{petId}_{direction.Key}.anim");
                        AssetDatabase.CreateAsset(clip, clipPath);
                        directionClips[direction.Key] = clip;
                        validClipCount++;
                        Debug.Log($"创建方向动画: {direction.Key}");
                    }
                }
                else
                {
                    Debug.LogWarning($"宠物 {petId} 缺少方向文件夹: {direction.Key}");
                }
            }

            if (validClipCount == 0)
            {
                Debug.LogWarning($"宠物 {petId} 没有找到任何有效的动画资源");
                return false;
            }

            // 立即保存动画资源
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 第二步：创建Animator Controller
            AnimatorController controller = CreateAnimatorController(petId, directionClips, directions, controllersPath);
            if (controller == null)
            {
                Debug.LogError($"创建Animator Controller失败: {petId}");
                return false;
            }

            // 立即保存Controller
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 第三步：创建预制体
            if (CreatePetPrefab(petId, controller, defaultSprite, prefabsPath))
            {
                Debug.Log($"成功生成宠物: {petId}");
                return true;
            }

            return false;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"处理宠物 {petId} 时发生错误: {ex.Message}");
            return false;
        }
    }

    private Sprite GetFirstSpriteFromDirection(string directionPath)
    {
        try
        {
            string[] spritePaths = Directory.GetFiles(directionPath, "*.png", SearchOption.AllDirectories);
            if (spritePaths.Length == 0)
            {
                Debug.LogWarning($"在路径 {directionPath} 下未找到PNG图片");
                return null;
            }

            System.Array.Sort(spritePaths);
            Sprite firstSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePaths[0]);

            if (firstSprite != null)
            {
                Debug.Log($"找到默认精灵: {spritePaths[0]}");
            }

            return firstSprite;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"获取默认精灵失败 {directionPath}: {ex.Message}");
            return null;
        }
    }

    private AnimationClip CreateDirectionAnimation(string directionPath, string clipName)
    {
        try
        {
            string[] spritePaths = Directory.GetFiles(directionPath, "*.png", SearchOption.AllDirectories);
            if (spritePaths.Length == 0)
            {
                Debug.LogWarning($"在路径 {directionPath} 下未找到PNG图片");
                return null;
            }

            System.Array.Sort(spritePaths);

            AnimationClip clip = new AnimationClip();
            clip.name = clipName;
            clip.frameRate = 24;

            EditorCurveBinding spriteBinding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };

            List<ObjectReferenceKeyframe> keyframes = new List<ObjectReferenceKeyframe>();
            float timePerFrame = 1f / clip.frameRate;

            for (int i = 0; i < spritePaths.Length; i++)
            {
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePaths[i]);
                if (sprite != null)
                {
                    keyframes.Add(new ObjectReferenceKeyframe
                    {
                        time = i * timePerFrame,
                        value = sprite
                    });
                }
            }

            if (keyframes.Count == 0)
            {
                Debug.LogWarning($"无法加载任何精灵图片: {directionPath}");
                return null;
            }

            AnimationClipSettings settings = new AnimationClipSettings();
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes.ToArray());

            return clip;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"创建方向动画失败 {directionPath}: {ex.Message}");
            return null;
        }
    }

    private AnimatorController CreateAnimatorController(string petId, Dictionary<string, AnimationClip> directionClips,
                                         Dictionary<string, Vector2> directions, string controllersPath)
    {
        try
        {
            string controllerPath = Path.Combine(controllersPath, $"{petId}_Controller.controller");

            // 如果已存在，先删除旧的
            if (File.Exists(controllerPath))
            {
                AssetDatabase.DeleteAsset(controllerPath);
                AssetDatabase.Refresh();
            }

            // 创建Animator Controller
            var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

            if (controller == null)
            {
                Debug.LogError($"创建Animator Controller文件失败: {controllerPath}");
                return null;
            }

            // 确保控制器有层
            if (controller.layers.Length == 0)
            {
                Debug.LogWarning("创建的Animator Controller没有层，手动添加基础层");
                controller.AddLayer("Base Layer");
            }

            // 获取基础层
            AnimatorControllerLayer baseLayer = controller.layers[0];

            // 清除默认状态
            if (baseLayer.stateMachine.states.Length > 0)
            {
                var states = baseLayer.stateMachine.states;
                for (int i = states.Length - 1; i >= 0; i--)
                {
                    baseLayer.stateMachine.RemoveState(states[i].state);
                }
            }

            // 添加参数
            controller.AddParameter("DirectionX", AnimatorControllerParameterType.Float);
            controller.AddParameter("DirectionY", AnimatorControllerParameterType.Float);

            // 创建混合树
            var blendTree = new BlendTree();
            blendTree.name = "DirectionBlend";
            blendTree.blendType = BlendTreeType.FreeformDirectional2D;
            blendTree.blendParameter = "DirectionX";
            blendTree.blendParameterY = "DirectionY";
            blendTree.useAutomaticThresholds = false;

            // 添加子动画
            int validChildCount = 0;
            foreach (var direction in directions)
            {
                if (directionClips.ContainsKey(direction.Key) && directionClips[direction.Key] != null)
                {
                    blendTree.AddChild(directionClips[direction.Key], direction.Value);
                    validChildCount++;
                }
            }

            if (validChildCount == 0)
            {
                Debug.LogError($"混合树没有有效的子动画");
                return null;
            }

            // 将混合树添加到状态机
            var blendState = baseLayer.stateMachine.AddState("Movement");
            blendState.motion = blendTree;
            baseLayer.stateMachine.defaultState = blendState;

            // 保存混合树到控制器
            AssetDatabase.AddObjectToAsset(blendTree, controller);

            Debug.Log($"创建Animator Controller: {controllerPath}");
            return controller;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"创建Animator Controller失败 {petId}: {ex.Message}\n{ex.StackTrace}");
            return null;
        }
    }

    private bool CreatePetPrefab(string petId, AnimatorController controller, Sprite defaultSprite, string prefabsPath)
    {
        try
        {
            GameObject petObject = new GameObject(petId);

            // 添加SpriteRenderer
            var spriteRenderer = petObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sortingLayerName = "Characters";
            spriteRenderer.sortingOrder = 3;

            // 设置默认精灵
            if (defaultSprite != null)
            {
                spriteRenderer.sprite = defaultSprite;
                Debug.Log($"已设置默认精灵: {defaultSprite.name}");
            }
            else
            {
                Debug.LogWarning($"未找到down方向的默认精灵，将使用空精灵");
            }

            // 添加Animator并设置Controller
            var animator = petObject.AddComponent<Animator>();

            if (controller == null)
            {
                Debug.LogError($"Animator Controller为null，无法分配给宠物: {petId}");
                DestroyImmediate(petObject);
                return false;
            }

            // 直接使用传入的controller对象，避免路径加载问题
            animator.runtimeAnimatorController = controller;

            // 验证Animator设置
            if (animator.runtimeAnimatorController == null)
            {
                Debug.LogError($"Animator Controller分配失败: {petId}");
                DestroyImmediate(petObject);
                return false;
            }

            Debug.Log($"成功分配Animator Controller给宠物: {petId}");

            // 添加控制脚本
            var controllerScript = petObject.AddComponent<PetBlendTreeController>();
            if (controllerScript == null)
            {
                Debug.LogWarning($"添加PetBlendTreeController脚本失败，但继续创建预制体");
            }

            // 创建预制体
            string prefabPath = Path.Combine(prefabsPath, $"{petId}.prefab");

            // 如果预制体已存在，先删除
            if (File.Exists(prefabPath))
            {
                AssetDatabase.DeleteAsset(prefabPath);
                AssetDatabase.Refresh();
            }

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(petObject, prefabPath);
            DestroyImmediate(petObject);

            if (prefab != null)
            {
                // 验证预制体上的Animator设置
                var prefabAnimator = prefab.GetComponent<Animator>();
                if (prefabAnimator != null && prefabAnimator.runtimeAnimatorController != null)
                {
                    Debug.Log($"创建预制体成功: {prefabPath} (Animator Controller已正确挂载)");
                    return true;
                }
                else
                {
                    Debug.LogError($"预制体上的Animator Controller未正确保存: {prefabPath}");
                    return false;
                }
            }
            else
            {
                Debug.LogError($"创建预制体失败: {prefabPath}");
                return false;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"创建预制体失败 {petId}: {ex.Message}");
            return false;
        }
    }
}