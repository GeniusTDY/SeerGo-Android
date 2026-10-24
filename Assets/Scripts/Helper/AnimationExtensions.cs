using UnityEngine;
using System.Collections;

public class OneShotAnim : MonoBehaviour
{
    public void Play(float callbackAtTime, System.Action callback = null)
    {
        gameObject.SetActive(true);
        StartCoroutine(PlayAnimation(callbackAtTime, callback));
    }

    IEnumerator PlayAnimation(float callbackAtTime, System.Action callback)
    {
        yield return null; // 等待一帧

        var anim = GetComponent<Animator>();
        if (anim == null) yield break;

        // 应用全局战斗倍速
        float speed = BattleEntityController.BattleAnimationSpeed;
        anim.speed = speed;

        // 播放动画
        anim.Play(0, 0, 0f);

        // 获取动画总时长
        float totalLength = anim.runtimeAnimatorController.animationClips[0].length;

        // 确保回调时间在合理范围内
        float actualCallbackTime = Mathf.Clamp(callbackAtTime, 0, totalLength);

        // 等待到回调时间点（除以倍速）
        if (actualCallbackTime > 0) yield return new WaitForSeconds(actualCallbackTime / speed);

        // 执行回调
        callback?.Invoke();

        // 等待剩余的动画时间（除以倍速）
        float remainingTime = totalLength - actualCallbackTime;
        if (remainingTime > 0) yield return new WaitForSeconds(remainingTime / speed);

        // 隐藏对象
        gameObject.SetActive(false);
    }
}