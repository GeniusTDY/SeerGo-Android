using PixeLadder.SimpleTooltip;
using SeerGoGame.Helper;
using SeerGoGame.Utilities;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SeerGoGame.Managers
{
    public class GameSceneManager : BaseBattleController
    {
        private static GameSceneManager _instance;
        public static GameSceneManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameSceneManager");
                    _instance = go.AddComponent<GameSceneManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private void OnEnable()
        {
            // 监听场景卸载事件
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnDisable()
        {
            // 取消监听
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        /// <summary>
        /// 场景卸载后的回调
        /// </summary>
        private void OnSceneUnloaded(Scene scene)
        {
            // 此时旧场景已销毁，新场景未加载
            SeerGoLogger.Log($"场景 '{scene.name}' 已卸载，执行清理");
            // 使用立即强制隐藏替代淡出动画，避免操作已销毁对象
            if (TooltipManager.Instance != null)
            {
                TooltipManager.Instance.ForceHideAllImmediate();
            }
            StartCoroutine(CleanupBetweenScenes());
        }
        private void HideTooltip()
        {
            if (TooltipManager.Instance == null)
            {
                return;
            }

            TooltipManager.Instance.ForceHideAllImmediate();
        }

        /// <summary>
        /// 切换场景
        /// </summary>
        public void ChangeScene(string sceneName)
        {
            // 释放 Resources 系统管理的内部资源（不含外部图片/音频）
            // 外部资源由 ExternalResourcesManager 的 LRU 缓存自动管理，
            // 跨场景复用的资源不会被误销毁
            int resMgrBefore = ResourcesManager.Instance.GetLoadedResourceCount();
            ResourcesManager.Instance.ReleaseAll();
            SeerGoLogger.Log($"[SceneChange] ResourcesManager 释放: {resMgrBefore}→{ResourcesManager.Instance.GetLoadedResourceCount()}, ExtRes 缓存保留: Sprite {ExternalResourcesManager.Instance.SpriteCacheCount}, Audio {ExternalResourcesManager.Instance.AudioCacheCount}");

            // 加载新场景
            SceneManager.LoadScene(sceneName);
        }

        /// <summary>
        /// 场景卸载后清理（仅处理 AssetBundle 和字典等安全的清理操作）
        /// 图片和音频资源已在 ChangeScene() 中提前释放
        /// </summary>
        private IEnumerator CleanupBetweenScenes()
        {
            // 等待一帧，确保旧场景完全销毁
            yield return null;

            // 卸载所有AssetBundle（有自己的安全引用管理）
            StartCoroutine(ExternalAssetBundleManager.Instance.SafeUnloadAllBundles(true));

            // 清理无效引用 + 字典
            ExternalAssetBundleManager.Instance.CleanupDeadReferences();
            GameObjectDictionary.ClearDictionary();
        }
    }
}