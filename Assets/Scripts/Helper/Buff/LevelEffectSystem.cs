using UnityEngine;
// 等级变化的战斗效果
public class LevelEffectSystem
{
    // 等级对应的实际效果系数
    public static float GetActualMultiplier(int level)
    {
        // 确保等级在有效范围内
        level = Mathf.Clamp(level, -6, 6);
        // 等级与倍率对应关系
        switch (level)
        {
            case -6: return 0.25f;   // 降低到1/4
            case -5: return 0.285f;  // 降低到2/7
            case -4: return 0.333f;  // 降低到1/3
            case -3: return 0.4f;    // 降低到2/5
            case -2: return 0.5f;    // 降低到1/2
            case -1: return 0.666f;  // 降低到2/3
            case 0: return 1.0f;    // 无变化
            case 1: return 1.5f;    // 提升到1.5倍
            case 2: return 2.0f;    // 提升到2倍
            case 3: return 2.5f;    // 提升到2.5倍
            case 4: return 3.0f;    // 提升到3倍
            case 5: return 3.5f;    // 提升到3.5倍
            case 6: return 4.0f;    // 提升到4倍
            default: return 1.0f;
        }
    }

    /// <summary>
    /// switch-case 形式获取命中等级倍率
    /// </summary>
    public static float GetAccuracyLevelMultiplier(int AccuracyLevel)
    {
        switch (AccuracyLevel)
        {
            case -6: return 0.25f; // -6级：基础命中率×25%
            case -5: return 0.35f; // -5级：基础命中率×35%
            case -4: return 0.45f; // -4级：基础命中率×45%
            case -3: return 0.55f; // -3级：基础命中率×55%
            case -2: return 0.70f; // -2级：基础命中率×70%
            case -1: return 0.85f; // -1级：基础命中率×85%
            case 0: return 1.0f;  // 0级：无变化
            case +1: return 1.5f;  // +1级：基础命中率×150%
            case +2: return 2.0f;  // +2级：基础命中率×200%
            case +3: return 2.5f;  // +3级：基础命中率×250%
            case +4: return 3.0f;  // +4级：基础命中率×300%
            case +5: return 3.5f;  // +5级：基础命中率×350%
            case +6: return 4.0f;  // +6级：基础命中率×400%
            default: return 1.0f;  
        }
    }
}