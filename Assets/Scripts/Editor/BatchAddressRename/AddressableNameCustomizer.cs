using NUnit.Framework.Internal;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.Timeline;

public static class AddressableNameCustomizer
{
    [MenuItem("Tools/Addressables/批量设置自定义地址（带前缀）")]
    public static void BatchSetCustomAddresses()
    {
        // 获取Addressable资产设置
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
        {
            Debug.LogError("未找到Addressable Asset Settings。请确保已初始化Addressables。");
            return;
        }

        // 1. 定义规则：选择目标文件夹和前缀
        string targetFolderPath = "Assets/PetsResource/Generated/Prefabs"; // 改为您的资源所在文件夹
        string customPrefix = "Prefabs/"; // 改为您想要的前缀

        string targetGroupName = "PetsAnimations"; // 修改为您需要的目标组名

        // 1. 查找目标组
        AddressableAssetGroup targetGroup = null;
        foreach (var group in settings.groups)
        {
            if (group.Name.Equals(targetGroupName))
            {
                targetGroup = group;
                break;
            }
        }

        // 如果未找到目标组，则报错并退出
        if (targetGroup == null)
        {
            Debug.LogError($"未找到名为 '{targetGroupName}' 的Addressable组。请检查组名是否正确。");
            return;
        }

        // 2. 获取目标文件夹下的所有资源GUID
        string[] assetGuids = AssetDatabase.FindAssets("", new[] { targetFolderPath });

        foreach (string assetGuid in assetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
            // 过滤掉.meta文件等，只处理主资源
            if (AssetDatabase.IsValidFolder(assetPath) || Path.GetExtension(assetPath) == ".meta")
                continue;

            // 3. 生成自定义地址：前缀 + 文件名（不含扩展名）
            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            string customAddress = $"{customPrefix}{fileName}";

            // 4. 找到或创建Addressable资源条目
            AddressableAssetEntry entry = settings.FindAssetEntry(assetGuid);
            if (entry == null)
            {
                // 如果资源还未标记为Addressable，先将其添加到默认组
                entry = settings.CreateOrMoveEntry(assetGuid, targetGroup);
            }

            // 5. 应用自定义地址
            if (entry.address != customAddress)
            {
                entry.address = customAddress;
                Debug.Log($"已设置地址: {assetPath} -> {customAddress}");
            }
        }

        // 6. 保存设置
        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, null, true);
        AssetDatabase.SaveAssets();
        Debug.Log("自定义地址批量设置完成！");
    }
}