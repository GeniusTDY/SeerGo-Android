using UnityEngine;
using FTRuntime;
using System.Collections;

/// <summary>
/// 宠物随机行走控制器（单序列帧段 + 点积方向匹配）
///
/// 动画帧段:
///   0- 6  Down      7-14  LeftDown   15-22 Left    23-30 LeftUp
///   31-38 Up        39-46 RightUp    47-54 Right   55-62 RightDown
///
/// 方向选择用点积（Dot），不再按角度区间，避免边界误判。
/// </summary>
[RequireComponent(typeof(SwfClip))]
public class PetWalkController : MonoBehaviour
{
    // =========================================================================
    // 8 方向帧段 + 归一化方向向量（一一对应）
    // 顺序: up rightup right rightdown down leftdown left leftup
    // =========================================================================

    /// <summary>0-based 帧区间 [start, end)，1-based 原始值见注释</summary>
    private static readonly (int start, int end)[] DirFrame = new (int, int)[]
    {
        (31, 39),  // 0: Up        (1-based: 32-39)
        (39, 47),  // 1: RightUp   (1-based: 40-47)
        (47, 55),  // 2: Right     (1-based: 48-55)
        (55, 63),  // 3: RightDown (1-based: 56-63)
        ( 0,  7),  // 4: Down      (1-based: 1-7)
        ( 7, 15),  // 5: LeftDown  (1-based: 8-15)
        (15, 23),  // 6: Left      (1-based: 16-23)
        (23, 31),  // 7: LeftUp    (1-based: 24-31)
    };

    /// <summary>8 方向归一化向量，与 DirFrame 同索引</summary>
    private static readonly Vector2[] DirVec = new Vector2[]
    {
        new Vector2( 0f,        1f),         // 0: up
        new Vector2( 0.7071f,   0.7071f),    // 1: rightup
        new Vector2( 1f,        0f),         // 2: right
        new Vector2( 0.7071f,  -0.7071f),    // 3: rightdown
        new Vector2( 0f,       -1f),         // 4: down
        new Vector2(-0.7071f,  -0.7071f),    // 5: leftdown
        new Vector2(-1f,        0f),         // 6: left
        new Vector2(-0.7071f,   0.7071f),    // 7: leftup
    };

    // =========================================================================
    // Inspector
    // =========================================================================

    [Header("行走")]
    public float walkSpeed   = 2f;
    public float walkRadius  = 3f;
    public float minWalkDist = 0.5f;

    [Header("动画")]
    [Range(0.1f, 5f)]
    [Tooltip("播放速率缩放（1=原始速度, 0.5=半速, 2=双倍）")]
    public float animSpeed = 0.5f;

    [Header("休息")]
    public float minRestTime = 3f;
    public float maxRestTime = 5f;

    // =========================================================================
    // 内部
    // =========================================================================

    private SwfClip           _clip;
    private SwfClipController _ctrl;
    private Vector3           _spawnPos;
    private int               _dirIdx = 4;        // 默认朝下（Down = 索引4）
    private int               _stabilizeFrames;
    private Coroutine         _routine;

    private void Awake()
    {
        _clip = GetComponent<SwfClip>();
        _ctrl = GetComponent<SwfClipController>();
    }

    private void Start()
    {
        _spawnPos = transform.position;
        if (!_ctrl) { Debug.LogError($"[WalkCtrl] {name}: 缺 SwfClipController"); return; }

        _ctrl.loopMode = SwfClipController.LoopModes.Loop;
        _ctrl.rateScale = animSpeed;

        _routine = StartCoroutine(WalkLoop());
    }

    private void OnDisable()
    {
        if (_routine != null) { StopCoroutine(_routine); _routine = null; }
    }

    private void OnValidate()
    {
        if (_ctrl) _ctrl.rateScale = animSpeed;
    }

    // =========================================================================
    // 帧段内循环（LateUpdate 拦截超出范围的帧，只在播放时生效）
    // =========================================================================

    private void LateUpdate()
    {
        if (!_ctrl || !_ctrl.isPlaying || _clip == null) return;

        // 方向刚切换完的稳定期，不重定向（防止抽搐闪烁）
        if (_stabilizeFrames > 0) { --_stabilizeFrames; return; }

        var (start, end) = DirFrame[_dirIdx];
        // 同时检查上下界：Loop 模式下控制器 Rewind 可能跳回 frame 0，
        // 对于末尾方向段（如 RightDown 帧55-62），会漏过仅检查上界的逻辑
        if (_clip.currentFrame < start || _clip.currentFrame >= end)
            _clip.currentFrame = start;
    }

    // =========================================================================
    // 行走循环
    // =========================================================================

    private IEnumerator WalkLoop()
    {
        _clip.currentFrame = DirFrame[_dirIdx].start;
        _stabilizeFrames = 2;
        _ctrl.Play(false);

        while (true)
        {
            _ctrl.Stop(true);
            _clip.currentFrame = DirFrame[_dirIdx].start; // 静止在当前方向首帧
            yield return new WaitForSeconds(Random.Range(minRestTime, maxRestTime));

            // 目标从出生点随机选取
            var target = PickTarget();
            // 方向从 当前位置→目标 计算，确保动画与移动一致
            var moveVec = (Vector2)(target - transform.position);
            _dirIdx = VecToDir(moveVec);
            _stabilizeFrames = 2;
            _ctrl.GotoAndPlay(DirFrame[_dirIdx].start);

            var o    = transform.position;
            var dist = moveVec.magnitude;
            var dur  = Mathf.Max(0.1f, dist / walkSpeed);
            var t    = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(o, target, Mathf.Clamp01(t / dur));
                yield return null;
            }
            transform.position = target;
        }
    }

    // =========================================================================
    // 方向 → 帧段索引（点积法）
    // =========================================================================

    /// <summary>在出生点半径内随机选一个目标</summary>
    private Vector3 PickTarget()
    {
        var a = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        var d = Random.Range(minWalkDist, walkRadius);
        return _spawnPos + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0) * d;
    }

    /// <summary>移动向量与 8 方向点积，取最近方向索引</summary>
    private static int VecToDir(Vector2 move)
    {
        int best    = 0;
        var bestDot = -2f;
        for (int i = 0; i < DirVec.Length; i++)
        {
            var dot = Vector2.Dot(move, DirVec[i]);
            if (dot > bestDot) { bestDot = dot; best = i; }
        }
        return best;
    }

    // =========================================================================
    // 公开
    // =========================================================================

    public void Teleport(Vector3 pos) { _ctrl.Stop(true); transform.position = pos; }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        var c = Application.isPlaying ? _spawnPos : transform.position;
        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Gizmos.DrawWireSphere(c, walkRadius);

        // 画 8 个方向标记
        Gizmos.color = Color.yellow;
        for (int i = 0; i < DirVec.Length; i++)
        {
            Gizmos.DrawLine(c, c + (Vector3)DirVec[i] * (walkRadius + 0.3f));
        }
    }
#endif
}
