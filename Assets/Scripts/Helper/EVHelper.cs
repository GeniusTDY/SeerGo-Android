using SeerGoGame.Utilities;
using System.Collections.Generic;
using UnityEngine;

public static class EVHelper
{
    // 属性名到索引的映射字典
    private static readonly Dictionary<string, int> evIndexMap = new()
    {
        ["HP"] = 0,
        ["ATK"] = 1,
        ["DEF"] = 2,
        ["SPATK"] = 3,
        ["SPDEF"] = 4,
        ["SPD"] = 5
    };

    /// <summary>
    /// 根据属性名获取对应的努力值
    /// </summary>
    /// <param name="evArray">努力值数组 [HP, ATK, DEF, SPATK, SPDEF, SPD]</param>
    /// <param name="statName">属性名</param>
    /// <returns>对应属性的努力值，如果属性名无效返回0</returns>
    public static int GetEVByStat(int[] evArray, string statName)
    {
        if (evArray == null || evArray.Length < 6)
        {
            SeerGoLogger.LogError("EV数组格式不正确，长度必须至少为6");
            return 0;
        }
        // 5倍学习力
        if (evIndexMap.TryGetValue(statName, out int index))
        {
            return evArray[index] * 5;
        }

        SeerGoLogger.LogWarning($"未找到属性名: {statName}");
        return 0;
    }

    /// <summary>
    /// 根据属性名获取对应的索引位置
    /// </summary>
    /// <param name="statName">属性名</param>
    /// <returns>索引位置，如果属性名无效返回0</returns>
    public static int GetEVIndex(string statName)
    {
        if (evIndexMap.TryGetValue(statName, out int index))
        {
            return index;
        }

        return 0;
    }
}