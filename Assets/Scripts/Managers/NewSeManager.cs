using System.Collections.Generic;
using UnityEngine;
using SeerGoGame.Models;
using SeerGoGame.Utilities;

/// <summary>
/// 特性配置管理器 - 负责加载和访问特性配置
/// </summary>
public class NewSeManager
{
    private static NewSeManager _instance;
    public static NewSeManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new NewSeManager();
            }
            return _instance;
        }
    }

    private Dictionary<int, NewSeConfig> seConfigDict = new Dictionary<int, NewSeConfig>();
    private bool isLoaded = false;

    /// <summary>
    /// 加载特性配置
    /// </summary>
    public void LoadConfig()
    {
        if (isLoaded)
        {
            SeerGoLogger.Log("特性配置已加载，跳过重复加载");
            return;
        }

        SeerGoLogger.Log("[NewSeManager] 开始加载特性配置...");
        
        TextAsset configFile = Resources.Load<TextAsset>("Config/new_se");
        if (configFile == null)
        {
            SeerGoLogger.LogError("[NewSeManager] 特性配置文件 new_se.json 未找到，请检查 Resources/Config/new_se.json 是否存在");
            return;
        }
        
        SeerGoLogger.Log($"[NewSeManager] 配置文件加载成功，内容长度: {configFile.text.Length}");

        try
        {
            // 配置文件结构为 {"NewSe": {"NewSeIdx": [...]}}
            // 需要先解析外层包装获取NewSeIdx数组
            var wrapper = JsonUtility.FromJson<NewSeWrapper>(configFile.text);
            SeerGoLogger.Log($"[NewSeManager] JSON解析完成: wrapper={wrapper != null}, NewSe={wrapper?.NewSe != null}, NewSeIdx={wrapper?.NewSe?.NewSeIdx != null}");
            
            if (wrapper?.NewSe?.NewSeIdx != null && wrapper.NewSe.NewSeIdx.Length > 0)
            {
                seConfigDict.Clear();
                foreach (var config in wrapper.NewSe.NewSeIdx)
                {
                    if (config != null)
                        seConfigDict[config.Idx] = config;
                }
                isLoaded = true;
                SeerGoLogger.Log($"[NewSeManager] 成功加载 {seConfigDict.Count} 个特性配置");
            }
            else
            {
                SeerGoLogger.LogError($"[NewSeManager] 特性配置解析失败，NewSeIdx为空或为null");
                SeerGoLogger.LogError($"[NewSeManager] 原始JSON内容前200字符: {configFile.text.Substring(0, Mathf.Min(200, configFile.text.Length))}");
            }
        }
        catch (System.Exception e)
        {
            SeerGoLogger.LogError($"[NewSeManager] 加载特性配置失败: {e.Message}");
            SeerGoLogger.LogError($"[NewSeManager] StackTrace: {e.StackTrace}");
        }
    }

    /// <summary>
    /// 根据ID获取特性配置
    /// </summary>
    public NewSeConfig GetConfigById(int id)
    {
        if (!isLoaded)
        {
            LoadConfig();
        }

        if (seConfigDict.TryGetValue(id, out NewSeConfig config))
        {
            return config;
        }
        SeerGoLogger.LogWarning($"未找到ID为 {id} 的特性配置");
        return null;
    }

    /// <summary>
    /// 检查特性是否存在
    /// </summary>
    public bool HasConfig(int id)
    {
        if (!isLoaded)
        {
            LoadConfig();
        }
        return seConfigDict.ContainsKey(id);
    }

    /// <summary>
    /// 获取特性数量
    /// </summary>
    public int GetConfigCount()
    {
        if (!isLoaded)
        {
            LoadConfig();
        }
        return seConfigDict.Count;
    }

    /// <summary>
    /// 获取所有特性ID列表
    /// </summary>
    public List<int> GetAllSeIds()
    {
        if (!isLoaded)
        {
            LoadConfig();
        }
        return new List<int>(seConfigDict.Keys);
    }

    /// <summary>
    /// 清空配置（用于测试或重新加载）
    /// </summary>
    public void ClearConfig()
    {
        seConfigDict.Clear();
        isLoaded = false;
    }
}
