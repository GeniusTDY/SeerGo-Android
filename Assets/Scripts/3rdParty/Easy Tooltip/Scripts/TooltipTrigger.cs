namespace PixeLadder.SimpleTooltip
{
    using UnityEngine;
    using UnityEngine.EventSystems;

    /// <summary>
    /// 支持多Canvas的Tooltip触发器
    /// </summary>
    [ExecuteAlways]
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        #region Fields
        [Header("Tooltip Content")]
        [SerializeField] private string title;
        [TextArea(3, 10)]
        [SerializeField] private string content;
        [SerializeField] private Sprite icon;
        [SerializeField] private Sprite icon2;

        [Header("Custom Styles")]
        [SerializeField] private Color titleColor = Color.white;
        [SerializeField] private Color iconColor = Color.white;

        [Header("Settings")]
        [SerializeField, Min(0f)] private float hoverDelay = 0.5f;

        [Header("Auto Hide")]
        [Tooltip("提示框显示后多少秒自动隐藏，0或负数则不自动隐藏")]
        [SerializeField, Min(0f)] private float autoHideDuration = 5f;

        [Header("Canvas Configuration")]
        [Tooltip("指定此Tooltip使用的Canvas。如果为空，将自动查找父级Canvas")]
        [SerializeField] private Canvas targetCanvas;
        #endregion

        #region Runtime State
        // auto-hide 逻辑已移至 TooltipManager 管理，不再在 Trigger 上维护协程
        #endregion

        #region Public Properties
        public string Title { get => title; set => title = value; }
        public string Content { get => content; set => content = value; }
        public Sprite Icon { get => icon; set => icon = value; }
        public Sprite Icon2 { get => icon2; set => icon2 = value; }
        public Color TitleColor { get => titleColor; set => titleColor = value; }
        public Color IconColor { get => iconColor; set => iconColor = value; }
        public float HoverDelay { get => hoverDelay; set => hoverDelay = value; }
        public float AutoHideDuration { get => autoHideDuration; set => autoHideDuration = value; }

        public Canvas TargetCanvas
        {
            get
            {
                if (targetCanvas == null)
                {
                    // 自动获取父级Canvas
                    targetCanvas = GetComponentInParent<Canvas>();
                }
                return targetCanvas;
            }
            set => targetCanvas = value;
        }
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            EnsureManagerExists();
        }

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                EnsureManagerExists();
            }
        }

        private void OnDisable()
        {
            // auto-hide 已由 TooltipManager 管理，无需在此停止
        }

        private void OnDestroy()
        {
            // 当触发物体被销毁时（切地图、关面板等），自动隐藏对应Canvas的tooltip
            if (Application.isPlaying && TooltipManager.Instance != null)
            {
                TooltipManager.Instance.HideTooltipForCanvas(targetCanvas);
            }
        }
        #endregion

        #region Public Static API - 增强版多Canvas支持
        /// <summary>
        /// 为GameObject添加Tooltip（自动检测Canvas）
        /// </summary>
        public static TooltipTrigger AddTooltip(GameObject target, string content, string title = "", Color titleColor = default, Sprite icon = null, Sprite icon2 = null)
        {
            return AddTooltip(target, content, title, titleColor, icon, null, icon2);
        }

        /// <summary>
        /// 为GameObject添加Tooltip（指定特定Canvas，支持双图标）
        /// </summary>
        public static TooltipTrigger AddTooltip(GameObject target, string content, string title = "", Color titleColor = default,
                                               Sprite icon = null, Canvas canvas = null, Sprite icon2 = null)
        {
            if (target == null)
            {
                Debug.LogError("TooltipTrigger Error: Target GameObject is null.");
                return null;
            }

            EnsureManagerExists();

            TooltipTrigger trigger = target.GetComponent<TooltipTrigger>() ?? target.AddComponent<TooltipTrigger>();

            trigger.Content = content;
            trigger.Title = title;
            trigger.Icon = icon;
            trigger.Icon2 = icon2;
            trigger.TitleColor = titleColor;

            if (canvas != null)
            {
                trigger.TargetCanvas = canvas;
            }

            return trigger;
        }

        /// <summary>
        /// 批量为一组GameObject添加Tooltip（使用相同Canvas）
        /// </summary>
        public static void AddTooltipsToGroup(GameObject[] targets, string content, string title = "", Color titleColor = default,
                                            Sprite icon = null, Canvas canvas = null)
        {
            foreach (GameObject target in targets)
            {
                if (target != null)
                {
                    AddTooltip(target, content, title, titleColor, icon, canvas);
                }
            }
        }
        #endregion

        #region Interface Implementations
        public void OnPointerEnter(PointerEventData eventData)
        {
            Canvas canvas = TargetCanvas;
            if (TooltipManager.Instance != null && canvas != null)
            {
                TooltipManager.Instance.ShowTooltipForCanvas(
                    canvas, content, title, icon, titleColor, iconColor, hoverDelay,
                    autoHideDuration, icon2
                );
            }
            else if (TooltipManager.Instance != null)
            {
                Debug.LogWarning($"TooltipTrigger: 无法确定Canvas，使用默认Canvas");
                TooltipManager.Instance.ShowTooltipForCanvas(
                    null, content, title, icon, titleColor, iconColor, hoverDelay,
                    autoHideDuration, icon2
                );
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Canvas canvas = TargetCanvas;
            // 鼠标离开，隐藏tooltip（auto-hide倒计时由TooltipManager管理，HideTooltipForCanvas会停止它）
            if (TooltipManager.Instance != null)
            {
                TooltipManager.Instance.HideTooltipForCanvas(canvas);
            }
        }
        #endregion

        #region Private Helper Methods
        private static void EnsureManagerExists()
        {
            if (TooltipManager.Instance != null || FindFirstObjectByType<TooltipManager>() != null)
            {
                return;
            }

            const string prefabPath = "TooltipManager";
            GameObject managerPrefab = Resources.Load<GameObject>(prefabPath);

            if (managerPrefab == null)
            {
                Debug.LogError($"TooltipTrigger Error: Could not find the 'TooltipManager' prefab in any 'Resources' folder.");
                return;
            }

            GameObject managerInstance = Instantiate(managerPrefab);
            managerInstance.name = "TooltipManager (Auto-Generated)";
        }
        #endregion
    }
}