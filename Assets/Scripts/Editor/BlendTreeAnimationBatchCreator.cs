using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Animations;
public class BlendTreeAnimationBatchCreator : EditorWindow
{
    private string petsRootFolder = "Assets/PetsResource/Pets/";
    private string outputRootFolder = "Assets/PetsResource/Generated/";
    private bool skipExisting = true;
    private bool isMountScript = true;
    private bool animType = true;

    [MenuItem("Tools/精灵动画混合树批量生成")]
    public static void ShowWindow()
    {
        GetWindow<BlendTreeAnimationBatchCreator>("Blend Tree Batch Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("混合树动画批量生成器", EditorStyles.boldLabel);
        // 宠物精灵根目录选择
        petsRootFolder = EditorGUILayout.TextField("精灵根目录:", petsRootFolder);
        outputRootFolder = EditorGUILayout.TextField("输出目录:", outputRootFolder);

        skipExisting = EditorGUILayout.Toggle("跳过已存在资源", skipExisting);
        isMountScript = EditorGUILayout.Toggle("是否挂载脚本", isMountScript);
        animType = EditorGUILayout.Toggle("动画类型，勾选是sprite，否则是image", animType);

        if (GUILayout.Button("批量生成精灵动画"))
        {
            BatchGenerateAllPets();
        }
    }

    private void BatchGenerateAllPets()
    {
        if (!Directory.Exists(petsRootFolder))
        {
            Debug.LogError($"目录不存在: {petsRootFolder}");
            return;
        }

        // 提前刷新资源数据库，确保检测准确
        AssetDatabase.Refresh();

        string[] petFolders = Directory.GetDirectories(petsRootFolder);
        int skippedCount = 0;
        int processedCount = 0;
        int errorCount = 0;

        foreach (string petFolder in petFolders)
        {
            string petName = Path.GetFileName(petFolder);

            try
            {
                // 检查是否应该跳过生成
                if (skipExisting && ShouldSkipGeneration(petName))
                {
                    skippedCount++;
                    Debug.Log($"跳过 {petName}：资源已存在");
                    continue;
                }

                GeneratePetAnimations(petFolder, petName);
                processedCount++;
            }
            catch (System.Exception e)
            {
                errorCount++;
                Debug.LogError($"生成宠物 {petName} 动画时出错: {e.Message}");
                Debug.LogError($"堆栈跟踪: {e.StackTrace}");
            }
        }

        AssetDatabase.Refresh();

        // 显示详细的生成报告
        if (skippedCount > 0 || errorCount > 0)
        {
            Debug.LogWarning($"批量生成完成！成功: {processedCount}只，跳过: {skippedCount}只，失败: {errorCount}只");
        }
        else
        {
            Debug.Log($"批量生成完成！共处理 {processedCount} 只精灵");
        }
    }

    /// <summary>
    /// 检查是否应该跳过资源生成（基于资源存在性）
    /// </summary>
    private bool ShouldSkipGeneration(string petName)
    {
        // 检查预制体是否存在
        string prefabPath = $"{outputRootFolder}Prefabs/{petName}.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
        {
            Debug.Log($"检测到已存在预制体: {prefabPath}");
            return true;
        }

        // 检查动画控制器是否存在
        string controllerPath = $"{outputRootFolder}Controllers/{petName}_Controller.controller";
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) != null)
        {
            Debug.Log($"检测到已存在动画控制器: {controllerPath}");
            return true;
        }

        // 检查动画剪辑目录是否存在且包含有效文件
        string animFolderPath = $"{outputRootFolder}Animations/{petName}";
        if (Directory.Exists(animFolderPath))
        {
            string[] existingClips = Directory.GetFiles(animFolderPath, "*.anim");
            if (existingClips.Length >= 8) // 假设每个宠物有8个方向的动画
            {
                Debug.Log($"检测到已存在动画剪辑: {animFolderPath} ({existingClips.Length}个)");
                return true;
            }
        }

