namespace PixeLadder.SimpleTooltip
{
    using SeerGoGame.Utilities;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// 支持多Canvas的Tooltip管理器，每个Canvas拥有独立的Tooltip实例
    /// </summary>
    public class TooltipManager : MonoBehaviour
    {
        #region Static Instance
        public static TooltipManager Instance { get; private set; }
        #endregion

        #region Fields
        [Header("Core Configuration")]
        [Tooltip("The UI Prefab for the tooltip itself.")]
        [SerializeField] private Tooltip tooltipPrefab;

        [Header("Default Canvas Configuration")]
        [Tooltip("默认Canvas，如果未指定特定Canvas则使用此Canvas")]
        [SerializeField] private Canvas defaultCanvas;

        [Header("Layout Settings")]
        [Tooltip("The maximum width the tooltip can have before its text starts wrapping.")]
        [SerializeField, Min(50f)] private float maxTooltipWidth = 350f;

        [Header("Animation Settings")]
        [Tooltip("The duration of the fade-in and fade-out animations in seconds.")]
        [SerializeField, Min(0f)] private float fadeDuration = 0.2f;

        [Header("Positioning")]
        [Tooltip("An offset to apply to the tooltip's position relative to the mouse cursor.")]
        [SerializeField] private Vector2 positionOffset = new(0, -20);

        // --- 多Canvas支持 ---
        private Dictionary<Canvas, TooltipInstance> canvasTooltips = new Dictionary<Canvas, TooltipInstance>();
        private Dictionary<Canvas, Coroutine> activeCoroutines = new Dictionary<Canvas, Coroutine>();
        private Dictionary<Canvas, Coroutine> autoHideCoroutines = new Dictionary<Canvas, Coroutine>();
        #endregion

        #region Tooltip Instance Data Class
        [System.Serializable]
        private class TooltipInstance
        {
            public Tooltip tooltipComponent;
            public RectTransform rectTransform;
            public CanvasGroup canvasGroup;
            public GameObject gameObject;

            public TooltipInstance(GameObject obj)
            {
                gameObject = obj;
                tooltipComponent = obj.GetComponent<Tooltip>();
                rectTransform = obj.GetComponent<RectTransform>();
                canvasGroup = obj.GetComponent<CanvasGroup>();
            }
        }
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                //DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // 确保有默认Canvas
            if (defaultCanvas == null)
            {
                defaultCanvas = FindFirstObjectByType<Canvas>();
                if (defaultCanvas == null)
                {
                    SeerGoLogger.LogError("TooltipManager Error: No Canvas found in scene. Please add a Canvas.");
                    return;
                }
            }

            SeerGoLogger.Log($"TooltipManager: 使用默认Canvas '{defaultCanvas.name}'");
        }
        #endregion

        #region Public API - 多Canvas支持
        /// <summary>
        /// 为特定Canvas获取或创建Tooltip实例
        /// </summary>
        public void ShowTooltipForCanvas(Canvas targetCanvas, string content, string title = "",
                                       Sprite icon = null, Color? titleColor = null,
                                       Color? iconColor = null, float delay = 0f,
                                       float autoHideDuration = 0f, Sprite icon2 = null)
        {
            if (targetCanvas == null)
            {
                SeerGoLogger.LogWarning("TooltipManager: Target canvas is null, using default canvas.");
                targetCanvas = defaultCanvas;
            }

            if (!canvasTooltips.ContainsKey(targetCanvas))
            {
                CreateTooltipForCanvas(targetCanvas);
            }

            var tooltipInstance = canvasTooltips[targetCanvas];
            if (tooltipInstance == null) return;

            // 停止该Canvas上可能正在运行的协程（包括旧的auto-hide）
            StopCanvasCoroutines(targetCanvas);

            activeCoroutines[targetCanvas] = StartCoroutine(
                ShowRoutine(targetCanvas, content, title, icon, titleColor ?? Color.white,
                          iconColor ?? Color.white, delay, icon2)
            );

            // 如果设置了autoHideDuration，在TooltipManager上启动自动隐藏倒计时
            if (autoHideDuration > 0f)
            {
                autoHideCoroutines[targetCanvas] = StartCoroutine(
                    AutoHideRoutine(targetCanvas, delay + autoHideDuration)
                );
            }
        }

