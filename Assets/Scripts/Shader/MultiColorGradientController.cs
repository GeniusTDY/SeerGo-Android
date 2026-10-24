using SeerGoGame.Utilities;
using UnityEngine;

public class MultiColorGradientController : MonoBehaviour
{
    [Header("基础设置")]
    public Color originalColor = new Color(0f, 0.4f, 0.2f, 1f); // 您图片中的绿色
    [Range(0, 1)]
    public float colorTolerance = 0.3f;

    [Header("多色渐变设置")]
    public Color gradientColor1 = Color.red;
    public Color gradientColor2 = Color.green;
    public Color gradientColor3 = Color.blue;
    public bool useThreeColors = true;
    [Range(0, 3)]
    public int gradientType = 3; // 推荐径向渐变适合圆形
    [Range(0, 1)]
    public float gradientIntensity = 0.7f;
    [Range(0, 1)]
    public float colorBlend1 = 0.3f;
    [Range(0, 1)]
    public float colorBlend2 = 0.7f;

    private Material gradientMaterial;
    private SpriteRenderer spriteRenderer;
    private Sprite lastSprite;  // 追踪sprite变化，用于图集帧切换时更新UV变换

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        CreateGradientMaterial();
        ApplyGradientSettings();
    }

    void CreateGradientMaterial()
    {
        Shader gradientShader = Shader.Find("Custom/MultiColorGradient");
        if (gradientShader != null)
        {
            gradientMaterial = new Material(gradientShader);
            if (spriteRenderer != null)
            {
                spriteRenderer.material = gradientMaterial;
            }
        }
        else
        {
            SeerGoLogger.LogError("找不到多色渐变Shader");
        }
    }

    void ApplyGradientSettings()
    {
        if (gradientMaterial != null)
        {
            gradientMaterial.SetColor("_OriginalColor", originalColor);
            gradientMaterial.SetFloat("_ColorTolerance", colorTolerance);

            gradientMaterial.SetColor("_GradientColor1", gradientColor1);
            gradientMaterial.SetColor("_GradientColor2", gradientColor2);
            gradientMaterial.SetColor("_GradientColor3", gradientColor3);
            gradientMaterial.SetFloat("_UseThreeColors", useThreeColors ? 1f : 0f);
            gradientMaterial.SetFloat("_GradientType", gradientType);
            gradientMaterial.SetFloat("_GradientIntensity", gradientIntensity);
            gradientMaterial.SetFloat("_ColorBlend1", colorBlend1);
            gradientMaterial.SetFloat("_ColorBlend2", colorBlend2);

            // 计算图集UV→sprite局部UV的反变换，确保渐变在所有帧间一致
            UpdateGradientUVTransform();
        }
    }

    void UpdateGradientUVTransform()
    {
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            Rect rect = spriteRenderer.sprite.rect;
            Texture2D tex = spriteRenderer.sprite.texture;
            if (tex != null)
            {
                float texW = tex.width;
                float texH = tex.height;
                // 从图集UV反算sprite自身的0~1局部UV
                // localUV = atlasUV * scale + offset
                float scaleX = texW / rect.width;
                float scaleY = texH / rect.height;
                float offsetX = -rect.x / rect.width;
                float offsetY = -rect.y / rect.height;
                gradientMaterial.SetVector("_GradientUVTransform",
                    new Vector4(scaleX, scaleY, offsetX, offsetY));
            }
        }
    }

    // === 渐变预设方法 ===

    [ContextMenu("应用彩虹渐变")]
    public void ApplyRainbowGradient()
    {
        useThreeColors = true;
        gradientColor1 = Color.red;
        gradientColor2 = Color.yellow;
        gradientColor3 = Color.blue;
        gradientType = 3; // 径向渐变
        gradientIntensity = 0.8f;
        ApplyGradientSettings();
    }

    [ContextMenu("应用火焰渐变")]
    public void ApplyFireGradient()
    {
        useThreeColors = true;
        gradientColor1 = Color.yellow;
        gradientColor2 = Color.red;
        gradientColor3 = new Color(0.5f, 0f, 0f, 1f); // 深红色
        gradientType = 2; // 垂直渐变
        gradientIntensity = 0.9f;
        ApplyGradientSettings();
    }

    [ContextMenu("应用海洋渐变")]
    public void ApplyOceanGradient()
    {
        useThreeColors = true;
        gradientColor1 = new Color(0.2f, 0.6f, 1f, 1f); // 浅蓝
        gradientColor2 = new Color(0f, 0.3f, 0.8f, 1f); // 中蓝
        gradientColor3 = new Color(0f, 0.1f, 0.4f, 1f); // 深蓝
        gradientType = 3; // 径向渐变
        gradientIntensity = 0.7f;
        ApplyGradientSettings();
    }

    [ContextMenu("应用森林渐变")]
    public void ApplyForestGradient()
    {
        useThreeColors = true;
        gradientColor1 = new Color(0.8f, 1f, 0.8f, 1f); // 浅绿
        gradientColor2 = new Color(0.2f, 0.8f, 0.2f, 1f); // 中绿
        gradientColor3 = new Color(0f, 0.4f, 0f, 1f); // 深绿
        gradientType = 3; // 径向渐变
        gradientIntensity = 0.6f;
        ApplyGradientSettings();
    }

    [ContextMenu("应用日落渐变")]
    public void ApplySunsetGradient()
    {
        useThreeColors = true;
        gradientColor1 = new Color(1f, 0.8f, 0.4f, 1f); // 橙黄
        gradientColor2 = new Color(1f, 0.4f, 0.2f, 1f); // 橙红
        gradientColor3 = new Color(0.6f, 0.2f, 0.4f, 1f); // 紫红
        gradientType = 2; // 垂直渐变
        gradientIntensity = 0.8f;
        ApplyGradientSettings();
    }

    [ContextMenu("切换双色/三色模式")]
    public void ToggleColorMode()
    {
        useThreeColors = !useThreeColors;
        ApplyGradientSettings();
    }

    [ContextMenu("随机渐变效果")]
    public void RandomizeGradient()
    {
        gradientColor1 = new Color(Random.value, Random.value, Random.value, 1f);
        gradientColor2 = new Color(Random.value, Random.value, Random.value, 1f);
        gradientColor3 = new Color(Random.value, Random.value, Random.value, 1f);
        gradientType = Random.Range(1, 4);
        gradientIntensity = Random.Range(0.5f, 0.9f);
        colorBlend1 = Random.Range(0.2f, 0.5f);
        colorBlend2 = Random.Range(0.5f, 0.8f);
        ApplyGradientSettings();
    }

    // === 实时控制方法 ===

    public void SetGradientColors(Color color1, Color color2, Color color3 = default)
    {
        gradientColor1 = color1;
        gradientColor2 = color2;
        if (color3 != default) gradientColor3 = color3;
        ApplyGradientSettings();
    }

    public void SetGradientType(int type)
    {
        gradientType = Mathf.Clamp(type, 0, 3);
        ApplyGradientSettings();
    }

    private void OnValidate()
    {
        if (Application.isPlaying && gradientMaterial != null)
        {
            ApplyGradientSettings();
        }
    }

    void LateUpdate()
    {
        // 动画切换sprite帧时，图集位置可能不同，实时更新UV反变换
        if (spriteRenderer != null && gradientMaterial != null)
        {
            Sprite currentSprite = spriteRenderer.sprite;
            if (currentSprite != lastSprite)
            {
                lastSprite = currentSprite;
                UpdateGradientUVTransform();
            }
        }
    }

    void OnDestroy()
    {
        if (gradientMaterial != null)
        {
            DestroyImmediate(gradientMaterial);
        }
    }
}