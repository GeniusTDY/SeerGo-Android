using UnityEngine;

public class PetBlendTreeController : MonoBehaviour
{
    private Animator animator;
    private Vector2 currentDirection = Vector2.down;

    [Header("移动设置")]
    public float moveRadius = 1.5f;         // 精灵移动范围半径
    public float minMoveDistance = 1f;    // 最小移动距离
    public float maxMoveDistance = 1.5f;    // 最大移动距离
    public float minMoveInterval = 1f;    // 最小移动间隔
    public float maxMoveInterval = 3f;    // 最大移动间隔
    public float moveSpeed = 1f;       // 移动速度

    private Vector3 originPosition;      // 初始位置
    private Vector3 targetPosition;      // 目标位置
    private bool isMoving = false;       // 是否正在移动
    private float nextMoveTime;           // 下次移动时间

    private void Start()
    {
        animator = GetComponent<Animator>();
        originPosition = transform.position;
        UpdateBlendTreeParameters();
        animator.speed = 0f;
        // 设置初始目标位置
        SetRandomTargetPosition();

        // 设置下次移动时间
        SetNextMoveTime();
    }

    private void Update()
    {
        // 键盘输入控制（测试用）
       // HandleKeyboardInput();

        // 自动移动逻辑
        HandleAutoMovement();
    }

    private void HandleKeyboardInput()
    {
        Vector2 inputDirection = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
        );

        if (inputDirection.magnitude > 0.1f)
        {
            SetMovementDirection(inputDirection);
        }
        else
        {
            StopMoving();
        }
    }

    private void HandleAutoMovement()
    {
        // 如果还没到移动时间，直接返回
        if (Time.time < nextMoveTime) return;

        // 如果不在移动状态，开始移动
        if (!isMoving)
        {
            StartMoving();
        }

        // 移动逻辑
        if (isMoving)
        {
            // 计算移动方向
            Vector3 moveDirection = (targetPosition - transform.position).normalized;
            SetMovementDirection(new Vector2(moveDirection.x, moveDirection.y));

            // 移动精灵
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // 检查是否到达目标位置
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                // 到达目标位置，停止移动
                StopMoving();

                // 设置下次移动时间
                SetNextMoveTime();

                // 设置新的目标位置
                SetRandomTargetPosition();
            }
        }
    }

    private void SetRandomTargetPosition()
    {
        // 在移动范围内随机选择一个点
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float randomDistance = Random.Range(minMoveDistance, maxMoveDistance);

        // 计算目标位置（确保在移动半径内）
        Vector3 randomPoint = originPosition + new Vector3(randomDirection.x, randomDirection.y, 0) * randomDistance;

        // 确保目标位置在移动半径内
        if (Vector3.Distance(randomPoint, originPosition) > moveRadius)
        {
            randomPoint = originPosition + (randomPoint - originPosition).normalized * moveRadius;
        }

        targetPosition = randomPoint;
    }

    private void SetNextMoveTime()
    {
        nextMoveTime = Time.time + Random.Range(minMoveInterval, maxMoveInterval);
    }

    public void StartMoving()
    {
        isMoving = true;
        animator.speed = 1.0f;
        //animator.SetFloat("Speed", 1f); // 设置移动速度
    }

    public void StopMoving()
    {
        isMoving = false;
       // animator.SetFloat("Speed", 0f); // 停止移动
        animator.speed = 0f;
    }

    public void SetMovementDirection(Vector2 direction)
    {
        if (direction.magnitude > 0.1f)
        {
            currentDirection = direction.normalized;
            UpdateBlendTreeParameters();
        }
    }

    private void UpdateBlendTreeParameters()
    {
        if (animator != null)
        {
            animator.SetFloat("DirectionX", currentDirection.x);
            animator.SetFloat("DirectionY", currentDirection.y);
        }
    }

    // 在场景视图中显示移动范围
    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            // 绘制移动范围
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(originPosition, moveRadius);

            // 绘制目标位置
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(targetPosition, 0.2f);

            // 绘制当前位置到目标位置的线
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, targetPosition);
        }
    }
}