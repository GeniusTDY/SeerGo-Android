using UnityEngine;
using UnityEngine.EventSystems;

// 星球缩放动画
public class AutoScaleAnim : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    // 起始scale
    private Vector3 olocalScale;

    void Start()
    {
        olocalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = new Vector3(1.1f, 1.1f,1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = olocalScale;
    }
}