        return false;
    }

    private void GeneratePetAnimations(string petFolder, string petName)
    {
        // 在生成前进行最终确认检查
        if (skipExisting && CheckResourceExistenceDetailed(petName))
        {
            Debug.Log($"确认跳过 {petName} 的生成");
            return;
        }

        try
        {
            // 8个方向的帧范围[1](@ref)
            Dictionary<string, int[]> directionFrames = new Dictionary<string, int[]>()
        {
            {"down", new int[]{1, 7}},
            {"leftdown", new int[]{8, 15}},
            {"left", new int[]{16, 23}},
            {"leftup", new int[]{24, 31}},
            {"up", new int[]{32, 39}},
            {"rightup", new int[]{40, 47}},
            {"right", new int[]{48, 55}},
            {"rightdown", new int[]{56, 63}}
        };

            // 创建输出目录
            string animOutputPath = $"{outputRootFolder}Animations/{petName}/";
            string controllerOutputPath = $"{outputRootFolder}Controllers/";
            string prefabOutputPath = $"{outputRootFolder}Prefabs/";

            CreateDirectoryIfNotExists(animOutputPath);
            CreateDirectoryIfNotExists(controllerOutputPath);
            CreateDirectoryIfNotExists(prefabOutputPath);

            // 生成每个方向的动画剪辑
            List<AnimationClip> directionClips = new List<AnimationClip>();
            foreach (var direction in directionFrames)
            {
                // 检查动画剪辑是否已存在
                string clipPath = $"{animOutputPath}{petName}_{direction.Key}.anim";
                if (skipExisting && AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath) != null)
                {
                    Debug.Log($"跳过已存在的动画剪辑: {clipPath}");
                    AnimationClip existingClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                    if (existingClip != null)
                    {
                        directionClips.Add(existingClip);
                    }
                    continue;
                }

                AnimationClip clip = CreateDirectionAnimationClip(petFolder, petName, direction.Key, direction.Value[0], direction.Value[1]);
                if (clip != null)
                {
                    directionClips.Add(clip);
                }
            }

            if (directionClips.Count == 0)
            {
                Debug.LogWarning($"没有为精灵 {petName} 创建新的动画剪辑（可能已全部存在）");
                return;
            }

            // 创建混合树动画控制器（如果不存在）
            string controllerPath = $"{controllerOutputPath}{petName}_Controller.controller";
            if (!skipExisting || AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) == null)
            {
                CreateBlendTreeController(petName, directionClips, controllerOutputPath);
            }
            else
            {
                Debug.Log($"跳过已存在的控制器: {controllerPath}");
            }

            // 创建宠物预制体（如果不存在）
            string prefabPath = $"{prefabOutputPath}{petName}.prefab";
            if (!skipExisting || AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) == null)
            {
                CreatePetPrefab(petName, prefabOutputPath, controllerOutputPath);
            }
            else
            {
                Debug.Log($"跳过已存在的预制体: {prefabPath}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"生成宠物 {petName} 动画时出错: {e.Message}");
            throw;
        }
    }

    /// <summary>
    /// 增强版的资源存在性检查，提供详细报告
    /// </summary>
    private bool CheckResourceExistenceDetailed(string petName)
    {
        List<string> existingResources = new List<string>();

        string prefabPath = $"{outputRootFolder}Prefabs/{petName}.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            existingResources.Add("预制体");

        string controllerPath = $"{outputRootFolder}Controllers/{petName}_Controller.controller";
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) != null)
            existingResources.Add("动画控制器");

        string animFolderPath = $"{outputRootFolder}Animations/{petName}";
        if (Directory.Exists(animFolderPath))
        {
            string[] existingClips = Directory.GetFiles(animFolderPath, "*.anim");
            if (existingClips.Length > 0)
                existingResources.Add($"{existingClips.Length}个动画剪辑");
        }

        if (existingResources.Count > 0)
        {
            Debug.Log($"宠物 {petName} 已有资源: {string.Join("、", existingResources)}");
            return true;
        }

        return false;
    }

    // 创建目录的辅助方法
    private void CreateDirectoryIfNotExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            Debug.Log($"创建目录: {path}");
        }
    }

    private AnimationClip CreateDirectionAnimationClip(string petFolder, string petName, string direction, int startFrame, int endFrame)
    {
        string clipPath = $"{outputRootFolder}Animations/{petName}/{petName}_{direction}.anim";

        // 最终的存在性检查
        if (skipExisting && AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath) != null)
        {
            Debug.Log($"动画剪辑已存在，跳过: {clipPath}");
            return AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        }

        AnimationClip clip = new AnimationClip();
        clip.name = $"{petName}_{direction}";
        clip.frameRate = 12f;
        EditorCurveBinding spriteBinding;
        // 创建精灵曲线
        if (animType)
        {
             spriteBinding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };
        }
        else
        {
             spriteBinding = new EditorCurveBinding
            {
                type = typeof(UnityEngine.UI.Image), // 修改为 Image 类型
                path = "", // 如果 Image 在子节点，需指定路径
                propertyName = "m_Sprite" // 属性名保持不变，因为 Image 也使用 m_Sprite
            };
        }
      
    

        List<ObjectReferenceKeyframe> keyframes = new List<ObjectReferenceKeyframe>();
        float frameTime = 0f;

        for (int frameNum = startFrame; frameNum <= endFrame; frameNum++)
        {
            string spritePath = $"{petFolder}/{frameNum}.png";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

            if (sprite != null)
            {
                keyframes.Add(new ObjectReferenceKeyframe
                {
                    time = frameTime,
                    value = sprite
                });
            }
            else
            {
                Debug.LogWarning($"找不到精灵: {spritePath}");
            }

            frameTime += 1f / clip.frameRate;
        }

        if (keyframes.Count == 0)
        {
            Debug.LogError($"方向 {direction} 没有找到任何有效的精灵帧");
            return null;
        }

        // 设置动画曲线
        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes.ToArray());

        // 设置循环
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        // 保存动画剪辑
        AssetDatabase.CreateAsset(clip, clipPath);
        Debug.Log($"创建动画剪辑: {clipPath}");

        return clip;
    }

    private void CreateBlendTreeController(string petName, List<AnimationClip> directionClips, string outputPath)
    {
        string controllerPath = $"{outputPath}{petName}_Controller.controller";

        // 检查控制器是否已存在
        if (skipExisting && AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) != null)
        {
            Debug.Log($"控制器已存在，跳过创建: {controllerPath}");
            return;
        }

        // 创建Animator Controller[1](@ref)
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

        // 添加必需的参数[1](@ref)
        controller.AddParameter("DirectionX", AnimatorControllerParameterType.Float);
        controller.AddParameter("DirectionY", AnimatorControllerParameterType.Float);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);

        // 获取根状态机
        AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;

        // 创建混合树
        BlendTree blendTree = new BlendTree();
        blendTree.name = "DirectionBlend";
        blendTree.blendType = BlendTreeType.FreeformDirectional2D; // 2D自由方向混合
        blendTree.blendParameter = "DirectionX";
        blendTree.blendParameterY = "DirectionY";
        blendTree.useAutomaticThresholds = false;

        // 预定义8个方向的标准向量值（精确的归一化向量）
        Vector2[] directionVectors = new Vector2[]
        {
        new Vector2(0f, 1f),           // 上 (Up)
        new Vector2(0.7071f, 0.7071f), // 右上 (UpRight)
        new Vector2(1f, 0f),           // 右 (Right)
        new Vector2(0.7071f, -0.7071f), // 右下 (RightDown)
        new Vector2(0f, -1f),          // 下 (Down)
        new Vector2(-0.7071f, -0.7071f), // 左下 (LeftDown)
        new Vector2(-1f, 0f),          // 左 (Left)
        new Vector2(-0.7071f, 0.7071f)  // 左上 (LeftUp)
        };


        // 设置8个方向的混合阈值（每个45度）
        string[] directionOrder = { "up", "rightup", "right", "rightdown", "down", "leftdown", "left", "leftup" };

        for (int i = 0; i < directionOrder.Length; i++)
        {
            string direction = directionOrder[i];
            AnimationClip clip = directionClips.Find(c => c.name.EndsWith("_" + direction));

            if (clip != null)
            {
                // 直接使用预定义的精确向量，避免计算误差
                Vector2 threshold = directionVectors[i];
                blendTree.AddChild(clip, threshold);
                Debug.Log($"添加方向 {direction} 到混合树: ({threshold.x:F4}, {threshold.y:F4})");
            }
            else
            {
                Debug.LogWarning($"找不到方向 {direction} 的动画剪辑");
            }
        }

        // 创建混合树状态
        AnimatorState blendState = rootStateMachine.AddState("DirectionBlend", new Vector3(300, 100));
        blendState.motion = blendTree;

        // 设置默认状态
        rootStateMachine.defaultState = blendState;

        // 保存控制器
        AssetDatabase.AddObjectToAsset(blendTree, controller);
        AssetDatabase.SaveAssets();

        Debug.Log($"创建动画控制器: {controllerPath}");
    }

    private void CreatePetPrefab(string petName, string prefabPath, string controllerPath)
    {
        string prefabFullPath = $"{prefabPath}{petName}.prefab";

        // 检查预制体是否已存在
        if (skipExisting && AssetDatabase.LoadAssetAtPath<GameObject>(prefabFullPath) != null)
        {
            Debug.Log($"预制体已存在，跳过创建: {prefabFullPath}");
            return;
        }

        // 创建GameObject
        GameObject petObject = new GameObject(petName);

        try
        {
            // 添加组件
            SpriteRenderer spriteRenderer = petObject.AddComponent<SpriteRenderer>();
            Animator animator = petObject.AddComponent<Animator>();

            // 设置Animator Controller
            string controllerFullPath = $"{controllerPath}{petName}_Controller.controller";
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerFullPath);

            if (controller != null)
            {
                animator.runtimeAnimatorController = controller;
                Debug.Log($"为预制体 {petName} 设置控制器: {controllerFullPath}");
            }
            else
            {
                Debug.LogError($"找不到控制器: {controllerFullPath}");
                // 创建默认控制器作为备选
                CreateFallbackController(petName, controllerPath, animator);
            }

            // 设置默认精灵（第一帧）
            string firstSpritePath = $"{petsRootFolder}{petName}/1.png";
            Sprite firstSprite = AssetDatabase.LoadAssetAtPath<Sprite>(firstSpritePath);
            if (firstSprite != null)
            {
                spriteRenderer.sprite = firstSprite;
            }
            else
            {
                Debug.LogWarning($"找不到默认精灵: {firstSpritePath}");
            }

            // 添加动画控制脚本

            if (isMountScript)
            {
                petObject.AddComponent<PetBlendTreeController>();
            }

            // 保存为预制体
            PrefabUtility.SaveAsPrefabAsset(petObject, prefabFullPath);

            Debug.Log($"创建预制体: {prefabFullPath}");
        }
        finally
        {
            // 确保销毁临时对象
            if (petObject != null)
            {
                DestroyImmediate(petObject);
            }
        }
    }

    private void CreateFallbackController(string petName, string controllerPath, Animator animator)
    {
        string fallbackPath = $"{controllerPath}{petName}_Fallback.controller";

        // 检查备用控制器是否已存在
        if (skipExisting && AssetDatabase.LoadAssetAtPath<AnimatorController>(fallbackPath) != null)
        {
            animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(fallbackPath);
            return;
        }

        // 创建简单的备用控制器
        AnimatorController fallbackController = AnimatorController.CreateAnimatorControllerAtPath(fallbackPath);

        // 添加必需参数
        fallbackController.AddParameter("DirectionX", AnimatorControllerParameterType.Float);
        fallbackController.AddParameter("DirectionY", AnimatorControllerParameterType.Float);

        animator.runtimeAnimatorController = fallbackController;
        Debug.LogWarning($"创建备用控制器: {fallbackPath}");
    }
}