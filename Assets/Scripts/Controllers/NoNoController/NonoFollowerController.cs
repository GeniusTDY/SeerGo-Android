using SeerGoGame.Utilities;
using UnityEngine;

public class NonoFollowerController : MonoBehaviour
{
    [Header("目标设置")]
    public Transform playerTarget; // 要跟随的玩家角色

    [Header("跟随参数")]
    public float followSmoothTime = 0.2f; // 位置跟随平滑时间
    public Vector3 baseOffset = new Vector3(3f, 1f, -1f); // 相对于玩家的基础位置偏移
    public float rotationSmoothTime = 0.1f; // 旋转平滑时间
    public float distanceBeforeTeleport = 5f; // 超过此距离则瞬移，防止掉队

    [Header("动画参数")]
    public float moveThreshold = 0.05f; // 判定为开始移动的速度阈值

    [Header("残影效果")]
    public bool enableAfterImageEffect = true;
    public AfterImageEffect afterImageEffect;

    // 组件与变量
    private Animator animator;
    private Vector3 currentVelocity = Vector3.zero;
    public Vector3 lastPosition; // 上一帧位置，用于计算实际速度

    // 动画参数哈希
    private int speedHash;
    private int directionXHash;
    private int directionYHash;

    void Start()
    {
        // 获取组件
        animator = GetComponent<Animator>();

        // 初始化动画参数哈希
        speedHash = Animator.StringToHash("Speed");
        directionXHash = Animator.StringToHash("DirectionX");
        directionYHash = Animator.StringToHash("DirectionY");

        lastPosition = transform.position;

        // 初始位置设定：立即出现在玩家身边
        if (playerTarget != null)
        {
            SnapToPlayerSide();
        }
        else
        {
            SeerGoLogger.LogWarning("StableNonoFollower: 请指定要跟随的玩家目标。");
        }

        // 初始化残影效果
        InitializeAfterImageEffect();
    }

    void LateUpdate()
    {
        if (playerTarget == null) return;

        // 1. 平滑更新位置
        UpdatePosition();

        // 2. 更新动画参数
        UpdateAnimationParameters();

        // 3. 更新残影效果强度（基于移动速度）
        UpdateAfterImageIntensity();
    }

    /// <summary>
    /// 初始化残影效果
    /// </summary>
    void InitializeAfterImageEffect()
    {
        if (enableAfterImageEffect)
        {
            if (afterImageEffect == null)
                afterImageEffect = GetComponent<AfterImageEffect>();

            if (afterImageEffect == null)
                afterImageEffect = gameObject.AddComponent<AfterImageEffect>();

            afterImageEffect.enableAfterImages = true;
        }
        else
        {
            if (afterImageEffect != null)
                afterImageEffect.enableAfterImages = false;
        }
    }

    /// <summary>
    /// 立即将NONO设定在玩家身边的指定位置
    /// </summary>
    void SnapToPlayerSide()
    {
        Vector3 targetPos = playerTarget.TransformPoint(baseOffset);
        transform.position = targetPos;
        lastPosition = targetPos;

        // 瞬移时清除所有残影
        if (afterImageEffect != null)
            afterImageEffect.CleanDestroyedObjectsInPool();
    }

    /// <summary>
    /// 使用SmoothDamp平滑更新NONO根节点的位置
    /// </summary>
    void UpdatePosition()
    {
        // 计算目标位置：玩家位置 + 经过玩家旋转调整的偏移量
        Vector3 targetPosition = playerTarget.TransformPoint(baseOffset);

        // 如果距离玩家过远，则瞬移，防止因平滑跟随导致掉队
        if (Vector3.Distance(transform.position, playerTarget.position) > distanceBeforeTeleport)
        {
            transform.position = targetPosition;
            currentVelocity = Vector3.zero; // 瞬移后重置速度

            // 瞬移时清除残影
            if (afterImageEffect != null)
                afterImageEffect.CleanDestroyedObjectsInPool();
        }
        else
        {
            // 应用平滑移动
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition,
                                                   ref currentVelocity, followSmoothTime);
        }
    }

    /// <summary>
    /// 更新动画状态机的参数
    /// </summary>
    void UpdateAnimationParameters()
    {
        if (animator == null) return;

        // 计算当前帧的实际移动速度
        float currentSpeed = (transform.position - lastPosition).magnitude / Time.deltaTime;
        lastPosition = transform.position;

        // 设置Speed参数：如果速度超过阈值，则为1（移动状态），否则为0（闲置状态）
        float targetSpeedValue = currentSpeed > moveThreshold ? 1f : 0f;
        animator.SetFloat(speedHash, targetSpeedValue);

        // 计算并设置方向参数（基于NONO自身坐标系指向玩家的向量）
        Vector3 toPlayer = playerTarget.position - transform.position;
        toPlayer.y = 0;
        if (toPlayer.magnitude > 0.1f)
        {
            Vector3 toPlayerLocal = transform.InverseTransformDirection(toPlayer.normalized);
            animator.SetFloat(directionXHash, toPlayerLocal.x);
            animator.SetFloat(directionYHash, toPlayerLocal.z);
        }
    }

    /// <summary>
    /// 根据移动速度更新残影效果强度
    /// </summary>
    void UpdateAfterImageIntensity()
    {
        if (afterImageEffect == null) return;

        float currentSpeed = (transform.position - lastPosition).magnitude / Time.deltaTime;

        // 根据速度调整残影强度
        float intensity = Mathf.Clamp01((currentSpeed - moveThreshold) / 2f); // 2f是最大速度参考值
        afterImageEffect.SetAfterImageIntensity(intensity);
    }

    /// <summary>
    /// 外部调用：开启/关闭残影效果
    /// </summary>
    public void SetAfterImageEnabled(bool enabled)
    {
        if (afterImageEffect != null)
            afterImageEffect.enableAfterImages = enabled;
    }

    /// <summary>
    /// 外部调用：设置残影效果强度
    /// </summary>
    public void BoostAfterImageEffect(float intensity, float duration)
    {
        if (afterImageEffect != null)
        {
            StartCoroutine(BoostAfterImageCoroutine(intensity, duration));
        }
    }

    private System.Collections.IEnumerator BoostAfterImageCoroutine(float intensity, float duration)
    {
        float originalSpawnInterval = afterImageEffect.spawnInterval;
        Color originalColor = afterImageEffect.afterImageColor;

        // 增强效果
        afterImageEffect.SetAfterImageIntensity(intensity);

        yield return new WaitForSeconds(duration);

        // 恢复原效果
        afterImageEffect.spawnInterval = originalSpawnInterval;
        afterImageEffect.afterImageColor = originalColor;
    }
}