        /// <summary>
        /// 隐藏特定Canvas的Tooltip
        /// </summary>
        public void HideTooltipForCanvas(Canvas targetCanvas)
        {
            if (targetCanvas == null) targetCanvas = defaultCanvas;
            if (targetCanvas == null) return;

            if (canvasTooltips.ContainsKey(targetCanvas))
            {
                var tooltipInstance = canvasTooltips[targetCanvas];
                if (tooltipInstance != null && tooltipInstance.gameObject != null)
                {
                    // 停止该Canvas的所有协程（show + auto-hide）
                    StopCanvasCoroutines(targetCanvas);

                    activeCoroutines[targetCanvas] = StartCoroutine(FadeOut(targetCanvas));
                }
                else
                {
                    // 实例已销毁，清理条目
                    CleanupCanvas(targetCanvas);
                }
            }
        }
        // 隐藏所有Canvas的提示
        public void HideAllTooltips()
        {
            // 遍历所有正在显示的提示
            foreach (var canvas in canvasTooltips.Keys.ToList())  // 使用ToList避免枚举时修改集合的问题
            {
                // 跳过已销毁的Canvas
                if (canvas == null)
                {
                    canvasTooltips.Remove(canvas);
                    continue;
                }

                if (canvasTooltips.ContainsKey(canvas))
                {
                    var tooltipInstance = canvasTooltips[canvas];
                    if (tooltipInstance != null && tooltipInstance.gameObject != null)
                    {
                        // 停止该Canvas的所有协程（show + auto-hide）
                        StopCanvasCoroutines(canvas);

                        // 开始淡出协程
                        activeCoroutines[canvas] = StartCoroutine(FadeOut(canvas));
                    }
                    else
                    {
                        // 实例已销毁，清理条目
                        CleanupCanvas(canvas);
                    }
                }
            }
        }

        /// <summary>
        /// 立即强制隐藏所有Tooltip，无淡出动画
        /// 适用于切地图、关面板、切场景等会销毁触发物体的场景
        /// </summary>
        public void ForceHideAllImmediate()
        {
            // 停止所有show和auto-hide协程
            foreach (var kvp in activeCoroutines)
            {
                if (kvp.Value != null)
                {
                    StopCoroutine(kvp.Value);
                }
            }
            activeCoroutines.Clear();

            foreach (var kvp in autoHideCoroutines)
            {
                if (kvp.Value != null)
                {
                    StopCoroutine(kvp.Value);
                }
            }
            autoHideCoroutines.Clear();

            foreach (var canvas in canvasTooltips.Keys.ToList())
            {
                // 已销毁的Canvas直接移除
                if (canvas == null)
                {
                    canvasTooltips.Remove(canvas);
                    continue;
                }

                var instance = canvasTooltips[canvas];
                if (instance?.gameObject != null)
                {
                    if (instance.canvasGroup != null)
                        instance.canvasGroup.alpha = 0;
                    instance.gameObject.SetActive(false);
                }
                else
                {
                    canvasTooltips.Remove(canvas);
                }
            }
        }
        /// <summary>
        /// 检查特定Canvas是否有激活的Tooltip
        /// </summary>
        public bool HasActiveTooltip(Canvas targetCanvas)
        {
            if (targetCanvas == null) targetCanvas = defaultCanvas;
            return canvasTooltips.ContainsKey(targetCanvas) &&
                   canvasTooltips[targetCanvas] != null &&
                   canvasTooltips[targetCanvas].gameObject.activeInHierarchy;
        }

        /// <summary>
        /// 获取特定Canvas的Tooltip实例（用于高级操作）
        /// </summary>
        public Tooltip GetTooltipForCanvas(Canvas targetCanvas)
        {
            if (targetCanvas == null) targetCanvas = defaultCanvas;
            return canvasTooltips.ContainsKey(targetCanvas) ? canvasTooltips[targetCanvas].tooltipComponent : null;
        }
        #endregion

        #region Private Methods - Tooltip实例管理
        /// <summary>
        /// 停止指定Canvas上的所有协程（show + auto-hide）
        /// </summary>
        private void StopCanvasCoroutines(Canvas targetCanvas)
        {
            if (activeCoroutines.ContainsKey(targetCanvas) && activeCoroutines[targetCanvas] != null)
            {
                StopCoroutine(activeCoroutines[targetCanvas]);
                activeCoroutines.Remove(targetCanvas);
            }

            if (autoHideCoroutines.ContainsKey(targetCanvas) && autoHideCoroutines[targetCanvas] != null)
            {
                StopCoroutine(autoHideCoroutines[targetCanvas]);
                autoHideCoroutines.Remove(targetCanvas);
            }
        }

        /// <summary>
        /// 清理指定Canvas的字典条目
        /// </summary>
        private void CleanupCanvas(Canvas targetCanvas)
        {
            canvasTooltips.Remove(targetCanvas);
            activeCoroutines.Remove(targetCanvas);
            autoHideCoroutines.Remove(targetCanvas);
        }

        /// <summary>
        /// 自动隐藏协程：在TooltipManager上运行，不受触发物体的生命周期影响
        /// </summary>
        private IEnumerator AutoHideRoutine(Canvas targetCanvas, float totalDelay)
        {
            yield return new WaitForSeconds(totalDelay);

            // 只有当前auto-hide协程仍然是活跃的那个才执行隐藏
            // （避免旧协程在已被新协程替换后仍然执行）
            if (autoHideCoroutines.ContainsKey(targetCanvas) &&
                autoHideCoroutines[targetCanvas] == null)
                yield break;

            HideTooltipForCanvas(targetCanvas);
        }

