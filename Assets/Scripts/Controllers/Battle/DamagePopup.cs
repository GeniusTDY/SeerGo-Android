using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DamagePopup : MonoBehaviour
{
    // 技能伤害颜色
    public static readonly Color SkillDamageOutlineColor = new Color(254f / 255f, 253f / 255f, 1f / 255f);
    public static readonly Color SkillDamageInColor = new Color(153f / 255f, 0f, 0f);
    // Buff伤害颜色
    public static readonly Color BuffDamageOutlineColor = Color.white;
    public static readonly Color BuffDamageInColor = new Color(249 / 255f, 54 / 255f, 247 / 255f);
    // 恢复颜色
    public static readonly Color HealColor = new Color(0f, 204f / 255f, 0f);
    public static readonly Color HealOutlineColor = Color.white;

    [Header("玩家伤害文本")]
    [SerializeField] private TMP_Text playerChangeHpText;
    [SerializeField] private Color playerDefaultTextColor = new Color(249 / 255f, 54 / 255f, 247 / 255f);
    [SerializeField] private Color playerDefaultOutlineColor = Color.white;

    [Header("怪物伤害文本")]
    [SerializeField] private TMP_Text monsterChangeHpText;
    [SerializeField] private Color monsterDefaultTextColor = new Color(249 / 255f, 54 / 255f, 247 / 255f);
    [SerializeField] private Color monsterDefaultOutlineColor = Color.white;

    [Header("通用设置")]
    [SerializeField] private float outlineWidth = 0.15f;
    [SerializeField] private int fontSize = 55;
    [SerializeField] private float dilateValue = 0.2f;

    [Header("动画设置")]
    [SerializeField] private float animationDuration = 1.3f;
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.3f;
    [SerializeField] private float floatDistance = 20f;
    [SerializeField] private float pauseDuration = 0.5f;

    private Vector2 playerStartPosition;
    private Vector2 monsterStartPosition;
    private Coroutine playerAnimationCoroutine;
    private Coroutine monsterAnimationCoroutine;

    // 伤害队列系统
    private Queue<PlayerDamageInfo> playerDamageQueue = new Queue<PlayerDamageInfo>();
    private Queue<MonsterDamageInfo> monsterDamageQueue = new Queue<MonsterDamageInfo>();
    private bool isPlayerAnimating = false;
    private bool isMonsterAnimating = false;

    // 伤害信息结构
    private struct PlayerDamageInfo
    {
        public int damage;
        public Color textColor;
        public Color outlineColor;
        public string prefix;
    }

    private struct MonsterDamageInfo
    {
        public int damage;
        public Color textColor;
        public Color outlineColor;
        public string prefix;
    }

    private void Start()
    {
        // 记录初始位置
        if (playerChangeHpText != null)
        {
            playerStartPosition = playerChangeHpText.rectTransform.anchoredPosition;
            playerChangeHpText.gameObject.SetActive(false);
        }

        if (monsterChangeHpText != null)
        {
            monsterStartPosition = monsterChangeHpText.rectTransform.anchoredPosition;
            monsterChangeHpText.gameObject.SetActive(false);
        }
    }

    private void ApplyPlayerTextStyle(string damageText, Color textColor, Color outlineColor)
    {
        if (playerChangeHpText == null) return;

        playerChangeHpText.text = damageText;
        playerChangeHpText.color = new Color(textColor.r, textColor.g, textColor.b, 0);
        playerChangeHpText.outlineWidth = outlineWidth;
        playerChangeHpText.outlineColor = outlineColor;
        playerChangeHpText.fontSize = fontSize;
        playerChangeHpText.fontStyle = FontStyles.Bold;
        playerChangeHpText.alignment = TextAlignmentOptions.Center;

        playerChangeHpText.fontMaterial.SetFloat("_FaceDilate", dilateValue);
        playerChangeHpText.UpdateMeshPadding();
    }

    private void ApplyMonsterTextStyle(string damageText, Color textColor, Color outlineColor)
    {
        if (monsterChangeHpText == null) return;

        monsterChangeHpText.text = damageText;
        monsterChangeHpText.color = new Color(textColor.r, textColor.g, textColor.b, 0);
        monsterChangeHpText.outlineWidth = outlineWidth;
        monsterChangeHpText.outlineColor = outlineColor;
        monsterChangeHpText.fontSize = fontSize;
        monsterChangeHpText.fontStyle = FontStyles.Bold;
        monsterChangeHpText.alignment = TextAlignmentOptions.Center;

        monsterChangeHpText.fontMaterial.SetFloat("_FaceDilate", dilateValue);
        monsterChangeHpText.UpdateMeshPadding();
    }

    /// <summary>
    /// 播放玩家伤害动画
    /// </summary>
    public void PlayPlayerDamageAnimation()
    {
        if (playerChangeHpText == null) return;

        if (isPlayerAnimating)
        {
            // 如果正在播放动画，直接返回，等待队列处理
            return;
        }

        if (playerAnimationCoroutine != null)
        {
            StopCoroutine(playerAnimationCoroutine);
        }

        playerAnimationCoroutine = StartCoroutine(PlayerDamageAnimationRoutine());
    }

    /// <summary>
    /// 播放怪物伤害动画
    /// </summary>
    public void PlayMonsterDamageAnimation()
    {
        if (monsterChangeHpText == null) return;

        if (isMonsterAnimating)
        {
            // 如果正在播放动画，直接返回，等待队列处理
            return;
        }

        if (monsterAnimationCoroutine != null)
        {
            StopCoroutine(monsterAnimationCoroutine);
        }

        monsterAnimationCoroutine = StartCoroutine(MonsterDamageAnimationRoutine());
    }

    private IEnumerator PlayerDamageAnimationRoutine()
    {
        isPlayerAnimating = true;

        while (playerDamageQueue.Count > 0)
        {
            PlayerDamageInfo damageInfo = playerDamageQueue.Dequeue();
            string damageText = damageInfo.damage == -1 ? "MISS" : $"{damageInfo.prefix}{damageInfo.damage}";

            ApplyPlayerTextStyle(damageText, damageInfo.textColor, damageInfo.outlineColor);
            playerChangeHpText.rectTransform.anchoredPosition = playerStartPosition;
            playerChangeHpText.gameObject.SetActive(true);

            yield return StartCoroutine(DamageAnimationRoutine(playerChangeHpText, playerStartPosition));

            // 短暂间隔，避免连续播放太快
            if (playerDamageQueue.Count > 0)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }

        isPlayerAnimating = false;
    }

    private IEnumerator MonsterDamageAnimationRoutine()
    {
        isMonsterAnimating = true;

        while (monsterDamageQueue.Count > 0)
        {
            MonsterDamageInfo damageInfo = monsterDamageQueue.Dequeue();
            string damageText = damageInfo.damage == -1 ? "MISS" : $"{damageInfo.prefix}{damageInfo.damage}";

            ApplyMonsterTextStyle(damageText, damageInfo.textColor, damageInfo.outlineColor);
            monsterChangeHpText.rectTransform.anchoredPosition = monsterStartPosition;
            monsterChangeHpText.gameObject.SetActive(true);

            yield return StartCoroutine(DamageAnimationRoutine(monsterChangeHpText, monsterStartPosition));

            // 短暂间隔，避免连续播放太快
            if (monsterDamageQueue.Count > 0)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }

        isMonsterAnimating = false;
    }

    private IEnumerator DamageAnimationRoutine(TMP_Text textComponent, Vector2 startPosition)
    {
        float elapsedTime = 0f;
        Vector2 targetPosition = startPosition + (Vector2.up * floatDistance);

        // 淡入阶段
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeInDuration;

            Color currentColor = textComponent.color;
            currentColor.a = Mathf.Lerp(0f, 1f, t);
            textComponent.color = currentColor;

            float floatProgress = t * 0.3f;
            textComponent.rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, floatProgress);

            yield return null;
        }

        // 主要上浮阶段
        float floatStartTime = fadeInDuration;
        float floatEndTime = fadeInDuration + (animationDuration - fadeOutDuration - fadeInDuration - pauseDuration);

        while (elapsedTime < floatEndTime)
        {
            elapsedTime += Time.deltaTime;
            float t = (elapsedTime - floatStartTime) / (floatEndTime - floatStartTime);

            textComponent.rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, 0.3f + t * 0.7f);

            yield return null;
        }

        // 停顿阶段
        float pauseStartTime = floatEndTime;
        Vector2 pausePosition = textComponent.rectTransform.anchoredPosition;

        while (elapsedTime < pauseStartTime + pauseDuration)
        {
            elapsedTime += Time.deltaTime;
            textComponent.rectTransform.anchoredPosition = pausePosition;
            yield return null;
        }

        // 淡出阶段
        float fadeOutStartTime = pauseStartTime + pauseDuration;
        Vector2 fadeOutStartPosition = textComponent.rectTransform.anchoredPosition;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = (elapsedTime - fadeOutStartTime) / fadeOutDuration;

            Color currentColor = textComponent.color;
            currentColor.a = Mathf.Lerp(1f, 0f, t);
            textComponent.color = currentColor;

            textComponent.rectTransform.anchoredPosition = Vector2.Lerp(fadeOutStartPosition,
                fadeOutStartPosition + (Vector2.up * 20f), t);

            yield return null;
        }

        textComponent.gameObject.SetActive(false);
    }

    /// <summary>
    /// 设置玩家伤害
    /// </summary>
    public void SetPlayerDamage(int damage, Color? textColor = null, Color? outlineColor = null, string prefix = "-")
    {
        PlayerDamageInfo damageInfo = new PlayerDamageInfo
        {
            damage = damage,
            textColor = textColor ?? playerDefaultTextColor,
            outlineColor = outlineColor ?? playerDefaultOutlineColor,
            prefix = prefix
        };

        playerDamageQueue.Enqueue(damageInfo);
    }

    /// <summary>
    /// 设置怪物伤害
    /// </summary>
    public void SetMonsterDamage(int damage, Color? textColor = null, Color? outlineColor = null, string prefix = "-")
    {
        MonsterDamageInfo damageInfo = new MonsterDamageInfo
        {
            damage = damage,
            textColor = textColor ?? monsterDefaultTextColor,
            outlineColor = outlineColor ?? monsterDefaultOutlineColor,
            prefix = prefix
        };

        monsterDamageQueue.Enqueue(damageInfo);
    }

    /// <summary>
    /// 链式调用设置玩家伤害
    /// </summary>
    public DamagePopup SetupPlayer(int damage, Color? textColor = null, Color? outlineColor = null, string prefix = "-")
    {
        SetPlayerDamage(damage, textColor, outlineColor, prefix);
        return this;
    }

    /// <summary>
    /// 链式调用设置怪物伤害
    /// </summary>
    public DamagePopup SetupMonster(int damage, Color? textColor = null, Color? outlineColor = null, string prefix = "-")
    {
        SetMonsterDamage(damage, textColor, outlineColor, prefix);
        return this;
    }

    /// <summary>
    /// 播放玩家伤害动画
    /// </summary>
    public void PlayPlayer()
    {
        PlayPlayerDamageAnimation();
    }

    /// <summary>
    /// 播放怪物伤害动画
    /// </summary>
    public void PlayMonster()
    {
        PlayMonsterDamageAnimation();
    }

    /// <summary>
    /// 快捷方法：播放玩家伤害动画
    /// </summary>
    public void PlayPlayerDamage(int damage, Color? textColor = null, Color? outlineColor = null, string prefix = "-")
    {
        SetPlayerDamage(damage, textColor, outlineColor, prefix);
        PlayPlayerDamageAnimation();
    }

    /// <summary>
    /// 快捷方法：播放怪物伤害动画
    /// </summary>
    public void PlayMonsterDamage(int damage, Color? textColor = null, Color? outlineColor = null, string prefix = "-")
    {
        SetMonsterDamage(damage, textColor, outlineColor, prefix);
        PlayMonsterDamageAnimation();
    }
}