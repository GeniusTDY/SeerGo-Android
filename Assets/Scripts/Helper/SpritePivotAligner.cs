using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SeerGoGame.Utilities;

public class SpritePivotAligner : MonoBehaviour
{
    [Header("精灵设置")]
    [SerializeField] private SpriteRenderer targetSprite;
    [SerializeField] public bool disableSpriteDuringAlignment = true; // 添加这个选项

    [Header("不透明区域计算")]
    [Range(0.01f, 0.5f)]
    public float vertexThreshold = 0.1f; // 顶点去重阈值
    public bool useMeshVertices = true; // 使用网格顶点代替纹理采样

    [Header("Pivot位置")]
    public AlignmentType pivotAlignment = AlignmentType.Center;

    [Header("目标位置设置")]
    public Vector3 targetWorldPositionForPositiveScale = new Vector3(-4f, 0f, 0f);
    public Vector3 targetWorldPositionForNegativeScale = new Vector3(4f, 0f, 0f);
    public bool useAutoTargetPosition = true;
    public Vector3 customTargetWorldPosition = Vector3.zero;
    public bool liveUpdate = true;

    [Header("调试")]
    public bool showDebugInfo = false;
    public bool drawBoundsGizmo = false;
    public bool drawMeshVertices = false;

    [System.Serializable]
    public class BoundsInfo
    {
        public Rect opaqueBounds;
        public Vector2 opaqueCenter;
        public Vector2 selectedPivot;
        public Vector2[] meshVertices;
        public int vertexCount;
        public int uniqueVertexCount;
    }

    public BoundsInfo boundsInfo = new BoundsInfo();

    public enum AlignmentType
    {
        Center, Top, Bottom, Left, Right,
        TopLeft, TopRight, BottomLeft, BottomRight
    }

    private bool isInitialized = false;
    private bool isAligning = false;
    private Coroutine currentAlignmentCoroutine;

    void Start()
    {
        AutoGetSpriteRenderer();

        // 延迟一帧初始化，确保Sprite完全加载
        if (gameObject.activeInHierarchy)
        {
            currentAlignmentCoroutine = StartCoroutine(DelayedInitialize());
        }
    }

    private IEnumerator DelayedInitialize()
    {
        yield return null; // 等待一帧

        CalculateOpaqueBoundsFromMesh();
        ApplyTargetPosition();
        isInitialized = true;

        if (showDebugInfo)
        {
            SeerGoLogger.Log($"✅ SpritePivotAligner初始化完成 - 顶点数: {boundsInfo.vertexCount}, 唯一顶点: {boundsInfo.uniqueVertexCount}");
        }
    }

    void OnEnable()
    {
        // 如果重新激活，重新初始化
        if (!isInitialized && targetSprite != null && targetSprite.sprite != null)
        {
            StartCoroutine(DelayedInitialize());
        }
    }

    void OnDisable()
    {
        // 停止所有协程
        if (currentAlignmentCoroutine != null)
        {
            StopCoroutine(currentAlignmentCoroutine);
            currentAlignmentCoroutine = null;
        }
        isAligning = false;
    }

    void Update()
    {
        if (liveUpdate && Application.isPlaying && isInitialized && !isAligning)
        {
            ApplyTargetPosition();
        }
    }

    private void AutoGetSpriteRenderer()
    {
        if (targetSprite == null)
        {
            targetSprite = GetComponent<SpriteRenderer>();
        }
    }

    public void CalculateOpaqueBoundsFromMesh()
    {
        if (!ValidateSprite()) return;

        Sprite sprite = targetSprite.sprite;
        if (sprite == null) return;

        // 方法1: 使用网格顶点（最适合图集）
        if (useMeshVertices && sprite.vertices != null && sprite.vertices.Length > 0)
        {
            CalculateFromMeshVertices(sprite);
        }
        else
        {
            // 方法2: 使用边界框（备用）
            CalculateFromBoundingBox(sprite);
        }

        CalculateSelectedPivot();

        if (showDebugInfo)
        {
            SeerGoLogger.Log($"✅ 不透明区域计算完成！边界: {boundsInfo.opaqueBounds}");
        }
    }

