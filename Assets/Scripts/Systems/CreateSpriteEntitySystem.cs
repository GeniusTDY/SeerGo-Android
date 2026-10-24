using DG.Tweening;
using FTRuntime;
using PixeLadder.SimpleTooltip;
using QFramework;
using SeerGoGame.Events;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace SeerGoGame.Systems
{
    public class CreateSpriteEntitySystem : AbstractSystem, ICreateSpriteEntitySystem
    {
        private IMapParser _mapParser;

        private Dictionary<string, List<GameObject>> _activeInstances = new Dictionary<string, List<GameObject>>();

        // 对象池管理频繁创建销毁的对象
        private Dictionary<string, Queue<GameObject>> _objectPools = new Dictionary<string, Queue<GameObject>>();

        // 缓存常用组件和资源
        private Canvas _uiCanvasCache;
        private readonly Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();


        protected override void OnInit()
        {
            _mapParser = this.GetUtility<IMapParser>();
            // 预缓存UI组件
            this.RegisterEvent<LoginSuccessEvent>(CacheUICanvas);
        }

        /// <summary>
        /// 预缓存UICanvas，避免在创建每个精灵时都使用GameObject.Find
        /// </summary>
        private void CacheUICanvas(LoginSuccessEvent e)
        {
            var canvasObj = GameObject.Find("UICanvas");
            _uiCanvasCache = canvasObj != null ? canvasObj.GetComponent<Canvas>() : null;
            if (_uiCanvasCache == null)
            {
                SeerGoLogger.LogWarning("在场景中未找到名为'UICanvas'的游戏对象或其上无Canvas组件。");
            }
        }

        // == 核心创建方法 ==

        /// <summary>
        /// 统一的精灵创建入口，根据NPC类型自动选择创建策略
        /// </summary>
        public async void CreateSpriteAsync(MapNpc item, Transform parent, List<GameObject> resultList)
        {

            if (!ValidateCreationParameters(item, parent, resultList))
            {
                SeerGoLogger.LogWarning("[CreateSpriteAsync] 参数校验失败，退出");
                return;
            }

            // 根据Rate概率决定是否创建精灵，Rate默认100表示100%出现
            if (item.Rate < 100)
            {
                int randomValue = Random.Range(0, 100);
                if (randomValue >= item.Rate)
                {
                    SeerGoLogger.Log($"[CreateSpriteAsync] 概率未命中: {item.Name}(ResId: {item.ResId}), Rate={item.Rate}, Random={randomValue}, 跳过创建");
                    return;
                }
            }

            GameObject instance = null;

            try
            {
                switch (item.Type)
                {
                    case "prefab":
                        instance = await CreateFromPrefabAsync(item, parent);
                        if (instance != null)
                        {
                            SpriteRenderer spriteRenderer = instance.GetComponent<SpriteRenderer>();
                            if (spriteRenderer != null && item.ResId.Contains("Pet"))
                            {
                                Color color = spriteRenderer.color;
                                color.a = 0f;
                                spriteRenderer.color = color;
                                spriteRenderer.DOFade(1f, 0.3f).SetEase(Ease.InOutSine);
                            }
                        }
                        break;
                    case "sprite":
                        instance = CreateFromSprite(item, parent);
                        break;
                    default:
                        SeerGoLogger.LogError($"不支持的NPC类型: {item.Type} (ResId: {item.ResId})");
                        break;
                }
            }
            catch (System.Exception ex)
            {
                SeerGoLogger.LogError($"[CreateSpriteAsync] 异常: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                return;
            }

            if (instance != null)
            {
                resultList.Add(instance);
                RegisterActiveInstance(item.ResId, instance); // 登记活跃实例
                SeerGoLogger.Log($"[CreateSpriteAsync] 创建成功: {instance.name}, resultList计数={resultList.Count}");
            }
            else
            {
                SeerGoLogger.LogWarning("[CreateSpriteAsync] instance为null，创建失败");
            }
        }

        /// <summary>
        /// 参数验证
        /// </summary>
        private bool ValidateCreationParameters(MapNpc item, Transform parent, List<GameObject> resultList)
        {
            if (item == null)
            {
                SeerGoLogger.LogError("创建精灵失败: MapNpc 参数为null。");
                return false;
            }
            if (parent == null)
            {
                SeerGoLogger.LogWarning($"创建精灵失败: 父节点为空 (ResId: {item.ResId})。将创建到场景根节点。");
                return false;
            }
            if (resultList == null)
            {
                SeerGoLogger.LogError("结果列表为null，无法添加精灵实例。");
                return false;
            }
            if (string.IsNullOrEmpty(item.ResId))
            {
                SeerGoLogger.LogError("资源ID（ResId）为空，无法加载资源。");
                return false;
            }
            return true;
        }

        // == 两种创建方式的实现 ==

        /// <summary>
        /// 通过 AssetBundle 异步加载预制体并实例化
        /// </summary>
        private async Task<GameObject> CreateFromPrefabAsync(MapNpc item, Transform parent)
        {
            // 检查 Bundle 文件是否存在
            if (!NpcAssetBundleLoader.ResIdExists(item.ResId))
            {
                SeerGoLogger.LogError($"资源不存在: {item.ResId}\n  期望路径: {NpcAssetBundleLoader.GetBundlePath(item.ResId)}\n  请执行菜单 Assets/Build NpcAssetBundles/构建全部 重新构建AB包");
                return null;
            }
            try
            {
                // 检查对象池中是否有可复用对象
                if (TryGetFromPool(item.ResId, out GameObject pooledInstance))
                {
                    pooledInstance.transform.SetParent(parent);
                    pooledInstance.SetActive(true);
                    ConfigureSpriteInstance(item, pooledInstance);
                    return pooledInstance;
                }

                // 通过 AssetBundle 异步加载预制体
                GameObject loadedPrefab = await NpcAssetBundleLoader.LoadPrefabAsync(item.ResId);

                if (loadedPrefab != null)
                {
                    GameObject instance = GameObject.Instantiate(loadedPrefab, parent);
                    ConfigureSpriteInstance(item, instance);
                    return instance;
                }
                else
                {
                    SeerGoLogger.LogError($"加载预制体失败: {item.ResId}");
                }
            }
            catch (System.Exception e)
            {
                SeerGoLogger.LogError($"通过AssetBundle加载预制体失败 {item.ResId}: {e.Message}");
            }
            return null;
        }

        /// <summary>
        /// 动态创建Sprite游戏对象
        /// </summary>
        private GameObject CreateFromSprite(MapNpc item, Transform parent)
        {
            // 使用缓存避免重复加载Sprite
            if (!_spriteCache.TryGetValue(item.ResId, out Sprite sprite))
            {
                sprite = _mapParser.LoadSpriteByResId(item.ResId);
                if (sprite != null)
                {
                    _spriteCache[item.ResId] = sprite;
                }
                else
                {
                    SeerGoLogger.LogWarning($"[CreateFromSprite] 无法加载Sprite资源: {item.ResId}，将创建空对象。");
                }
            }

            GameObject instance = new GameObject($"Sprite_{item.Id}");
            instance.transform.SetParent(parent);

            ConfigureSpriteInstance(item, instance, sprite);

            return instance;
        }

        // == 对象池管理 == 

        /// <summary>
        /// 尝试从对象池中获取一个可用的实例
        /// </summary>
        private bool TryGetFromPool(string resId, out GameObject instance)
        {
            instance = null;
            if (_objectPools.TryGetValue(resId, out Queue<GameObject> pool) && pool.Count > 0)
            {
                instance = pool.Dequeue();
                return instance != null;
            }
            return false;
        }

        /// <summary>
        /// 将实例返回到对象池，而不是直接Destroy
        /// </summary>
        public void ReturnToPool(string resId, GameObject instance)
        {
            if (instance == null) return;

            instance.SetActive(false);

            if (!_objectPools.ContainsKey(resId))
            {
                _objectPools[resId] = new Queue<GameObject>();
            }
            _objectPools[resId].Enqueue(instance);

            // 清理过大的对象池，防止内存占用过高
            if (_objectPools[resId].Count > 20) // 设置一个合理的上限
            {
                var excess = _objectPools[resId].Count - 10;
                for (int i = 0; i < excess; i++)
                {
                    var objToDestroy = _objectPools[resId].Dequeue();
                    if (objToDestroy != null)
                    {
                        GameObject.Destroy(objToDestroy);
                    }
                }
            }
        }

        // == 精灵实例的通用配置 ==

        /// <summary>
        /// 集中配置精灵实例的通用组件和属性
        /// </summary>
        private void ConfigureSpriteInstance(MapNpc item, GameObject instance, Sprite sprite = null)
        {
            instance.tag = item.eventHandler?.button?.action ?? "unopened";
            // 1. 设置基本变换
            instance.transform.position = ParseUtils.ParseVector2(item.Position);
            instance.transform.eulerAngles = ParseUtils.ParseVector3(item.Rotation);

            Vector2 size = ParseUtils.ParseVector2(item.Size);
            if (size != Vector2.zero)
            {
                instance.transform.localScale = new Vector3(size.x, size.y, 1);
            }

            // 2. 设置渲染器：PetWalkController预制体用SwfClip.sortingOrder，否则用SpriteRenderer
            PetWalkController petWalk = instance.GetComponentInChildren<PetWalkController>();
            if (petWalk != null)
            {
                // SwfClip 内部管理自己的 MeshRenderer/SortingGroup，直接设其 sortingOrder
                SwfClip swfClip = instance.GetComponentInChildren<SwfClip>();
                if (swfClip != null)
                {
                    swfClip.sortingOrder = item.Sort;
                    SeerGoLogger.Log($"[Configure] PetWalk SwfClip.sortingOrder = {item.Sort} (resId={item.ResId})");
                }
            }
            else
            {
                SpriteRenderer spriteRenderer = instance.GetComponent<SpriteRenderer>();
                if (spriteRenderer == null)
                {
                    spriteRenderer = instance.AddComponent<SpriteRenderer>();
                }

                if (sprite != null)
                {
                    spriteRenderer.sprite = sprite;
                }
                spriteRenderer.color = ParseUtils.ParseColor(item.Color);
                spriteRenderer.sortingOrder = item.Sort;
            }

            // 3. 添加碰撞器
            if (item.PolygonEnable)
            {
                BoxCollider2D collider = instance.GetComponent<BoxCollider2D>();
                bool existedBefore = (collider != null);
                if (collider == null) collider = instance.AddComponent<BoxCollider2D>();

                if (!existedBefore)
                {
                    // 立即尝试用 Renderer 边界设尺寸（SpriteRenderer 此时已有有效边界）
                    SizeByRenderer(collider, instance);
                }

                BoxCollider2D triggerCollider = instance.AddComponent<BoxCollider2D>();
                triggerCollider.isTrigger = true;
                triggerCollider.size = collider.size;
                triggerCollider.offset = collider.offset;

                // SwfClip 网格可能尚未生成，挂修正组件在下一帧刷新
                if (!existedBefore && instance.GetComponent<ColliderFixer>() == null)
                    instance.AddComponent<ColliderFixer>();
            }

            // 4. 添加交互控制器
            NpcCilckController controller = instance.GetComponent<NpcCilckController>();
            if (controller == null)
            {
                controller = instance.AddComponent<NpcCilckController>();
            }
            controller.Initialize(item);

            // 5. 添加提示工具
            if (_uiCanvasCache != null)
            {
                TooltipTrigger.AddTooltip(instance, null, item.description ?? item.Name ?? "无", Color.yellow, null, _uiCanvasCache);
            }
        }

        /// <summary>
        /// 缩放PolygonCollider2D的顶点
        /// </summary>
        /// <param name="scaleFactor">缩放系数（例如2.0表示放大2倍）</param>
        public void ScaleColliderPoints(PolygonCollider2D triggerCollider, float scaleFactor)
        {
            // 检查组件是否存在
            if (triggerCollider == null) return;

            // 获取所有路径（通常不规则形状只有一条路径，路径0）
            int pathCount = triggerCollider.pathCount;
            for (int pathIndex = 0; pathIndex < pathCount; pathIndex++)
            {
                // 获取指定路径的顶点数组
                Vector2[] points = triggerCollider.GetPath(pathIndex);
                // 遍历每个顶点并进行缩放
                for (int i = 0; i < points.Length; i++)
                {
                    points[i] *= scaleFactor;
                }
                // 将缩放后的顶点数组重新赋给碰撞体路径
                triggerCollider.SetPath(pathIndex, points);
            }
        }

        /// <summary>用第一个 Renderer 世界边界设置 BoxCollider2D 本地尺寸</summary>
        private void SizeByRenderer(BoxCollider2D col, GameObject instance)
        {
            var r = instance.GetComponentInChildren<Renderer>();
            if (r == null || r.bounds.size.magnitude < 0.001f) return;
            Vector3 ls = instance.transform.lossyScale;
            col.size = new Vector2(
                r.bounds.size.x / Mathf.Max(Mathf.Abs(ls.x), 0.001f),
                r.bounds.size.y / Mathf.Max(Mathf.Abs(ls.y), 0.001f)
            );
            col.offset = new Vector2(
                (r.bounds.center.x - instance.transform.position.x) / ls.x,
                (r.bounds.center.y - instance.transform.position.y) / ls.y
            );
        }








        // == 资源验证与卸载 ==

        /// <summary>
        /// 优化的单个资源卸载方法
        /// </summary>
        public async Task UnloadSpriteAsync(string resId)
        {
            try
            {
                // 1. 将活跃实例移入对象池而非直接销毁
                if (_activeInstances.TryGetValue(resId, out var instances))
                {
                    foreach (var instance in instances)
                    {
                        if (instance != null)
                        {
                            ReturnToPool(resId, instance);
                        }
                    }
                    instances.Clear();
                    _activeInstances.Remove(resId);
                }

                // 2. 释放 AssetBundle 资源
                NpcAssetBundleLoader.UnloadByResId(resId, false);

                // 3. 清理缓存
                _spriteCache.Remove(resId);

                // SeerGoLogger.Log($"已卸载资源: {resId} (实例已放入对象池)");
            }
            catch (System.Exception ex)
            {
                SeerGoLogger.LogError($"卸载资源时发生异常 (ResId: {resId}): {ex.Message}");
            }
            await Task.Yield();
        }

        /// <summary>
        /// 批量卸载所有资源
        /// </summary>
        public async Task UnloadAllSpritesAsync()
        {
            // SeerGoLogger.Log("开始批量卸载所有资源...");

            // 收集所有资源ID
            var resIds = new List<string>(_activeInstances.Keys);
            foreach (var resId in resIds)
            {
                await UnloadSpriteAsync(resId);
            }

            // 清理所有 AssetBundle
            NpcAssetBundleLoader.UnloadAll(false);

            // 清空对象池
            foreach (var pool in _objectPools.Values)
            {
                while (pool.Count > 0)
                {
                    var instance = pool.Dequeue();
                    if (instance != null)
                    {
                        Object.Destroy(instance); // 彻底销毁池中的对象
                    }
                }
            }
            _objectPools.Clear();
        }

        /// <summary>
        /// 登记活跃实例，用于后续管理
        /// </summary>
        private void RegisterActiveInstance(string resId, GameObject instance)
        {
            if (!_activeInstances.ContainsKey(resId))
            {
                _activeInstances[resId] = new List<GameObject>();
            }
            _activeInstances[resId].Add(instance);
        }

        // 传送点创建方法
        public void CreateTeleporterSprite(MapTeleport item, Transform parentTransform, List<GameObject> spritesList)
        {
            //使用缓存和对象池
            GameObject teleporterInstance = CreateFromCachedPrefab("Prefabs/Teleporter", parentTransform);
            if (teleporterInstance == null) return;

            teleporterInstance.transform.position = ParseUtils.ParseVector2(item.Position);
            teleporterInstance.GetComponent<NpcCilckController>().Initialize(item);

            if (_uiCanvasCache != null)
            {
                TooltipTrigger.AddTooltip(teleporterInstance, null, item.Name ?? "传送点", Color.yellow, null, _uiCanvasCache);
            }

            spritesList.Add(teleporterInstance);
            RegisterActiveInstance("Teleporter", teleporterInstance); // 使用统一标签登记
        }

        /// <summary>
        /// 从缓存或Resources加载预制体的辅助方法
        /// </summary>
        private GameObject CreateFromCachedPrefab(string path, Transform parent)
        {
            // 这里可以实现对Resources加载资源的缓存
            GameObject prefab = Resources.Load<GameObject>(path);
            if (prefab == null)
            {
                SeerGoLogger.LogError($"Resources加载失败！路径: {path}");
                return null;
            }
            return GameObject.Instantiate(prefab, parent);
        }

        public async void Deinit()
        {
            this.UnRegisterEvent<LoginSuccessEvent>(CacheUICanvas);
            await UnloadAllSpritesAsync();
        }

    }

    /// <summary>修正碰撞体尺寸：Start() 刷新后再更新触发体，然后自毁</summary>
    internal class ColliderFixer : MonoBehaviour
    {
        IEnumerator Start()
        {
            yield return null; // 等一帧，确保 SwfClip 网格已生成
            var cols = GetComponents<BoxCollider2D>();
            if (cols.Length < 2) { Destroy(this); yield break; }

            var r = GetComponentInChildren<Renderer>();
            if (r != null && r.bounds.size.magnitude > 0.001f)
            {
                Vector3 ls = transform.lossyScale;
                Vector2 size = new Vector2(
                    r.bounds.size.x / Mathf.Max(Mathf.Abs(ls.x), 0.001f),
                    r.bounds.size.y / Mathf.Max(Mathf.Abs(ls.y), 0.001f)
                );
                Vector2 offset = new Vector2(
                    (r.bounds.center.x - transform.position.x) / ls.x,
                    (r.bounds.center.y - transform.position.y) / ls.y
                );
                foreach (var col in cols) { col.size = size; col.offset = offset; }
            }
            Destroy(this);
        }
    }
}



