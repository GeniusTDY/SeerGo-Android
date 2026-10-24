namespace PixeLadder.SimpleTooltip
{
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    /// <summary>
    /// A simple "view" component that displays tooltip data. It is controlled by the TooltipManager.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class Tooltip : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("The parent object for the title and icon. Can be hidden if both are absent.")]
        [SerializeField] private GameObject header;
        [Tooltip("The TextMeshPro component for the title text.")]
        [SerializeField] public TextMeshProUGUI titleField;
        [Tooltip("The TextMeshPro component for the main content text.")]
        [SerializeField] public TextMeshProUGUI contentField;
        [Tooltip("The Image component for the icon.")]
        [SerializeField] private Image iconField;
        [Tooltip("The second Image component for dual icons (e.g. dual attribute text).")]
        [SerializeField] private Image iconField2;

        private void Awake()
        {
            // 若未在prefab中指定iconField2，自动创建在iconField旁边
            if (iconField2 == null && iconField != null)
            {
                GameObject icon2Obj = Instantiate(iconField.gameObject, iconField.transform.parent);
                icon2Obj.name = "Icon2";
                iconField2 = icon2Obj.GetComponent<Image>();
            }
        }

        /// <summary>
        /// Populates the UI elements with the provided content and styles. Supports dual icons.
        /// </summary>
        public void SetText(string content, string title = "", Sprite icon = null, Color? titleColor = null, Color? iconColor = null, Sprite icon2 = null)
        {
            // Set Title (null-safe)
            bool hasTitle = !string.IsNullOrEmpty(title);
            if (titleField != null)
            {
                titleField.gameObject.SetActive(hasTitle);
                if (hasTitle)
                {
                    titleField.text = title;
                    titleField.color = titleColor ?? Color.white;
                }
            }

            // Set Content (null-safe)
            if (contentField != null)
            {
                contentField.text = content ?? string.Empty;
            }

            // Set Icon (null-safe)
            bool hasIcon = (icon != null);
            if (iconField != null)
            {
                iconField.gameObject.SetActive(hasIcon);
                if (hasIcon)
                {
                    iconField.sprite = icon;
                    iconField.color = iconColor ?? Color.white;
                }
            }

            // Set Icon2 (null-safe)
            bool hasIcon2 = (icon2 != null);
            if (iconField2 != null)
            {
                iconField2.gameObject.SetActive(hasIcon2);
                if (hasIcon2)
                {
                    iconField2.sprite = icon2;
                    iconField2.color = iconColor ?? Color.white;
                }
            }

            // Conditionally hide the entire header area
            if (header != null)
            {
                header.SetActive(hasTitle || hasIcon || hasIcon2);
            }
        }
    }
}