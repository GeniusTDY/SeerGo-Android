using SeerGoGame.Utilities;
using System.Collections.Generic;
using UnityEngine;

public class PetPrefabConfigManager : MonoBehaviour
{
    private static PetPrefabConfigManager _instance;
    public static PetPrefabConfigManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("PetPrefabConfigManager");
                _instance = go.AddComponent<PetPrefabConfigManager>();
                //DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    [System.Serializable]
    public class PetPrefabConfig
    {
        public int id;
        public float scale;
        public Vector3 position;
        public Vector3 flipPosition;
    }

    private Dictionary<int, PetPrefabConfig> configDictionary = new Dictionary<int, PetPrefabConfig>();

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            // DontDestroyOnLoad(gameObject);
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
            string path = ResourcePathBuilder.GetPetsInfoDataPath()+ "/pet_prefab_config";
            // 从Resources文件夹加载CSV文件
            TextAsset configFile = Resources.Load<TextAsset>(path);

            if (configFile == null)
            {
                SeerGoLogger.LogError("在Resources文件夹中找不到prefab_config文件");
                return;
            }

            // 按行分割文本
            string[] lines = configFile.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

            // 跳过表头
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrEmpty(lines[i]))
                    continue;

                string[] values = lines[i].Split(',');

                if (values.Length >= 4)
                {
                    PetPrefabConfig config = new PetPrefabConfig
                    {
                        id = int.Parse(values[0]),
                        scale = float.Parse(values[1]),
                        position = new Vector3(float.Parse(values[2]), float.Parse(values[3]), 0),
                        flipPosition = new Vector3(float.Parse(values[4]), float.Parse(values[3]), 0)
                    };

                    configDictionary[config.id] = config;
                }
            }

            SeerGoLogger.Log($"成功加载 {configDictionary.Count} 个预制体配置");
        }
        catch (System.Exception e)
        {
            SeerGoLogger.LogError($"加载配置文件失败: {e.Message}");
        }
    }

    // 按ID查询配置
    public PetPrefabConfig GetConfigById(int id)
    {
        if (configDictionary.ContainsKey(id))
        {
            return configDictionary[id];
        }

        SeerGoLogger.LogWarning($"未找到ID为 {id} 的配置");
        return null;
    }

    // 检查配置是否存在
    public bool HasConfig(int id)
    {
        return configDictionary.ContainsKey(id);
    }

    // 获取所有配置数量
    public int GetConfigCount()
    {
        return configDictionary.Count;
    }
}