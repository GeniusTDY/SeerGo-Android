using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class AddressableLabelValidator
{
    /// <summary>
    /// 检查标签是否存在（同步版本）
    /// </summary>
    public static bool CheckLabelExists(string label)
    {
        var locationsHandle = Addressables.LoadResourceLocationsAsync(label);
        var locations = locationsHandle.WaitForCompletion();

        bool exists = locations.Count > 0;
        Addressables.Release(locationsHandle);

        return exists;
    }

    /// <summary>
    /// 异步检查标签是否存在
    /// </summary>
    public static async Task<bool> CheckLabelExistsAsync(string label)
    {
        var locationsHandle = Addressables.LoadResourceLocationsAsync(label);
        await locationsHandle.Task;

        bool exists = locationsHandle.Status == AsyncOperationStatus.Succeeded &&
                       locationsHandle.Result.Count > 0;

        Addressables.Release(locationsHandle);
        return exists;
    }

    /// <summary>
    /// 获取标签对应的所有资源位置
    /// </summary>
    public static async Task<IList<UnityEngine.ResourceManagement.ResourceLocations.IResourceLocation>> GetLabelLocationsAsync(string label)
    {
        var locationsHandle = Addressables.LoadResourceLocationsAsync(label);
        await locationsHandle.Task;

        var locations = locationsHandle.Result;
        Addressables.Release(locationsHandle);

        return locations;
    }
}