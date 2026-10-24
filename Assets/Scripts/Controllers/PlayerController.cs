using QFramework;
using SeerGoGame.Controller;
using SeerGoGame.Events;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SeerGoGame.Controllers
{
    public class PlayerController : BaseController
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 12f;
        [SerializeField] private float moveThreshold = 0.1f;
        [SerializeField] private float directionChangeThreshold = 0.1f;

        [Header("Body Parts")]
        public GameObject Head;
        public GameObject Body;
        public GameObject Body_Bg;
        public GameObject Hand;
        public GameObject Belt;
        public GameObject Foot;
        public GameObject Foot_Bg;
        public GameObject Foot_Effect;

        [Header("NoNo Parts")]
        public GameObject NoNo;
        public GameObject NoNo_Bg;

        [Header("Body Color")]
        public MultiColorGradientController playerColorController;
        public MultiColorGradientController nonoColorController;

        [Header("遮罩设置")]
        [SerializeField] private SpriteRenderer maskSprite;  // 遮罩SpriteRenderer组件
        [SerializeField] private Color unclickableColor = new Color(253f / 255f, 0f / 255f, 60f / 255f, 0.5f);  // 红色区域颜色
        [SerializeField] private float colorTolerance = 0.1f;  // 减小容差，避免误判
        [SerializeField] private float alphaTolerance = 0.2f;  // 透明度容差

        // 组件缓存
        private new Rigidbody2D rigidbody2D;

        // 身体部位管理
        private List<Animator> bodyPartAnimators = new List<Animator>();
        private bool areBodyPartsInitialized = false;

        // 动画参数哈希
        private readonly int DirectionXHash = Animator.StringToHash("DirectionX");
        private readonly int DirectionYHash = Animator.StringToHash("DirectionY");
        private readonly int SpeedHash = Animator.StringToHash("Speed");

        // 移动状态
        private Vector3 targetPosition = new Vector3(0, -2, 0);
        private Vector2 lastDirection = Vector2.down;
        private bool isMoving = false;
        private float currentSpeed = 0f;

        // 遮罩纹理缓存
        private Texture2D maskTexture = null;
        private Rect maskWorldBounds = new Rect();

        #region 初始化系统
        void Awake()
        {
            InitializeComponents();
        }

        void Start()
        {
            InitializeBodyParts();
            InitializeEventListeners();
            SetInitialIdleState();
            // 不再强制覆盖渐变色，由Inspector控制
            // nonoColorController.ApplyFireGradient();
            // ColorUtility.TryParseHtmlString("#FF0000", out Color color);
            // ColorUtility.TryParseHtmlString("#FFEB04", out Color color2);
            // playerColorController.SetGradientColors(color, color2, color);
        }

        private void InitializeComponents()
        {
            // 缓存组件引用
            rigidbody2D = GetComponent<Rigidbody2D>();

            if (rigidbody2D == null)
            {
                rigidbody2D = gameObject.AddComponent<Rigidbody2D>();
                ConfigureRigidbody();
            }
        }

        private void ConfigureRigidbody()
        {
            // 优化刚体设置
            rigidbody2D.gravityScale = 0;
            rigidbody2D.freezeRotation = true;
            rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        private void InitializeBodyParts()
        {
            if (areBodyPartsInitialized) return;

            bodyPartAnimators.Clear();
            GameObject[] bodyParts = { Head, Body, Body_Bg, Hand, Belt, Foot, Foot_Bg, Foot_Effect, NoNo, NoNo_Bg };

            foreach (var part in bodyParts)
            {
                if (part != null)
                {
                    var animator = part.GetComponent<Animator>();
                    if (animator != null)
                    {
                        bodyPartAnimators.Add(animator);
                    }
                }
            }
            areBodyPartsInitialized = true;
        }

        private void InitializeEventListeners()
        {
            this.RegisterEvent<InitPlayerPositionResultEvent>(OnInitPlayerPositionChanged)
               .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<PlayerPositionResultEvent>(OnPlayerPositionChanged)
               .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void SetInitialIdleState()
        {
            // 设置初始空闲状态
            Vector2 initDirection = new Vector2(0, -1);
            SynchronizeBodyParts(initDirection, 0f);
            lastDirection = initDirection;
        }
        #endregion

        #region 更新循环
        private void Update()
        {
            HandlePlayerInput();
            UpdateAnimationState();

#if UNITY_EDITOR
            // 颜色测试按键（仅编辑器调试用）
            if (Input.GetKeyDown(KeyCode.Z))
            {
                playerColorController.RandomizeGradient();
                nonoColorController.RandomizeGradient();
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                playerColorController.ApplyOceanGradient();
                nonoColorController.ApplyOceanGradient();
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                playerColorController.ApplyFireGradient();
                nonoColorController.ApplyFireGradient();
            }
            else if (Input.GetKeyDown(KeyCode.V))
            {
                playerColorController.ApplyRainbowGradient();
                nonoColorController.ApplyRainbowGradient();
            }
            else if (Input.GetKeyDown(KeyCode.B))
            {
                playerColorController.ApplyForestGradient();
                nonoColorController.ApplyForestGradient();
            }
            else if (Input.GetKeyDown(KeyCode.N))
            {
                playerColorController.ApplyOceanGradient();
                nonoColorController.ApplyOceanGradient();
            }
#endif
        }

        private void FixedUpdate()
        {
            HandleMovement();
        }
        #endregion

        #region 输入处理系统
        private void HandlePlayerInput()
        {
#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
            {
                SetTargetPositionFromMouse();
            }
#elif UNITY_ANDROID
           if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
        
                // 只检测触摸开始事件
                if (touch.phase == TouchPhase.Began && !IsTouchOverUI())
                {
                    SetTargetPositionFromMouse();
                }
            }
#else
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
            {
                SetTargetPositionFromMouse();
            }
#endif
        }
        private bool IsTouchOverUI()
        {
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        }
        // PC触摸检测
        private bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        private void SetTargetPositionFromMouse()
        {
            if (Camera.main == null) return;

            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;

            // 检查是否点击到遮罩区域
            bool canMove = CheckIfClickable(mouseWorldPos);

            if (canMove)
            {
                targetPosition = mouseWorldPos;

                // 立即更新方向预览
                UpdateDirectionPreview();
            }
        }

        /// <summary>
        /// 检查点击位置是否可移动
        /// </summary>
        private bool CheckIfClickable(Vector3 worldPos)
        {
            // 如果没有遮罩组件或者遮罩Sprite为空，直接允许移动
            if (maskSprite == null || maskSprite.sprite == null)
            {
                return true; // 无遮罩图片，默认允许移动
            }

            // 更新世界边界
            UpdateMaskWorldBounds();

            // 检查点是否在遮罩边界内
            if (!maskWorldBounds.Contains(worldPos))
            {
                return true;
            }

            // 获取点击位置的颜色
            Color clickedColor = GetColorAtPoint(worldPos);

            // 判断是否可点击
            bool isClickable = IsColorClickable(clickedColor);

            return isClickable;
        }

        /// <summary>
        /// 更新遮罩的世界边界
        /// </summary>
        private void UpdateMaskWorldBounds()
        {
            if (maskSprite == null) return;

            Vector3 position = maskSprite.transform.position;
            Vector2 size = maskSprite.size;

            // 计算世界空间中的边界
            maskWorldBounds = new Rect(
                position.x - size.x / 2f,
                position.y - size.y / 2f,
                size.x,
                size.y
            );
        }

        /// <summary>
        /// 获取点击位置的颜色（拉伸模式）
        /// </summary>
        private Color GetColorAtPoint(Vector3 worldPos)
        {
            if (maskSprite == null || maskSprite.sprite == null)
            {
                return Color.clear;
            }

            Sprite sprite = maskSprite.sprite;

            // 缓存纹理信息
            if (maskTexture == null || sprite.texture != maskTexture)
            {
                maskTexture = sprite.texture;

                if (maskTexture == null)
                {
                    return Color.clear;
                }
            }

            // 确保纹理可读取
            if (!maskTexture.isReadable)
            {
                SeerGoLogger.LogError($"纹理{maskTexture.name}不可读，请开启Read/Write Enabled");
                return Color.clear;
            }

            try
            {
                // 将世界坐标转换为相对于遮罩的局部UV坐标
                // 因为遮罩是拉伸的，所以直接使用比例
                float u = (worldPos.x - maskWorldBounds.xMin) / maskWorldBounds.width;
                float v = (worldPos.y - maskWorldBounds.yMin) / maskWorldBounds.height;

                // 确保UV在[0,1]范围内
                u = Mathf.Clamp01(u);
                v = Mathf.Clamp01(v);

                // 转换为纹理像素坐标
                int pixelX = Mathf.FloorToInt(u * maskTexture.width);
                int pixelY = Mathf.FloorToInt(v * maskTexture.height);

                // 边界检查
                pixelX = Mathf.Clamp(pixelX, 0, maskTexture.width - 1);
                pixelY = Mathf.Clamp(pixelY, 0, maskTexture.height - 1);

                // 获取颜色
                Color color = maskTexture.GetPixel(pixelX, pixelY);

                // 添加调试信息

                return color;
            }
            catch (System.Exception e)
            {
                SeerGoLogger.LogError($"获取像素颜色失败: {e.Message}");
                return Color.clear;
            }
        }

        /// <summary>
        /// 判断颜色是否可点击
        /// </summary>
        private bool IsColorClickable(Color color)
        {
            // 1. 检查透明度：如果颜色非常透明，直接允许点击
            if (color.a < 0.1f) // 完全透明
            {
                return true;
            }

            // 2. 直接比较RGBA值
            float rDiff = Mathf.Abs(color.r - unclickableColor.r);
            float gDiff = Mathf.Abs(color.g - unclickableColor.g);
            float bDiff = Mathf.Abs(color.b - unclickableColor.b);
            float aDiff = Mathf.Abs(color.a - unclickableColor.a);


            // 3. 判断是否为不可点击颜色
            // 使用更精确的判断：当RGBA都接近不可点击颜色时，才判断为不可点击
            bool isUnclickableColor = rDiff <= colorTolerance &&
                                      gDiff <= colorTolerance &&
                                      bDiff <= colorTolerance &&
                                      aDiff <= alphaTolerance;

            if (isUnclickableColor)
            {
                return false;
            }

            return true;
        }

        private void UpdateDirectionPreview()
        {
            Vector2 direction = (targetPosition - transform.position).normalized;
            if (direction.magnitude > 0.1f)
            {
                SynchronizeBodyParts(direction, 1f);
            }
        }
#endregion

        #region 移动控制系统
        private void HandleMovement()
        {
            if (rigidbody2D == null) return;

            Vector2 toTarget = targetPosition - transform.position;
            float distance = toTarget.magnitude;

            // 添加移动阈值判断，避免微小距离的抖动
            if (distance <= moveThreshold)
            {
                StopMovement();
                return;
            }

            MoveTowardsTarget(toTarget, distance);
        }

        private void StopMovement()
        {
            if (isMoving)
            {
                rigidbody2D.velocity = Vector2.zero;
                isMoving = false;
                currentSpeed = 0f;
                SynchronizeBodyPartsSpeed(0f);
            }
        }

        private void MoveTowardsTarget(Vector2 toTarget, float distance)
        {
            // 计算方向并应用移动
            Vector2 direction = toTarget.normalized;
            Vector2 newPosition = Vector2.MoveTowards(
                rigidbody2D.position,
                targetPosition,
                moveSpeed * Time.fixedDeltaTime
            );

            rigidbody2D.MovePosition(newPosition);
            isMoving = true;
            currentSpeed = moveSpeed;

            // 只有方向变化超过阈值时才更新动画
            if (ShouldUpdateDirection(direction))
            {
                lastDirection = direction;
                SynchronizeBodyParts(direction, 1f);
            }
        }

        private bool ShouldUpdateDirection(Vector2 newDirection)
        {
            // 方向变化阈值检查，避免频繁更新
            return Vector2.Angle(lastDirection, newDirection) > directionChangeThreshold;
        }
        #endregion

        #region 动画同步系统
        private void UpdateAnimationState()
        {
            // 实时更新移动状态动画参数
            if (isMoving)
            {
                SynchronizeBodyParts(lastDirection, 1f);
            }
        }

        private void SynchronizeBodyParts(Vector2 direction, float speed)
        {
            // 使用哈希参数提高性能
            foreach (var animator in bodyPartAnimators)
            {
                if (animator != null && animator.isActiveAndEnabled)
                {
                    animator.SetFloat(DirectionXHash, direction.x);
                    animator.SetFloat(DirectionYHash, direction.y);
                    animator.SetFloat(SpeedHash, speed);
                }
            }
        }

        private void SynchronizeBodyPartsSpeed(float speed)
        {
            foreach (var animator in bodyPartAnimators)
            {
                if (animator != null && animator.isActiveAndEnabled)
                {
                    animator.SetFloat(SpeedHash, speed);
                }
            }
        }
        #endregion

        #region 事件处理系统
        private void OnInitPlayerPositionChanged(InitPlayerPositionResultEvent e)
        {
            if (e.Success)
            {
                UpdateInitialPlayerPosition(e);
            }
            else
            {
                HandlePositionError(e.ErrorMessage);
            }
        }

        private void OnPlayerPositionChanged(PlayerPositionResultEvent e)
        {
            targetPosition = ParseUtils.ParseVector3(e.Position);
        }

        private void UpdateInitialPlayerPosition(InitPlayerPositionResultEvent e)
        {
            Vector2 pos = ParseUtils.ParseVector2(e.Position);
            Vector3 targetPos = new Vector3(pos.x, pos.y, 0);

            rigidbody2D.position = targetPos;
            targetPosition = targetPos;

            // 重置移动状态
            StopMovement();
            SetInitialIdleState();
        }

        private void HandlePositionError(string error)
        {
            SeerGoLogger.LogError($"控制器: 赛尔位置失败 - {error}");
        }
        #endregion

        #region 公共方法
        public void SetMoveSpeed(float speed)
        {
            moveSpeed = Mathf.Max(0, speed);
        }

        public void SetTargetPosition(Vector3 position)
        {
            targetPosition = position;
            targetPosition.z = 0;
        }

        public void StopPlayerMovement()
        {
            targetPosition = transform.position;
            StopMovement();
        }

        public bool IsPlayerMoving()
        {
            return isMoving;
        }

        public Vector2 GetCurrentDirection()
        {
            return lastDirection;
        }

        /// <summary>
        /// 设置遮罩Sprite（拉伸模式）
        /// </summary>
        public void SetMaskSprite(Sprite sprite)
        {
            if (maskSprite != null)
            {
                maskSprite.sprite = sprite;
                maskSprite.size = new Vector2(18f, 10f);
                maskSprite.drawMode = SpriteDrawMode.Simple; // 使用Simple模式，手动拉伸

                // 设置SpriteRenderer的scale来拉伸图片
                if (sprite != null)
                {
                    // 计算拉伸比例
                    Vector2 originalSize = sprite.bounds.size;
                    float scaleX = 18f / originalSize.x;
                    float scaleY = 10f / originalSize.y;
                    maskSprite.transform.localScale = new Vector3(scaleX, scaleY, 1f);
                }


                // 重置纹理缓存
                maskTexture = null;

                // 检查纹理设置
                if (sprite != null && sprite.texture != null)
                {
                    maskTexture = sprite.texture;
                }
            }
            else
            {
                Debug.LogWarning("没有设置遮罩SpriteRenderer组件");
            }
        }

        /// <summary>
        /// 清除遮罩图片
        /// </summary>
        public void ClearMaskSprite()
        {
            SetMaskSprite(null);
        }

        #endregion

        #region 资源管理
        private void OnDestroy()
        {
            // 清理资源
            bodyPartAnimators.Clear();
        }
        #endregion
    }
}

