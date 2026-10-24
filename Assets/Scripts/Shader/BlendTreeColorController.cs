using SeerGoGame.Utilities;
using UnityEngine;

public class BlendTreeColorController : MonoBehaviour
{
    [Header("颜色设置")]
    public Color targetColor = Color.blue;
    [Range(0, 1)]
    public float colorTolerance = 0.3f;
    public Color originalColor = (Color)new Color32(0, 102, 51, 255);

    [Header("效果设置")]
    [Range(0, 2)]
    public float saturation = 1.0f;
    [Range(0, 2)]
    public float brightness = 1.0f;

    [Header("渐变效果设置")]
    public Color gradientColor1 = Color.red;
    public Color gradientColor2 = Color.blue;
    [Range(0, 3)]
    public int gradientType = 0; // 0:无渐变, 1:水平, 2:垂直, 3:径向
    [Range(0, 1)]
    public float gradientIntensity = 0.5f;
    [Range(0, 2)]
    public float gradientSpeed = 0.0f;

    private Material colorMaterial;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 创建颜色材质
        CreateColorMaterial();

        // 应用初始颜色设置
        ApplyColorSettings();
    }

    void CreateColorMaterial()
    {
        Shader colorShader = Shader.Find("Custom/BlendTreeColorReplace");
        if (colorShader != null)
        {
            colorMaterial = new Material(colorShader);

            // 应用材质到SpriteRenderer
            if (spriteRenderer != null)
            {
                spriteRenderer.material = colorMaterial;
            }
        }
        else
        {
            SeerGoLogger.LogError("找不到颜色替换Shader，使用默认材质");
        }
    }

    void ApplyColorSettings()
    {
        if (colorMaterial != null)
        {
            // 基础颜色设置
            colorMaterial.SetColor("_NewColor", targetColor);
            colorMaterial.SetColor("_OriginalColor", originalColor);
            colorMaterial.SetFloat("_ColorTolerance", colorTolerance);
            colorMaterial.SetFloat("_Saturation", saturation);
            colorMaterial.SetFloat("_Brightness", brightness);

            // 渐变效果设置
            colorMaterial.SetColor("_GradientColor1", gradientColor1);
            colorMaterial.SetColor("_GradientColor2", gradientColor2);
            colorMaterial.SetFloat("_GradientType", gradientType);
            colorMaterial.SetFloat("_GradientIntensity", gradientIntensity);
            colorMaterial.SetFloat("_GradientSpeed", gradientSpeed);
        }
    }

    // === 基础颜色控制方法 ===

    public void ChangeColor(Color newColor)
    {
        targetColor = newColor;
        ApplyColorSettings();
    }

    public void SetColorTolerance(float tolerance)
    {
        colorTolerance = tolerance;
        ApplyColorSettings();
    }

    public void SetSaturation(float sat)
    {
        saturation = sat;
        ApplyColorSettings();
    }

    public void SetBrightness(float bright)
    {
        brightness = bright;
        ApplyColorSettings();
    }

    // === 渐变效果控制方法 ===

    public void SetGradientColors(Color color1, Color color2)
    {
        gradientColor1 = color1;
        gradientColor2 = color2;
        ApplyColorSettings();
    }

    public void SetGradientType(int type)
    {
        gradientType = Mathf.Clamp(type, 0, 3);
        ApplyColorSettings();
    }

    public void SetGradientIntensity(float intensity)
    {
        gradientIntensity = Mathf.Clamp01(intensity);
        ApplyColorSettings();
    }

    public void SetGradientSpeed(float speed)
    {
        gradientSpeed = Mathf.Clamp(speed, 0, 2);
        ApplyColorSettings();
    }

    public void EnableGradient(bool enable)
    {
        gradientIntensity = enable ? 0.5f : 0f;
        ApplyColorSettings();
    }

    // === 预设渐变效果 ===

    public void SetGradientPreset(string presetName)
    {
        switch (presetName.ToLower())
        {
            case "rainbow":
                SetGradientColors(Color.red, Color.blue);
                SetGradientType(3); // 径向渐变
                SetGradientIntensity(0.7f);
                break;

            case "fire":
                SetGradientColors(Color.yellow, Color.red);
                SetGradientType(2); // 垂直渐变
                SetGradientIntensity(0.8f);
                break;

            case "water":
                SetGradientColors(Color.cyan, Color.blue);
                SetGradientType(1); // 水平渐变
                SetGradientIntensity(0.6f);
                break;

            case "none":
                SetGradientIntensity(0f);
                break;

            default:
                SeerGoLogger.LogWarning($"未知渐变预设: {presetName}");
                break;
        }
    }

    // === 动画渐变效果 ===

    public void StartPulseGradient(float pulseSpeed = 1f)
    {
        StartCoroutine(PulseGradientRoutine(pulseSpeed));
    }

    private System.Collections.IEnumerator PulseGradientRoutine(float speed)
    {
        float timer = 0f;
        while (true)
        {
            timer += Time.deltaTime * speed;
            float pulse = (Mathf.Sin(timer) + 1f) * 0.5f; // 0到1的脉冲
            SetGradientIntensity(pulse * 0.8f); // 最大强度0.8
            yield return null;
        }
    }

    public void StartColorCycle(float cycleSpeed = 1f)
    {
        StartCoroutine(ColorCycleRoutine(cycleSpeed));
    }

    private System.Collections.IEnumerator ColorCycleRoutine(float speed)
    {
        float hue = 0f;
        while (true)
        {
            hue += Time.deltaTime * speed;
            if (hue > 1f) hue -= 1f;

            Color cycleColor = Color.HSVToRGB(hue, 0.8f, 1f);
            SetGradientColors(cycleColor, Color.HSVToRGB(hue + 0.3f, 0.8f, 1f));

            yield return null;
        }
    }

    // === 快速设置预设颜色 ===

    public void SetPresetColor(string presetName)
    {
        switch (presetName.ToLower())
        {
            case "red":
                ChangeColor(Color.red);
                break;
            case "blue":
                ChangeColor(Color.blue);
                break;
            case "yellow":
                ChangeColor(Color.yellow);
                break;
            case "cyan":
                ChangeColor(Color.cyan);
                break;
            case "magenta":
                ChangeColor(Color.magenta);
                break;
            default:
                SeerGoLogger.LogWarning($"未知预设颜色: {presetName}");
                break;
        }
    }

    [ContextMenu("随机渐变效果")]
    public void RandomizeGradient()
    {
        gradientColor1 = new Color(Random.value, Random.value, Random.value, 1f);
        gradientColor2 = new Color(Random.value, Random.value, Random.value, 1f);
        gradientType = Random.Range(0, 4);
        gradientIntensity = Random.Range(0.3f, 0.8f);
        gradientSpeed = Random.Range(0f, 1f);
        ApplyColorSettings();
    }

    [ContextMenu("重置渐变设置")]
    public void ResetGradient()
    {
        gradientColor1 = Color.red;
        gradientColor2 = Color.blue;
        gradientType = 0;
        gradientIntensity = 0f;
        gradientSpeed = 0f;
        ApplyColorSettings();
    }

    // 在Inspector中修改时实时更新
    private void OnValidate()
    {
        if (Application.isPlaying && colorMaterial != null)
        {
            ApplyColorSettings();
        }
    }

    void OnDestroy()
    {
        // 清理材质
        if (colorMaterial != null)
        {
            DestroyImmediate(colorMaterial);
        }

        // 停止所有协程
        StopAllCoroutines();
    }
}

