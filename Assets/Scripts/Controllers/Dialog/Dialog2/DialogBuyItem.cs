using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogBuyItem : Dialog2Base
{
    [Header("UI组件")]
    [SerializeField] private TextMeshProUGUI content;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_InputField itemCount;

    private const int MIN_VALUE = 0;
    private const int MAX_VALUE = 99;

    private void Start()
    {
        // 初始化时绑定输入校验事件
        if (itemCount != null)
        {
            // 先移除已有事件（防止重复绑定）
            itemCount.onValueChanged.RemoveListener(OnItemCountValueChanged);
            // 绑定新的校验事件
            itemCount.onValueChanged.AddListener(OnItemCountValueChanged);

            // 初始化输入框默认值
            if (string.IsNullOrEmpty(itemCount.text))
            {
                itemCount.text = MIN_VALUE.ToString();
            }
            else
            {
                // 初始化时校验一次已有值
                OnItemCountValueChanged(itemCount.text);
            }
        }
    }

    /// <summary>
    /// 初始化弹窗内容
    /// </summary>
    public void Initialize(string title, string message, int iconId, int initialCount)
    {
        // 设置物品图标
        if (itemIcon != null)
        {
            Sprite sprite = ResourcesManager.Instance.Load<Sprite>("Sprites/Item/" + iconId);
            if (sprite != null)
            {
                itemIcon.sprite = sprite;
            }
            else
            {
                Debug.LogWarning($"物品图标加载失败: Sprites/Item/{iconId}");
            }
        }

        // 设置初始数量
        if (itemCount != null)
        {
            int validCount = Mathf.Clamp(initialCount, MIN_VALUE, MAX_VALUE);
            itemCount.text = validCount.ToString();
        }

        // 设置消息内容
        if (content != null)
            content.text = message ?? "";

        isClosing = false;
    }

    /// <summary>
    /// 输入框值改变时的校验逻辑
    /// </summary>
    /// <param name="inputText">输入的文本内容</param>
    private void OnItemCountValueChanged(string inputText)
    {
        // 1. 过滤非数字字符，只保留数字
        string cleanText = System.Text.RegularExpressions.Regex.Replace(inputText, @"[^0-9]", "");

        // 2. 处理空文本或全0的情况
        if (string.IsNullOrEmpty(cleanText))
        {
            itemCount.text = MIN_VALUE.ToString();
            return;
        }

        // 3. 转换为整数并限制范围
        if (int.TryParse(cleanText, out int inputValue))
        {
            // 限制最小值
            if (inputValue < MIN_VALUE)
            {
                inputValue = MIN_VALUE;
            }
            // 限制最大值
            else if (inputValue > MAX_VALUE)
            {
                inputValue = MAX_VALUE;
            }

            // 4. 更新输入框文本（避免死循环，先移除监听再赋值）
            itemCount.onValueChanged.RemoveListener(OnItemCountValueChanged);
            itemCount.text = inputValue.ToString();
            itemCount.onValueChanged.AddListener(OnItemCountValueChanged);

            // 5. 保持光标在末尾（提升用户体验）
            itemCount.caretPosition = itemCount.text.Length;
        }
        else
        {
            // 解析失败时重置为最小值
            itemCount.text = MIN_VALUE.ToString();
        }
    }

    /// <summary>
    /// 外部获取当前有效的物品数量（供其他逻辑调用）
    /// </summary>
    /// <returns>0-99之间的整数</returns>
    public int GetValidItemCount()
    {
        if (itemCount == null || string.IsNullOrEmpty(itemCount.text))
            return MIN_VALUE;

        if (int.TryParse(itemCount.text, out int count))
        {
            // 双重校验，确保数值在范围内
            return Mathf.Clamp(count, MIN_VALUE, MAX_VALUE);
        }
        return MIN_VALUE;
    }
}