    private void CalculateFromMeshVertices(Sprite sprite)
    {
        Vector2[] vertices = sprite.vertices;
        boundsInfo.vertexCount = vertices.Length;

        if (vertices.Length == 0)
        {
            CalculateFromBoundingBox(sprite);
            return;
        }

        // 去重并计算边界
        List<Vector2> uniqueVertices = new List<Vector2>();
        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;

        foreach (Vector2 vertex in vertices)
        {
            // 检查是否为重复顶点
            bool isDuplicate = false;
            foreach (Vector2 existing in uniqueVertices)
            {
                if (Vector2.Distance(vertex, existing) < vertexThreshold)
                {
                    isDuplicate = true;
                    break;
                }
            }

            if (!isDuplicate)
            {
                uniqueVertices.Add(vertex);
                minX = Mathf.Min(minX, vertex.x);
                maxX = Mathf.Max(maxX, vertex.x);
                minY = Mathf.Min(minY, vertex.y);
                maxY = Mathf.Max(maxY, vertex.y);
            }
        }

        boundsInfo.uniqueVertexCount = uniqueVertices.Count;
        boundsInfo.meshVertices = uniqueVertices.ToArray();

        if (uniqueVertices.Count > 0)
        {
            boundsInfo.opaqueBounds = new Rect(minX, minY, maxX - minX, maxY - minY);
            boundsInfo.opaqueCenter = boundsInfo.opaqueBounds.center;
        }
        else
        {
            CalculateFromBoundingBox(sprite);
        }
    }

    private void CalculateFromBoundingBox(Sprite sprite)
    {
        // 使用三角形边界作为备用方案
        Vector2[] vertices = sprite.vertices;
        if (vertices != null && vertices.Length > 0)
        {
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;

            foreach (Vector2 vertex in vertices)
            {
                minX = Mathf.Min(minX, vertex.x);
                maxX = Mathf.Max(maxX, vertex.x);
                minY = Mathf.Min(minY, vertex.y);
                maxY = Mathf.Max(maxY, vertex.y);
            }

            boundsInfo.opaqueBounds = new Rect(minX, minY, maxX - minX, maxY - minY);
            boundsInfo.meshVertices = vertices;
        }
        else
        {
            // 最终回退：使用精灵的边界
            Bounds bounds = sprite.bounds;
            boundsInfo.opaqueBounds = new Rect(bounds.min.x, bounds.min.y, bounds.size.x, bounds.size.y);
            boundsInfo.meshVertices = new Vector2[0];
        }

        boundsInfo.opaqueCenter = boundsInfo.opaqueBounds.center;
        boundsInfo.vertexCount = boundsInfo.meshVertices.Length;
        boundsInfo.uniqueVertexCount = boundsInfo.vertexCount;
    }

    private Vector3 GetCurrentTargetPosition()
    {
        if (useAutoTargetPosition)
        {
            float scaleX = transform.localScale.x;
            return scaleX >= 0 ? targetWorldPositionForPositiveScale : targetWorldPositionForNegativeScale;
        }
        return customTargetWorldPosition;
    }

    public void MovePivotToTargetPosition()
    {
        if (!ValidateSprite()) return;

        if (boundsInfo.opaqueBounds.width == 0 || boundsInfo.opaqueBounds.height == 0)
        {
            CalculateOpaqueBoundsFromMesh();
        }

        Vector3 targetPos = GetCurrentTargetPosition();
        MovePivotToPosition(targetPos);

        if (showDebugInfo)
        {
            SeerGoLogger.Log($"✅ 不透明区域中心点已移动到世界坐标: {targetPos}");
        }
    }

    private void CalculateSelectedPivot()
    {
        boundsInfo.selectedPivot = GetAlignmentPoint(boundsInfo.opaqueBounds, pivotAlignment);
    }

    private Vector2 GetAlignmentPoint(Rect bounds, AlignmentType type)
    {
        return type switch
        {
            AlignmentType.Center => bounds.center,
            AlignmentType.Top => new Vector2(bounds.center.x, bounds.yMax),
            AlignmentType.Bottom => new Vector2(bounds.center.x, bounds.yMin),
            AlignmentType.Left => new Vector2(bounds.xMin, bounds.center.y),
            AlignmentType.Right => new Vector2(bounds.xMax, bounds.center.y),
            AlignmentType.TopLeft => new Vector2(bounds.xMin, bounds.yMax),
            AlignmentType.TopRight => new Vector2(bounds.xMax, bounds.yMax),
            AlignmentType.BottomLeft => new Vector2(bounds.xMin, bounds.yMin),
            AlignmentType.BottomRight => new Vector2(bounds.xMax, bounds.yMin),
            _ => bounds.center
        };
    }

    public void MovePivotToPosition(Vector3 targetPosition)
    {
        Vector3 currentPivotWorldPos = GetSelectedPivotWorldPosition();
        Vector3 offset = targetPosition - currentPivotWorldPos;

        // 直接设置最终位置，而不是累加
        transform.position += offset;
    }

    public Vector3 GetSelectedPivotWorldPosition()
    {
        if (targetSprite == null || targetSprite.sprite == null)
            return transform.position;

        return LocalToWorldPoint(boundsInfo.selectedPivot, targetSprite);
    }

    private Vector3 LocalToWorldPoint(Vector2 localPoint, SpriteRenderer renderer)
    {
        return renderer.transform.TransformPoint(localPoint);
    }

    private bool ValidateSprite()
    {
        if (targetSprite == null)
        {
            SeerGoLogger.LogError("请先分配目标精灵");
            return false;
        }
        if (targetSprite.sprite == null)
        {
            SeerGoLogger.LogError("精灵缺少Sprite引用");
            return false;
        }
        return true;
    }

