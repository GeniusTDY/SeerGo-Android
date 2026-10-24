using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace SeerGoGame.Utilities
{
    public class MapXmlParser : IMapParser
    {
        public MapData ParseMapXml(string xmlContent)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(MapData));
                using (StringReader reader = new StringReader(xmlContent))
                {
                    return serializer.Deserialize(reader) as MapData;
                }
            }
            catch (System.Exception e)
            {
                SeerGoLogger.LogError($"解析地图XML失败: {e.Message}");
                return null;
            }
        }

        public MapData LoadMapFromResources(string mapPath)
        {
            TextAsset xmlFile = null;
            try
            {
                xmlFile = Resources.Load<TextAsset>(mapPath);
                if (xmlFile == null) { return null; }
                return ParseMapXml(xmlFile.text);
            }
            catch (System.Exception ex)
            {
                SeerGoLogger.LogError($"处理地图文件时发生错误: {mapPath}. 异常: {ex.Message}");
                return null;
            }
            finally
            {
                if (xmlFile != null)
                {
                    Resources.UnloadAsset(xmlFile);
                }
            }
        }

        public Sprite LoadSpriteByResId(string resId)
        {
            // 从外部 Resources/ 目录加载NPC精灵Sprite
            string path = $"{ResourcePathBuilder.GetSpritesMapNpcPath()}/{resId}";
            return ExternalResourcesManager.Instance.LoadSprite(path);
        }

    }

}