using SeerGoGame.Utilities;
using UnityEngine;

namespace SeerGoGame.Helper
{
    public static class ConfigHelper
    {
   public static T LoadJsonConfig<T>(string path) where T : class
{
    var textAsset = Resources.Load<TextAsset>(path);
    if (textAsset == null)
    {
        SeerGoLogger.LogError($"File not found: {path}");
        return null;
    }

    var json = textAsset.text;
    var result = JsonUtility.FromJson<T>(json);
    if (result == null)
    {
        SeerGoLogger.LogError($"Failed to parse JSON: {json}");
    }
    return result;
}

    }
}
