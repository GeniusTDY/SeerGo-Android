using UnityEngine;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine.U2D;
using System.IO;
using System.Collections.Generic;


public class PetAtlasCreator : EditorWindow
{
    private static string petsFolder = "Assets/PetsResource/Pets";
    private static string atlasFolder = "Assets/PetsResource/Atlas";

    [MenuItem("Tools/Pets/生成所有宠物图集")]
    public static void CreateAllPetAtlases()
    {
        if (!Directory.Exists(atlasFolder))
        {
            Directory.CreateDirectory(atlasFolder);
        }

        // 清空旧的Atlas
        ClearOldAtlases();

        // 获取所有宠物文件夹
        string[] petFolders = Directory.GetDirectories(petsFolder, "Pet_*");

        foreach (string petFolder in petFolders)
        {
            CreatePetAtlases(petFolder);
        }

        AssetDatabase.Refresh();
        Debug.Log($"已生成 {petFolders.Length} 个宠物的所有图集");
    }

    private static void CreatePetAtlases(string petFolderPath)
    {
        string folderName = Path.GetFileName(petFolderPath);

        // 提取宠物序号
        int petNumber = 0;
        if (folderName.StartsWith("Pet_"))
        {
            string numberStr = folderName.Substring(4);
            int.TryParse(numberStr, out petNumber);
        }

        if (petNumber == 0)
        {
            Debug.LogWarning($"无法解析宠物序号: {folderName}");
            return;
        }

        // 获取所有朝向文件夹
        string[] directionFolders = Directory.GetDirectories(petFolderPath);

        foreach (string directionFolder in directionFolders)
        {
            string directionName = Path.GetFileName(directionFolder);
            CreateDirectionAtlas(petNumber, directionName, directionFolder);
        }
    }

    private static void CreateDirectionAtlas(int petNumber, string directionName, string directionFolderPath)
    {
        // 获取该方向文件夹中的所有png图片
        string[] imagePaths = Directory.GetFiles(directionFolderPath, "*.png", SearchOption.TopDirectoryOnly);

        if (imagePaths.Length == 0)
        {
            Debug.LogWarning($"在 {directionFolderPath} 中没有找到PNG图片");
            return;
        }

        // 创建SpriteAtlas
        SpriteAtlas atlas = new SpriteAtlas();

        // 设置打包设置
        SpriteAtlasPackingSettings packingSettings = new SpriteAtlasPackingSettings
        {
            enableRotation = false,
            enableTightPacking = true,
            padding = 2
        };
        atlas.SetPackingSettings(packingSettings);

        // 设置纹理设置
        SpriteAtlasTextureSettings textureSettings = new SpriteAtlasTextureSettings
        {
            readable = false,
            generateMipMaps = false,
            sRGB = true,
            filterMode = FilterMode.Bilinear
        };
        atlas.SetTextureSettings(textureSettings);

        // 添加所有精灵到图集
        List<Object> objectsToPack = new List<Object>();
        foreach (string imagePath in imagePaths)
        {
            Object obj = AssetDatabase.LoadAssetAtPath<Object>(imagePath);
            if (obj != null)
            {
                objectsToPack.Add(obj);
            }
        }

        atlas.Add(objectsToPack.ToArray());

        // 保存图集
        string atlasName = $"{petNumber}_{directionName}_atla.spriteatlas";
        string atlasPath = Path.Combine(atlasFolder, atlasName);
        AssetDatabase.CreateAsset(atlas, atlasPath);

        Debug.Log($"已创建图集: {atlasName} ({objectsToPack.Count} 张图片)");
    }

    private static void ClearOldAtlases()
    {
        if (!Directory.Exists(atlasFolder))
            return;

        string[] oldAtlases = Directory.GetFiles(atlasFolder, "*.spriteatlas");
        foreach (string atlas in oldAtlases)
        {
            AssetDatabase.DeleteAsset(atlas);
        }

        Debug.Log($"已清除 {oldAtlases.Length} 个旧图集");
    }

    [MenuItem("Tools/Pets/打开Pets文件夹")]
    public static void OpenPetsFolder()
    {
        if (Directory.Exists(petsFolder))
        {
            EditorUtility.RevealInFinder(petsFolder);
        }
        else
        {
            Debug.LogWarning($"文件夹不存在: {petsFolder}");
        }
    }

    [MenuItem("Tools/Pets/打开Atlas文件夹")]
    public static void OpenAtlasFolder()
    {
        if (!Directory.Exists(atlasFolder))
        {
            Directory.CreateDirectory(atlasFolder);
        }
        EditorUtility.RevealInFinder(atlasFolder);
    }
}