using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 单个残影实例的数据和逻辑
/// </summary>
public class AfterImage
{
    public GameObject GameObject { get; private set; }
    public bool IsExpired { get; private set; }

    private List<SpriteRenderer> afterImageRenderers;
    private float currentLifeTime;
    private float totalLifeTime;
    private float fadeStartTime;
    private Color initialColor;

    public AfterImage(GameObject gameObject)
    {
        GameObject = gameObject;
        afterImageRenderers = new List<SpriteRenderer>();
        IsExpired = false;
    }

    /// <summary>
    /// 初始化残影
    /// </summary>
    public void Initialize(SpriteRenderer[] sourceRenderers, Color color,
                         string sortingLayer, int sortingOrder,
                         float lifeTime, float fadeStartRatio)
    {
        this.totalLifeTime = lifeTime;
        this.currentLifeTime = 0f;
        this.fadeStartTime = lifeTime * fadeStartRatio;
        this.initialColor = color;
        this.IsExpired = false;

        // 设置残影对象的位置和旋转
        if (sourceRenderers.Length > 0)
        {
            GameObject.transform.position = sourceRenderers[0].transform.position;
            GameObject.transform.rotation = sourceRenderers[0].transform.rotation;
        }

        // 确保有足够数量的SpriteRenderer
        while (afterImageRenderers.Count < sourceRenderers.Length)
        {
            CreateNewSpriteRenderer();
        }

        // 禁用多余的SpriteRenderer
        for (int i = 0; i < afterImageRenderers.Count; i++)
        {
            afterImageRenderers[i].gameObject.SetActive(i < sourceRenderers.Length);
        }

        // 复制源SpriteRenderer的状态
        for (int i = 0; i < sourceRenderers.Length; i++)
        {
            if (i < afterImageRenderers.Count)
            {
                CopySpriteRendererProperties(sourceRenderers[i], afterImageRenderers[i],
                                           color, sortingLayer, sortingOrder);
            }
        }
    }

    /// <summary>
    /// 创建新的SpriteRenderer用于残影
    /// </summary>
    private void CreateNewSpriteRenderer()
    {
        GameObject spriteObj = new GameObject("AfterImageSprite");
        spriteObj.transform.SetParent(GameObject.transform);
        SpriteRenderer renderer = spriteObj.AddComponent<SpriteRenderer>();
        afterImageRenderers.Add(renderer);
    }

    /// <summary>
    /// 复制SpriteRenderer属性
    /// </summary>
    private void CopySpriteRendererProperties(SpriteRenderer source, SpriteRenderer target,
                                             Color color, string sortingLayer, int sortingOrder)
    {
        target.sprite = source.sprite;
        target.color = color;
        target.flipX = source.flipX;
        target.flipY = source.flipY;
        target.drawMode = source.drawMode;
        target.size = source.size;
        target.sortingLayerName = sortingLayer;
        target.sortingOrder = sortingOrder;

        // 复制变换信息
        target.transform.localPosition = source.transform.localPosition;
        target.transform.localRotation = source.transform.localRotation;
        target.transform.localScale = source.transform.localScale;

        // 复制材质（如果存在特殊材质）
        if (source.material != null)
        {
            target.material = new Material(source.material);
        }
    }

    /// <summary>
    /// 更新残影状态
    /// </summary>
    public void UpdateAfterImage(float deltaTime)
    {
        // 如果GameObject已被销毁，标记为过期
        if (this.GameObject == null)
        {
            this.IsExpired = true;
            return;
        }
        if (IsExpired) return;

        currentLifeTime += deltaTime;

        // 检查生命周期
        if (currentLifeTime >= totalLifeTime)
        {
            IsExpired = true;
            return;
        }

        // 更新透明度（淡出效果）
        UpdateAlpha();
    }

    /// <summary>
    /// 更新残影的透明度
    /// </summary>
    private void UpdateAlpha()
    {

        if (this.GameObject == null) return;
        float alpha = 1f;

        if (currentLifeTime >= fadeStartTime)
        {
            // 淡出阶段
            float fadeProgress = (currentLifeTime - fadeStartTime) / (totalLifeTime - fadeStartTime);
            alpha = 1f - fadeProgress;
        }

        // 应用新的透明度到所有SpriteRenderer
        Color newColor = initialColor;
        newColor.a = initialColor.a * alpha;

        foreach (SpriteRenderer renderer in afterImageRenderers)
        {
            if (renderer.gameObject.activeInHierarchy)
            {
                renderer.color = newColor;
            }
        }
    }
}