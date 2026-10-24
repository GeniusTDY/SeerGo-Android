using SeerGoGame.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Helper
{
    // 定义一个静态类来充当全局容器
    public static class GameObjectDictionary
    {
        // 核心字典，用于存储关联的GameObject
        public static readonly Dictionary<int, GameObject> Dictionary = new Dictionary<int, GameObject>();

        // 提供安全的注册方法，避免键冲突
        public static bool RegisterGameObject(int id, GameObject gameObject)
        {
            if (Dictionary.ContainsKey(id))
            {
                SeerGoLogger.LogWarning($"字典中已存在ID为 {id} 的GameObject，注册失败。");
                return false;
            }
            Dictionary.Add(id, gameObject);
            return true;
        }

        // 提供安全的获取方法，避免键不存在时报错
        public static GameObject GetGameObject(int id)
        {
            Dictionary.TryGetValue(id, out GameObject result);
            return result; // 如果id不存在，则返回null
        }

        // 从字典中移除并清理引用
        public static void UnregisterGameObject(int id)
        {
            if (Dictionary.ContainsKey(id))
            {
                Dictionary.Remove(id);
            }
        }

        // 在游戏退出或需要重置时清空字典
        public static void ClearDictionary()
        {
            Dictionary.Clear();
        }
    }
}