        private void CreateTooltipForCanvas(Canvas targetCanvas)
        {
            if (targetCanvas == null || tooltipPrefab == null) return;

            GameObject tooltipObj = Instantiate(tooltipPrefab.gameObject, targetCanvas.transform, false);
            tooltipObj.name = $"Tooltip_{targetCanvas.name}";

            var tooltipInstance = new TooltipInstance(tooltipObj);
            canvasTooltips[targetCanvas] = tooltipInstance;

            tooltipObj.SetActive(false);
            SeerGoLogger.Log($"TooltipManager: 为Canvas '{targetCanvas.name}'创建Tooltip实例");
        }

        private IEnumerator ShowRoutine(Canvas targetCanvas, string content, string title, Sprite icon,
                                       Color titleColor, Color iconColor, float delay, Sprite icon2 = null)
        {
            if (!canvasTooltips.ContainsKey(targetCanvas)) yield break;

            var tooltipInstance = canvasTooltips[targetCanvas];
            tooltipInstance.canvasGroup.alpha = 0;
            yield return new WaitForSeconds(delay);

            yield return ResizeTooltipRoutine(targetCanvas, content, title, icon, titleColor, iconColor, icon2);

            tooltipInstance.gameObject.SetActive(true);
            tooltipInstance.gameObject.transform.SetAsLastSibling();
            PositionTooltip(targetCanvas);

            activeCoroutines[targetCanvas] = StartCoroutine(FadeIn(targetCanvas));
        }

        private IEnumerator ResizeTooltipRoutine(Canvas targetCanvas, string content, string title,
                                              Sprite icon, Color titleColor, Color iconColor, Sprite icon2 = null)
        {
            if (!canvasTooltips.ContainsKey(targetCanvas)) yield break;

            var tooltipInstance = canvasTooltips[targetCanvas];
            tooltipInstance.gameObject.SetActive(false);

            // 设置文本内容（支持双图标）
            tooltipInstance.tooltipComponent.SetText(content, title, icon, titleColor, iconColor, icon2);

            // 三次布局重建确保正确尺寸
            for (int i = 0; i < 3; i++)
            {
                tooltipInstance.gameObject.SetActive(true);
                LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipInstance.rectTransform);
                yield return new WaitForEndOfFrame();
                tooltipInstance.gameObject.SetActive(false);
            }
        }

        private IEnumerator FadeIn(Canvas targetCanvas)
        {
            if (!canvasTooltips.ContainsKey(targetCanvas)) yield break;

            var tooltipInstance = canvasTooltips[targetCanvas];
            float start = Time.unscaledTime;

            while (Time.unscaledTime < start + fadeDuration)
            {
                if (tooltipInstance.canvasGroup == null) yield break;
                tooltipInstance.canvasGroup.alpha = Mathf.Lerp(0, 1, (Time.unscaledTime - start) / fadeDuration);
                yield return null;
            }

            if (tooltipInstance.canvasGroup != null)
                tooltipInstance.canvasGroup.alpha = 1;
        }

        private IEnumerator FadeOut(Canvas targetCanvas)
        {
            if (!canvasTooltips.ContainsKey(targetCanvas)) yield break;

            var tooltipInstance = canvasTooltips[targetCanvas];
            if (tooltipInstance?.gameObject == null) yield break;

            float start = Time.unscaledTime;
            float startAlpha = tooltipInstance.canvasGroup != null ? tooltipInstance.canvasGroup.alpha : 0;

            while (Time.unscaledTime < start + fadeDuration)
            {
                if (tooltipInstance.canvasGroup == null || tooltipInstance.gameObject == null) yield break;
                tooltipInstance.canvasGroup.alpha = Mathf.Lerp(startAlpha, 0, (Time.unscaledTime - start) / fadeDuration);
                yield return null;
            }

            if (tooltipInstance.canvasGroup != null && tooltipInstance.gameObject != null)
                tooltipInstance.canvasGroup.alpha = 0;

            if (tooltipInstance.gameObject != null)
                tooltipInstance.gameObject.SetActive(false);
        }

        private void PositionTooltip(Canvas targetCanvas)
        {
            if (!canvasTooltips.ContainsKey(targetCanvas)) return;

            var tooltipInstance = canvasTooltips[targetCanvas];
            float outlineSize = 0;

            Outline outline = tooltipInstance.tooltipComponent.GetComponentInChildren<Outline>();
            if (outline != null)
            {
                outlineSize = outline.effectDistance.x;
            }

            Vector3 mousePos = Input.mousePosition + (Vector3)positionOffset;
            float scale = tooltipInstance.rectTransform.lossyScale.x;

            float totalWidth = (tooltipInstance.rectTransform.rect.width + outlineSize) * scale;
            float totalHeight = (tooltipInstance.rectTransform.rect.height + outlineSize) * scale;

            float x = Mathf.Clamp(mousePos.x, 0, Screen.width - totalWidth);
            float y = Mathf.Clamp(mousePos.y, totalHeight, Screen.height);

            tooltipInstance.rectTransform.position = new Vector3(x, y, 0);
        }
        #endregion
    }
}