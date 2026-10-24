using SeerGoGame.Utilities;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NONO残影效果管理器
/// </summary>
public class AfterImageEffect : MonoBehaviour
{
    [Header("残影基础设置")]
    [Tooltip("是否启用残影效果")]
    public bool enableAfterImages = true;

    [Tooltip("残影生成间隔(秒)")]
    public float spawnInterval = 0.1f;

    [Tooltip("每个残影的存活时间(秒)")]
    public float lifeTime = 0.5f;

    [Tooltip("残影淡出时间占生命周期的比例")]
    [Range(0.1f, 0.9f)]
    public float fadeStartRatio = 0.6f;

    [Header("残影视觉设置")]
    [Tooltip("残影颜色（通常比本体浅或使用特殊色调）")]
    public Color afterImageColor = new Color(0.7f, 0.8f, 1f, 0.6f);

    [Tooltip("残影的渲染排序层级")]
    public string sortingLayerName = "Default";

    [Tooltip("残影的渲染排序顺序")]
    public int sortingOrder = 6;

    [Header("触发条件")]
    [Tooltip("移动速度达到多少时开始生成残影")]
    public float minSpeedToSpawn = 0.1f;

    [Tooltip("是否仅在移动时生成残影")]
    public bool spawnOnlyWhenMoving = true;

    [Header("数量控制")]
    [Tooltip("同时存在的最多残影数量")]
    public int maxConcurrentAfterImages = 5;


    // 私有变量
    private float spawnTimer;
    private SpriteRenderer[] mainSpriteRenderers; // NONO本体的所有SpriteRenderer
    private List<AfterImage> activeAfterImages = new List<AfterImage>();
    private NonoFollowerController followerController;
    private Transform nonoRoot; // NONO的根节点

    // 残影池（优化性能）
    private Queue<AfterImage> afterImagePool = new Queue<AfterImage>();
    private const int POOL_INITIAL_SIZE = 5;

    void Start()
    {
        followerController = GetComponent<NonoFollowerController>();
        nonoRoot = transform; // NONO的根节点

        // 获取NONO本体所有的SpriteRenderer（包括子对象）
        mainSpriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        InitializeAfterImagePool();
    }

    void Update()
    {
        if (!enableAfterImages || mainSpriteRenderers == null || mainSpriteRenderers.Length == 0)
            return;

        // 检查是否满足生成条件
        bool shouldSpawn = CheckSpawnConditions();

        if (shouldSpawn)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                spawnTimer = 0f;
                SpawnAfterImage();
            }
        }
        else
        {
            spawnTimer = 0f; // 重置计时器
        }

        // 更新所有活跃的残影
        UpdateAllAfterImages();
    }

    /// <summary>
    /// 检查是否满足生成残影的条件
    /// </summary>
    private bool CheckSpawnConditions()
    {
        if (!spawnOnlyWhenMoving)
            return true;

        // 跟随控制器，使用速度检测
        if (followerController != null)
        {
            // 根据NonoFollowerController实际实现来获取速度
            float currentSpeed = (transform.position - followerController.lastPosition).magnitude / Time.deltaTime;
            return currentSpeed > minSpeedToSpawn;
        }

        return true;
    }

    /// <summary>
    /// 生成一个残影
    /// </summary>
    private void SpawnAfterImage()
    {
        if (activeAfterImages.Count >= maxConcurrentAfterImages)
        {
            return;
        }

        AfterImage afterImage = GetAfterImageFromPool();
        if (afterImage != null)
        {
            afterImage.Initialize(mainSpriteRenderers, afterImageColor, sortingLayerName, sortingOrder, lifeTime, fadeStartRatio);
            activeAfterImages.Add(afterImage);
        }
    }

    /// <summary>
    /// 更新所有活跃的残影
    /// </summary>
    private void UpdateAllAfterImages()
    {
        for (int i = activeAfterImages.Count - 1; i >= 0; i--)
        {
            AfterImage image = activeAfterImages[i];
            image.UpdateAfterImage(Time.deltaTime);

            if (image.IsExpired)
            {
                activeAfterImages.RemoveAt(i);
                ReturnAfterImageToPool(image);
            }
        }
    }

    /// <summary>
    /// 初始化残影对象池
    /// </summary>
    private void InitializeAfterImagePool()
    {
        for (int i = 0; i < POOL_INITIAL_SIZE; i++)
        {
            CreateNewAfterImageForPool();
        }
    }

    /// <summary>
    /// 创建新的残影对象并加入对象池
    /// </summary>
    private void CreateNewAfterImageForPool()
    {
        GameObject poolObj = new GameObject("AfterImage_Pooled");
        poolObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        poolObj.SetActive(false);

        AfterImage newAfterImage = new AfterImage(poolObj);
        afterImagePool.Enqueue(newAfterImage);
    }

    /// <summary>
    /// 从对象池获取残影（增强安全性）
    /// </summary>
    private AfterImage GetAfterImageFromPool()
    {
        // 清理对象池中已被销毁的对象
        CleanDestroyedObjectsInPool();

        if (afterImagePool.Count == 0)
        {
            CreateNewAfterImageForPool();
        }

        AfterImage afterImage = afterImagePool.Dequeue();
        // 从池中取出的对象可能已被销毁
        if (afterImage != null && afterImage.GameObject != null) // 显式检查GameObject
        {
            afterImage.GameObject.SetActive(true);
            return afterImage;
        }
        else
        {
            // 如果对象无效，递归调用自身以获取下一个，或创建新的
            SeerGoLogger.LogWarning("从对象池中取到了一个已销毁的对象，将尝试获取下一个。");
            return GetAfterImageFromPool(); 
        }
    }

    /// <summary>
    /// 将残影返回对象池
    /// </summary>
    public void ReturnAfterImageToPool(AfterImage afterImage)
    {
        // 确保afterImage及其GameObject未被销毁
        if (afterImage == null || afterImage.GameObject == null)
        {
            SeerGoLogger.LogWarning("尝试将一个空或已销毁的AfterImage返回对象池，操作将被忽略。");
            return; // 直接忽略，不加入池中
        }

        afterImage.GameObject.SetActive(false);
        afterImagePool.Enqueue(afterImage);
    }

    /// <summary>
    /// 清理对象池中所有已被Unity销毁的GameObject的引用
    /// </summary>
    public void CleanDestroyedObjectsInPool()
    {
        // 创建一个临时队列来存放仍然有效的对象
        Queue<AfterImage> validPool = new Queue<AfterImage>();
        foreach (var image in afterImagePool)
        {
            // 检查AfterImage实例及其包裹的GameObject是否有效
            if (image != null && image.GameObject != null)
            {
                validPool.Enqueue(image);
            }
        }
        // 用清理后的队列替换原对象池
        afterImagePool = validPool;
    }

    /// <summary>
    /// 设置残影效果强度
    /// </summary>
    public void SetAfterImageIntensity(float intensity)
    {
        spawnInterval = Mathf.Lerp(0.05f, 0.3f, 1f - intensity);
        afterImageColor.a = Mathf.Lerp(0.3f, 0.8f, intensity);
    }

    void OnDisable()
    {
        CleanDestroyedObjectsInPool();
    }

    void OnDestroy()
    {
        // 清理对象池
        while (afterImagePool.Count > 0)
        {
            AfterImage image = afterImagePool.Dequeue();

            if (image.GameObject != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(image.GameObject);
                }
                else
                {
                    DestroyImmediate(image.GameObject);
                }
            }
        }
    }
}