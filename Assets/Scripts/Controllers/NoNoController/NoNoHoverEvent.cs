using SeerGoGame.Utilities;
using UnityEngine;

public class NoNoHoverEvent : MonoBehaviour
{
    public SpriteRenderer[] spriteRenderers;

    // 用于缓存当前是否处于悬停状态，避免重复操作
    private bool isHovering = false;

    private void Start()
    {
        // 简单的空值检查，如果数组为空或未赋值，在控制台给出警告
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            SeerGoLogger.LogWarning("SpriteRenderers 数组未在 " + gameObject.name + " 上赋值。");
        }
        SetSpritesState(false);
    }

    private void OnMouseEnter()
    {
        if (isHovering) return; // 如果已经是悬停状态，则直接返回
        SetSpritesState(true);
        isHovering = true;
    }

    private void OnMouseExit()
    {
        if (!isHovering) return; // 如果已经不是悬停状态，则直接返回
        SetSpritesState(false);
        isHovering = false;
    }

    private void SetSpritesState(bool state)
    {
        if (spriteRenderers == null) return;

        foreach (SpriteRenderer sr in spriteRenderers)
        {
            if (sr != null)
            {
                sr.enabled = state;
            }
        }
    }

}