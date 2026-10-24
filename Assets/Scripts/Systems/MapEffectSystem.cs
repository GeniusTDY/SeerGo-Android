using QFramework;
using SeerGoGame.Interfaces;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/**
 * 地图动画系统（AssetBundle 版本）
 * 按 mapId 加载 MapAnim/ 目录下 Map_{mapId}_* 的全部 AB 包并实例化
 */
namespace SeerGoGame.Systems
{

    public class MapEffectSystem : AbstractSystem, IMapEffectSystem
    {
        /// <summary>已加载的效果实例 (effectLabel → 实例列表)</summary>
        private Dictionary<string, List<GameObject>> mActiveEffects = new();

        protected override void OnInit()
        {
            NpcAssetBundleLoader.Initialize(); // 确保路径已初始化
        }

        /// <summary>
        /// 加载指定 mapId 的所有地图动画特效
        /// </summary>
        public async Task LoadEffectsAsync(int mapId, Transform parent)
        {
            string effectLabel = $"Map_{mapId}_Effects";

            // 复用已加载的
            if (mActiveEffects.ContainsKey(effectLabel))
            {
                SeerGoLogger.Log($"[MapEffect] 特效已加载: {effectLabel}");
                return;
            }

            try
            {
                // 通过 Loader 加载该 mapId 下所有 Map_* Bundle 的预制体
                var prefabs = await NpcAssetBundleLoader.LoadMapEffectsAsync(mapId);

                if (prefabs == null || prefabs.Count == 0)
                {
                    SeerGoLogger.LogWarning($"[MapEffect] mapId={mapId} 无动画特效");
                    return;
                }

                mActiveEffects[effectLabel] = new List<GameObject>();

                foreach (var prefab in prefabs)
                {
                    if (prefab != null)
                    {
                        var instance = GameObject.Instantiate(prefab, parent);
                        mActiveEffects[effectLabel].Add(instance);
                    }
                }

                SeerGoLogger.Log($"[MapEffect] 加载完成: {effectLabel}, 实例数={mActiveEffects[effectLabel].Count}");
            }
            catch (System.Exception e)
            {
                SeerGoLogger.LogError($"[MapEffect] 加载失败 mapId={mapId}: {e.Message}");
            }
        }

        /// <summary>
        /// 卸载指定 mapId 的所有地图动画特效（销毁实例 + 卸载 AB）
        /// </summary>
        public async Task UnloadEffectsAsync(int mapId)
        {
            string effectLabel = $"Map_{mapId}_Effects";

            // 销毁场景实例
            if (mActiveEffects.TryGetValue(effectLabel, out var instances))
            {
                foreach (var instance in instances)
                {
                    if (instance != null) GameObject.Destroy(instance);
                }
                mActiveEffects.Remove(effectLabel);
            }

            // 卸载对应 Bundle
            NpcAssetBundleLoader.UnloadMapEffects(mapId, false);

            await Task.Yield();
        }

        public async void Deinit()
        {
            var model = this.GetModel<IMapEffectModel>();
            await UnloadEffectsAsync(model.CurrentMapId);
        }
    }
}
