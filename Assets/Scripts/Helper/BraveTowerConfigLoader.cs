using SeerGoGame.Models;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// 勇者之塔配置加载器
/// 从 Resources/Config/brave_tower.json 读取并解析配置
/// </summary>
public static class BraveTowerConfigLoader
{
    private const string ConfigPath = "Config/brave_tower";

    /// <summary>
    /// 同步加载勇者之塔配置
    /// </summary>
    public static BraveTowerConfig LoadConfig()
    {
        var result = new BraveTowerConfig();

        var textAsset = Resources.Load<TextAsset>(ConfigPath);
        if (textAsset == null)
        {
            SeerGoLogger.LogError($"[勇者之塔配置] 找不到配置文件: Resources/{ConfigPath}.json");
            return result;
        }

        var json = textAsset.text;

        // 1. 解析floors数组
        ParseFloors(json, result);

        // 2. 解析rewards对象
        ParseRewards(json, result);

        SeerGoLogger.Log($"[勇者之塔配置] 加载完成: {result}");

        Resources.UnloadAsset(textAsset);
        return result;
    }

    /// <summary>
    /// 使用JsonUtility解析floors数组
    /// </summary>
    private static void ParseFloors(string json, BraveTowerConfig result)
    {
        try
        {
            // JsonUtility会忽略JSON中多余的"rewards"字段
            var container = JsonUtility.FromJson<BraveTowerFloorsContainer>(json);
            if (container != null && container.floors != null)
            {
                foreach (var floor in container.floors)
                {
                    result.Floors.Add(floor);
                }
                SeerGoLogger.Log($"[勇者之塔配置] 解析楼层: {result.Floors.Count}层");
            }
        }
        catch (System.Exception e)
        {
            SeerGoLogger.LogError($"[勇者之塔配置] 解析floors失败: {e.Message}");
        }
    }

    /// <summary>
    /// 解析rewards对象（JSON map格式 → 数组 → Dictionary）
    /// </summary>
    private static void ParseRewards(string json, BraveTowerConfig result)
    {
        try
        {
            // 提取 "rewards" 对象内容
            // JSON: "rewards": { "1": {...}, "2": {...} }
            var rewardsArrayJson = ConvertRewardsObjectToArray(json);
            if (string.IsNullOrEmpty(rewardsArrayJson))
            {
                SeerGoLogger.LogWarning("[勇者之塔配置] 未找到rewards数据");
                return;
            }

            var container = JsonUtility.FromJson<BraveTowerRewardArrayContainer>(rewardsArrayJson);
            if (container != null && container.entries != null)
            {
                foreach (var entry in container.entries)
                {
                    result.FloorRewards[entry.floor] = entry;
                }
                SeerGoLogger.Log($"[勇者之塔配置] 解析奖励: {result.FloorRewards.Count}层");
            }
        }
        catch (System.Exception e)
        {
            SeerGoLogger.LogError($"[勇者之塔配置] 解析rewards失败: {e.Message}");
        }
    }

    /// <summary>
    /// 将JSON对象格式的rewards转换为数组格式，兼容JsonUtility
    /// 输入: "rewards": { "1": {...}, "2": {...} }
    /// 输出: { "entries": [ {"floor":1,...}, {"floor":2,...} ] }
    /// </summary>
    private static string ConvertRewardsObjectToArray(string json)
    {
        // 使用正则匹配 "rewards": 后的整个对象
        var rewardsMatch = Regex.Match(json, @"""rewards""\s*:\s*\{");
        if (!rewardsMatch.Success)
            return null;

        int startIndex = rewardsMatch.Index + rewardsMatch.Length - 1; // 指向 '{'
        int braceCount = 0;
        int endIndex = -1;

        for (int i = startIndex; i < json.Length; i++)
        {
            if (json[i] == '{') braceCount++;
            else if (json[i] == '}')
            {
                braceCount--;
                if (braceCount == 0)
                {
                    endIndex = i;
                    break;
                }
            }
        }

        if (endIndex < 0)
            return null;

        // 提取rewards对象内部的键值对
        string rewardsContent = json.Substring(startIndex + 1, endIndex - startIndex - 1);

        // 逐个解析 "N": {...} 条目（用括号计数处理嵌套对象）
        var entries = new List<string>();
        int pos = 0;
        while (pos < rewardsContent.Length)
        {
            // 匹配楼层号 "N":
            var keyMatch = Regex.Match(rewardsContent.Substring(pos), @"""(\d+)""\s*:\s*");
            if (!keyMatch.Success)
                break;

            string floorNum = keyMatch.Groups[1].Value;
            // 定位到 '{' 的位置
            int entryStart = pos + keyMatch.Index + keyMatch.Length;

            // 跳过空白找到第一个 '{'
            while (entryStart < rewardsContent.Length && (rewardsContent[entryStart] == ' ' || rewardsContent[entryStart] == '\r' || rewardsContent[entryStart] == '\n' || rewardsContent[entryStart] == '\t'))
                entryStart++;

            if (entryStart >= rewardsContent.Length || rewardsContent[entryStart] != '{')
                break;

            // 括号计数找到匹配的 '}'
            int nestedBrace = 0;
            int entryEnd = -1;
            for (int j = entryStart; j < rewardsContent.Length; j++)
            {
                if (rewardsContent[j] == '{') nestedBrace++;
                else if (rewardsContent[j] == '}')
                {
                    nestedBrace--;
                    if (nestedBrace == 0)
                    {
                        entryEnd = j;
                        break;
                    }
                }
            }

            if (entryEnd < 0)
                break;

            string entryJson = rewardsContent.Substring(entryStart, entryEnd - entryStart + 1);
            // 在对象开头插入 "floor" 字段
            var modifiedEntry = "{\"floor\":" + floorNum + "," + entryJson.Substring(1);
            entries.Add(modifiedEntry);

            pos = entryEnd + 1;
        }

        if (entries.Count == 0)
            return null;

        var sb = new StringBuilder();
        sb.Append("{\"entries\":[");
        sb.Append(string.Join(",", entries));
        sb.Append("]}");

        return sb.ToString();
    }
}
