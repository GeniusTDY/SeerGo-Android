using SeerGoGame.Controller;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using SeerGoGame.Utilities;
using UnityEngine.UI;

public class GlobalClickManager : BaseController
{
    void Update()
    {
#if UNITY_EDITOR
        HandlePCClick();
#elif UNITY_ANDROID
        HandleAndroidTouch();
#else
        HandlePCClick();
#endif
    }

#if UNITY_EDITOR || UNITY_STANDALONE
    private void HandlePCClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PerformRaycast(Input.mousePosition);
        }
    }
#endif

#if UNITY_ANDROID
    private void HandleAndroidTouch()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            if (touch.phase == TouchPhase.Began)
            {
                PerformRaycast(touch.position);
            }
        }
    }
#endif

    private void PerformRaycast(Vector3 screenPosition)
    {
        // screenPosition是屏幕像素坐标，RaycastAll需要世界坐标
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

        // 先检测是否点击到了NPC或其他可交互对象
        RaycastHit2D[] hits = Physics2D.RaycastAll(worldPosition, Vector2.zero);

        bool hitInteractable = false;

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("battle")
                    || hit.collider.CompareTag("teleport")
                    || hit.collider.CompareTag("openDialog")
                    || hit.collider.CompareTag("openPanel"))
                {
                    NpcCilckController npcScript = hit.collider.GetComponent<NpcCilckController>();
                    if (npcScript != null)
                    {
                        // 检查是否被外部UI遮挡（排除自身UI）
                        if (!IsObjectBehindUI(screenPosition, hit.collider.gameObject))
                        {
                            npcScript.ActivateTrigger();
                            hitInteractable = true;
                        }
                    }
                }
            }
        }

        // 如果什么都没点到，或者点到的是UI后面的对象，就取消激活所有NPC
        if (hits.Length == 0 || !hitInteractable)
        {
            DeactivateAllNPCs();
        }
    }

    /// <summary>
    /// 检查对象是否被外部UI遮挡（排除点击对象自身的UI组件）
    /// </summary>
    private bool IsObjectBehindUI(Vector3 screenPosition, GameObject clickedObject)
    {
        if (EventSystem.current == null)
        {
            SeerGoLogger.Log("EventSystem.current 为 null！");
            return false;
        }

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = screenPosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        // 过滤掉不可见UI + 点击对象自身/子对象的UI
        int validHitCount = 0;
        foreach (RaycastResult result in results)
        {
            // 排除被点击对象自身及其子对象上的UI（自己不能挡自己）
            if (IsChildOf(result.gameObject, clickedObject))
            {
                SeerGoLogger.Log($"忽略(自身UI): {result.gameObject.name}");
                continue;
            }

            if (IsValidBlockingUI(result.gameObject))
            {
                validHitCount++;
                SeerGoLogger.Log($"有效阻挡UI: {result.gameObject.name}");
            }
            else
            {
                SeerGoLogger.Log($"忽略(不可见): {result.gameObject.name}");
            }
        }

        // SeerGoLogger.Log($"EventSystem总检测结果: {results.Count}, 有效阻挡: {validHitCount}");
        // SeerGoLogger.Log($"检测坐标: {screenPosition}");

        return validHitCount > 0;
    }

    /// <summary>
    /// 判断ui对象是否是target的自身或子对象
    /// </summary>
    private bool IsChildOf(GameObject uiObject, GameObject target)
    {
        if (uiObject == target) return true;
        return uiObject.transform.IsChildOf(target.transform);
    }

    /// <summary>
    /// 判断一个UI元素是否真正可见且应该阻挡点击
    /// </summary>
    private bool IsValidBlockingUI(GameObject uiObject)
    {
        // 1. 对象本身未激活
        if (!uiObject.activeInHierarchy)
            return false;

        // 2. 检查自身或父节点上的 CanvasGroup
        CanvasGroup canvasGroup = uiObject.GetComponentInParent<CanvasGroup>();
        if (canvasGroup != null)
        {
            // alpha为0 或 显式禁用了射线检测
            if (canvasGroup.alpha <= 0f || !canvasGroup.blocksRaycasts)
                return false;
        }

        // 3. 检查 Graphic 组件的颜色透明度（如完全透明的Image）
        Graphic graphic = uiObject.GetComponent<Graphic>();
        if (graphic != null)
        {
            // raycastTarget 被手动关闭
            if (!graphic.raycastTarget)
                return false;

            // 颜色完全透明（部分UI用透明Image做射线接收）
            if (graphic.color.a <= 0f)
                return false;
        }

        return true;
    }

    // 遍历场景中所有NPC，并调用取消激活方法
    private void DeactivateAllNPCs()
    {
        NpcCilckController[] allNpcs = FindObjectsOfType<NpcCilckController>();
        foreach (NpcCilckController npc in allNpcs)
        {
            npc.DeactivateTrigger();
        }
    }
}