    private void ApplyTargetPosition()
    {
        if (!ValidateSprite() || isAligning) return;

        if (boundsInfo.opaqueBounds.width == 0 || boundsInfo.opaqueBounds.height == 0)
        {
            CalculateOpaqueBoundsFromMesh();
        }

        MovePivotToPosition(GetCurrentTargetPosition());
    }

    /// <summary>
    /// 同步初始化：设置缩放、计算边界、移动pivot到目标位置，完成后禁用组件避免后续Update干扰
    /// </summary>
    public void InitializeSync(Vector3 targetWorldPos, Vector3 scale)
    {
        if (targetSprite == null)
            targetSprite = GetComponent<SpriteRenderer>();

        if (targetSprite == null || targetSprite.sprite == null)
        {
            transform.localScale = scale;
            transform.position = targetWorldPos;
            return;
        }

        transform.localScale = scale;

        CalculateOpaqueBoundsFromMesh();
        MovePivotToPosition(targetWorldPos);

        liveUpdate = false;
        showDebugInfo = false;
        disableSpriteDuringAlignment = false;
        enabled = false;

        isInitialized = true;
    }

    // 公共方法
    public void CalculateAndMove()
    {
        isAligning = true;

        if (disableSpriteDuringAlignment && targetSprite != null)
        {
            targetSprite.enabled = false;
        }

        CalculateOpaqueBoundsFromMesh();
        MovePivotToTargetPosition();

        if (disableSpriteDuringAlignment && targetSprite != null)
        {
            targetSprite.enabled = true;
        }

        isAligning = false;
    }

    public IEnumerator CalculateAndMoveAsync()
    {
        isAligning = true;

        if (disableSpriteDuringAlignment && targetSprite != null)
        {
            targetSprite.enabled = false;
        }

        CalculateOpaqueBoundsFromMesh();
        MovePivotToTargetPosition();

        yield return null; // 等待一帧

        if (disableSpriteDuringAlignment && targetSprite != null)
        {
            targetSprite.enabled = true;
        }

        isAligning = false;
    }

    public void UsePositiveScaleTarget()
    {
        useAutoTargetPosition = true;
        Vector3 scale = transform.localScale;
        if (scale.x < 0)
        {
            scale.x = Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
        MovePivotToTargetPosition();
    }

    public void UseNegativeScaleTarget()
    {
        useAutoTargetPosition = true;
        Vector3 scale = transform.localScale;
        if (scale.x > 0)
        {
            scale.x = -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
        MovePivotToTargetPosition();
    }

    void OnDrawGizmosSelected()
    {
        if (!showDebugInfo || targetSprite == null) return;
        if (boundsInfo.opaqueBounds.width == 0 || boundsInfo.opaqueBounds.height == 0) return;

        // 绘制不透明区域边界框
        Gizmos.color = Color.green;
        Vector2[] corners = new Vector2[]
        {
            new Vector2(boundsInfo.opaqueBounds.xMin, boundsInfo.opaqueBounds.yMin),
            new Vector2(boundsInfo.opaqueBounds.xMax, boundsInfo.opaqueBounds.yMin),
            new Vector2(boundsInfo.opaqueBounds.xMax, boundsInfo.opaqueBounds.yMax),
            new Vector2(boundsInfo.opaqueBounds.xMin, boundsInfo.opaqueBounds.yMax)
        };

        Vector3[] worldCorners = new Vector3[4];
        for (int i = 0; i < 4; i++)
        {
            worldCorners[i] = LocalToWorldPoint(corners[i], targetSprite);
        }

        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawLine(worldCorners[i], worldCorners[(i + 1) % 4]);
        }

        // 绘制网格顶点（如果启用）
        if (drawMeshVertices && boundsInfo.meshVertices != null)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
            foreach (var vertex in boundsInfo.meshVertices)
            {
                Vector3 worldPoint = LocalToWorldPoint(vertex, targetSprite);
                Gizmos.DrawWireSphere(worldPoint, 0.02f);
            }
        }

        // 绘制不透明区域中心点
        Vector3 worldOpaqueCenter = LocalToWorldPoint(boundsInfo.opaqueCenter, targetSprite);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(worldOpaqueCenter, 0.05f);

        // 绘制选中的pivot点
        Vector3 worldPivot = GetSelectedPivotWorldPosition();
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(worldPivot, 0.08f);

        // 绘制目标位置
        Vector3 targetPos = GetCurrentTargetPosition();
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(targetPos, Vector3.one * 0.1f);
        Gizmos.DrawLine(worldPivot, targetPos);

        // 显示信息
#if UNITY_EDITOR
        string info = $"Vertices: {boundsInfo.vertexCount} -> {boundsInfo.uniqueVertexCount}";
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, info);
#endif
    }
}