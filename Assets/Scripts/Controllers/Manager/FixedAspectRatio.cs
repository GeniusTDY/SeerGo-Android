using UnityEngine;
/*
 * 挂载相机，安卓背景自适应
 */
public class FixedAspectRatio : MonoBehaviour
{
    // 例如：16:9 的宽高比
    public float targetAspectWidth = 16f;
    public float targetAspectHeight = 9f;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        // 计算目标宽高比
        float targetAspect = targetAspectWidth / targetAspectHeight;
        // 强制设置相机的宽高比
        mainCamera.aspect = targetAspect;
    }
}