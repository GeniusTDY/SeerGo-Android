using UnityEngine;

public class RobotColorController : MonoBehaviour
{
    private Material robotMaterial;
    private SpriteRenderer spriteRenderer;

    // 预设颜色方案
    [System.Serializable]
    public class ColorScheme
    {
        public string name;
        public Color bodyColor = Color.white;
        public Color eyeColor = Color.yellow;
        public float bodyIntensity = 1f;
        public float eyeIntensity = 2f;
        public float eyeThreshold = 0.8f;
    }

    [Header("颜色方案预设")]
    public ColorScheme[] colorSchemes;

    [Header("当前设置")]
    public Color currentBodyColor = new Color(1f, 0.8f, 0.2f);
    public Color currentEyeColor = new Color(1f, 1f, 0.3f);
    public float currentBodyIntensity = 1f;
    public float currentEyeIntensity = 2f;
    public float currentEyeThreshold = 0.8f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // 创建材质实例以避免影响其他对象
            robotMaterial = new Material(spriteRenderer.material);
            spriteRenderer.material = robotMaterial;
            ApplyCurrentColors();

        }
    }

    // 应用当前颜色设置
    public void ApplyCurrentColors()
    {
        if (robotMaterial != null)
        {
            robotMaterial.SetColor("_BodyColor", currentBodyColor);
            robotMaterial.SetColor("_EyeColor", currentEyeColor);
            robotMaterial.SetFloat("_BodyIntensity", currentBodyIntensity);
            robotMaterial.SetFloat("_EyeIntensity", currentEyeIntensity);
            robotMaterial.SetFloat("_EyeThreshold", currentEyeThreshold);
        }
    }

    // 动态设置身体颜色
    public void SetBodyColor(Color newColor)
    {
        currentBodyColor = newColor;
        ApplyCurrentColors();
    }

    // 动态设置眼睛颜色
    public void SetEyeColor(Color newColor)
    {
        currentEyeColor = newColor;
        ApplyCurrentColors();
    }

    // 设置颜色强度
    public void SetIntensities(float bodyIntensity, float eyeIntensity)
    {
        currentBodyIntensity = bodyIntensity;
        currentEyeIntensity = eyeIntensity;
        ApplyCurrentColors();
    }

    // 应用预设方案
    public void ApplyColorScheme(int schemeIndex)
    {
        if (schemeIndex >= 0 && schemeIndex < colorSchemes.Length)
        {
            ColorScheme scheme = colorSchemes[schemeIndex];
            currentBodyColor = scheme.bodyColor;
            currentEyeColor = scheme.eyeColor;
            currentBodyIntensity = scheme.bodyIntensity;
            currentEyeIntensity = scheme.eyeIntensity;
            currentEyeThreshold = scheme.eyeThreshold;
            ApplyCurrentColors();
        }
    }

    // 渐变过渡到新颜色
    public void TransitionToColor(Color newBodyColor, Color newEyeColor, float duration)
    {
        StartCoroutine(ColorTransitionCoroutine(newBodyColor, newEyeColor, duration));
    }

    private System.Collections.IEnumerator ColorTransitionCoroutine(Color targetBodyColor, Color targetEyeColor, float duration)
    {
        Color startBodyColor = currentBodyColor;
        Color startEyeColor = currentEyeColor;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            currentBodyColor = Color.Lerp(startBodyColor, targetBodyColor, t);
            currentEyeColor = Color.Lerp(startEyeColor, targetEyeColor, t);
            ApplyCurrentColors();

            yield return null;
        }

        // 确保最终颜色准确
        currentBodyColor = targetBodyColor;
        currentEyeColor = targetEyeColor;
        ApplyCurrentColors();
    }

    // 示例：常用颜色预设
    [ContextMenu("设置为金色")]
    public void SetToGold()
    {
        SetBodyColor(new Color(1f, 0.8f, 0.2f));
        SetEyeColor(new Color(1f, 1f, 0.3f));
        SetIntensities(1f, 2f);
    }

    [ContextMenu("设置为银色")]
    public void SetToSilver()
    {
        SetBodyColor(new Color(0.9f, 0.9f, 0.95f));
        SetEyeColor(new Color(0.2f, 0.8f, 1f));
        SetIntensities(0.8f, 2.5f);
    }

    [ContextMenu("设置为铜色")]
    public void SetToCopper()
    {
        SetBodyColor(new Color(0.9f, 0.6f, 0.4f));
        SetEyeColor(new Color(1f, 0.3f, 0.1f));
        SetIntensities(1.2f, 2.2f);
    }

    [ContextMenu("设置为冰蓝色")]
    public void SetToIceBlue()
    {
        SetBodyColor(new Color(0.6f, 0.8f, 1f));
        SetEyeColor(new Color(0.2f, 0.9f, 1f));
        SetIntensities(0.9f, 2.8f);
    }
}