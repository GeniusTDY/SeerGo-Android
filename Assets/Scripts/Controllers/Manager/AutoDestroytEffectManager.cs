using UnityEngine;
/**
 * 监听动画完成自动销毁
 */
public class AutoDestroryEffect : MonoBehaviour
{
    private ParticleSystem particleSys; // 如果是粒子特效
    private Animator animator; // 如果是动画特效
    private bool isPlaying = true;

    void Start()
    {
        // 尝试获取粒子系统或Animator组件
        particleSys = GetComponent<ParticleSystem>();
        animator = GetComponent<Animator>();

        // 如果没有指定循环，可以开始检测
        if (particleSys != null && !particleSys.main.loop)
        {
            // 对于非循环粒子，可以开始检查
        }
        else if (animator != null)
        {
            // 对于动画，可以通过检查状态来判定结束
            StartCoroutine(CheckAnimationEnd());
        }
    }

    void Update()
    {
        // 方法：检查粒子系统是否播放完毕（适用于非循环粒子）
        if (particleSys != null && !particleSys.IsAlive(true) && isPlaying)
        {
            OnEffectFinished();
        }
    }

    // 方法：检查动画状态是否结束[9](@ref)
    private System.Collections.IEnumerator CheckAnimationEnd()
    {
        if (animator != null)
        {
            // 等待直到进入播放状态
            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0);
            // 等待动画播放完成（normalizedTime >= 1）
            yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);
            OnEffectFinished();
        }
    }

    void OnEffectFinished()
    {
        isPlaying = false;
        Destroy(gameObject);
    }
}