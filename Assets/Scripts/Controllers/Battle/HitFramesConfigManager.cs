using SeerGoGame.Utilities;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 打击帧配置管理器 - 从 hit_frames.csv 加载新精灵的打击帧数据
/// CSV结构: skinId, name, physics, attribute, special, critical, fit
/// 映射关系: physics→attack, attribute→cp, special→sa
/// </summary>
public class HitFramesConfigManager : MonoBehaviour
{
    private static HitFramesConfigManager _instance;
    public static HitFramesConfigManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("HitFramesConfigManager");
                _instance = go.AddComponent<HitFramesConfigManager>();
            }
            return _instance;
        }
    }

    /// <summary>
    /// 打击帧数据结构: 外层key = petId, 内层key = 动画类型("attack"/"cp"/"sa")
    /// </summary>
    private Dictionary<int, Dictionary<string, int>> hitFramesDict = new Dictionary<int, Dictionary<string, int>>();

    // CSV列索引
    private const int COL_SKIN_ID = 0;
    private const int COL_PHYSICS = 2;   // → attack
    private const int COL_ATTRIBUTE = 3; // → cp
    private const int COL_SPECIAL = 4;   // → sa

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            LoadConfig();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadConfig()
    {
        try
        {
            string path = ResourcePathBuilder.GetPetsInfoDataPath() + "/hit_frames";
            TextAsset configFile = Resources.Load<TextAsset>(path);

            if (configFile == null)
            {
                SeerGoLogger.LogWarning("[HitFramesConfigManager] 未找到 hit_frames 配置文件");
                return;
            }

            string[] lines = configFile.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

            // 跳过表头
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrEmpty(lines[i])) continue;

                string[] values = lines[i].Split(',');

                if (values.Length >= 5)
                {
                    if (!int.TryParse(values[COL_SKIN_ID], out int petId)) continue;

                    var frameDict = new Dictionary<string, int>();

                    // physics → attack
                    if (int.TryParse(values[COL_PHYSICS], out int physicsFrame) && physicsFrame > 0)
                        frameDict["attack"] = physicsFrame;

                    // attribute → cp
                    if (int.TryParse(values[COL_ATTRIBUTE], out int attributeFrame) && attributeFrame > 0)
                        frameDict["cp"] = attributeFrame;

                    // special → sa
                    if (int.TryParse(values[COL_SPECIAL], out int specialFrame) && specialFrame > 0)
                        frameDict["sa"] = specialFrame;

                    if (frameDict.Count > 0)
                        hitFramesDict[petId] = frameDict;
                }
            }

            SeerGoLogger.Log($"[HitFramesConfigManager] 成功加载 {hitFramesDict.Count} 个精灵的打击帧配置");
        }
        catch (System.Exception e)
        {
            SeerGoLogger.LogError($"[HitFramesConfigManager] 加载配置文件失败: {e.Message}");
        }
    }

    /// <summary>
    /// 获取指定精灵和动画类型的打击帧号
    /// </summary>
    /// <param name="petId">精灵ID</param>
    /// <param name="animationSuffix">动画类型: "attack"/"cp"/"sa"</param>
    /// <returns>帧号，未找到返回0</returns>
    public int GetHitFrame(int petId, string animationSuffix)
    {
        if (hitFramesDict.TryGetValue(petId, out var frameDict))
        {
            if (frameDict.TryGetValue(animationSuffix, out int frame))
                return frame;
        }
        return 0;
    }

    /// <summary>
    /// 检查指定精灵是否有打击帧配置
    /// </summary>
    public bool HasConfig(int petId)
    {
        return hitFramesDict.ContainsKey(petId);
    }

    /// <summary>
    /// 获取所有打击帧配置数量
    /// </summary>
    public int GetConfigCount()
    {
        return hitFramesDict.Count;
    }
}
