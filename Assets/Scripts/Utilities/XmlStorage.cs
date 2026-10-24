using SeerGoGame.Interfaces;
using System.IO;
using System.Xml;
using UnityEngine;

namespace SeerGoGame.Utilities
{
    public class XmlStorage : IStorage
    {
        public void Save<T>(string key, T value)
        {
            string serializedValue = JsonUtility.ToJson(value);
            PlayerPrefs.SetString(key, serializedValue);
            PlayerPrefs.Save();
            //SeerGoLogger.Log($"保存数据: {key} = {serializedValue}");
        }

        public T Load<T>(string key)
        {
            if (PlayerPrefs.HasKey(key))
            {
                string serializedValue = PlayerPrefs.GetString(key);
                T value = JsonUtility.FromJson<T>(serializedValue);
               // SeerGoLogger.Log($"加载数据: {key} = {serializedValue}");
                return value;
            }
            return default(T);
        }

        public bool ContainsKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        public void Delete(string key)
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
           // SeerGoLogger.Log($"删除数据: {key}");
        }
    }
}