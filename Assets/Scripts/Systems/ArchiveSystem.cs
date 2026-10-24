using QFramework;
using SeerGoGame.Events;
using SeerGoGame.Interfaces;
using SeerGoGame.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace SeerGoGame.Systems
{
    public class ArchiveSystem : AbstractSystem, IArchiveSystem
    {

        private IArchiveModel ArchiveModel;

        protected override void OnInit()
        {
            ArchiveModel = this.GetModel<IArchiveModel>();
        }

        private string GetSaveDirectory()
        {
#if UNITY_EDITOR
            return Path.Combine(Application.dataPath, "../Saves/");
#elif UNITY_ANDROID
        return Path.Combine(Application.persistentDataPath, "Saves/");
#else
      /*  string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        return Path.Combine(documentsPath, "MyGame/Saves/");*/
         return Path.Combine(Application.dataPath, "../Saves/");
#endif
        }

        public string GetSavePath(string saveName)
        {
            string directory = GetSaveDirectory();
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return Path.Combine(directory, $"{saveName}.xml");
        }

        public bool SaveGame(string saveName, ArchiveData data)
        {
            try
            {
                string filePath = GetSavePath(saveName);
                data.LastLoginDate = DateTime.Now;

                XmlSerializer serializer = new XmlSerializer(typeof(ArchiveData));
                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    serializer.Serialize(stream, data);
                }

                SeerGoLogger.Log($"存档成功: {filePath}");
                this.SendEvent(new ArchiveEvent { Type = "Save", Success = true, SaveName = saveName });
                return true;
            }
            catch (Exception e)
            {
                SeerGoLogger.LogError($"存档失败: {e.Message}");
                this.SendEvent(new ArchiveEvent { Type = "Save", Success = false, SaveName = saveName });
                return false;
            }
        }

        public ArchiveData LoadGame(string saveName)
        {
            try
            {
                string filePath = GetSavePath(saveName);
                if (!File.Exists(filePath))
                {
                    SeerGoLogger.LogWarning($"存档不存在: {saveName}");
                    return CreateNewSave();
                }

                XmlSerializer serializer = new XmlSerializer(typeof(ArchiveData));
                using (FileStream stream = new FileStream(filePath, FileMode.Open))
                {
                    ArchiveData data = (ArchiveData)serializer.Deserialize(stream);
                    SeerGoLogger.Log($"读档成功: {saveName}");
                   // data.Print();
                    ArchiveModel.CurrentData = data;
                    this.SendEvent(new ArchiveEvent { Type = "Load", Success = true, SaveName = saveName });
                    return data;
                }
            }
            catch (Exception e)
            {
                SeerGoLogger.LogError($"读档失败: {e.Message}");
                this.SendEvent(new ArchiveEvent { Type = "Load", Success = false, SaveName = saveName });
                return CreateNewSave();
            }
        }

        public string[] GetAllSaves()
        {
            string directory = GetSaveDirectory();
            if (!Directory.Exists(directory)) return new string[0];

            string[] files = Directory.GetFiles(directory, "*.xml");
            List<string> saveNames = new List<string>();

            foreach (string file in files)
            {
                saveNames.Add(Path.GetFileNameWithoutExtension(file));
            }

            return saveNames.ToArray();
        }

        public void DeleteSave(string saveName)
        {
            string filePath = GetSavePath(saveName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                SeerGoLogger.Log($"删除存档: {saveName}");
            }
        }

        private ArchiveData CreateNewSave()
        {
            return new ArchiveData
            {
                Nickname = "新玩家",
                LastLoginDate = DateTime.Now
            };
        }
    }
}