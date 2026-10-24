using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace SeerGoGame.Utilities
{

    public class WorldMapXmlParser : IWorldMapXmlParser
    {
        public List<GalaxyData> ParseGalaxiesFromXml(string xmlContent)
        {
            List<GalaxyData> galaxies = new List<GalaxyData>();
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlContent);

                XmlNodeList galaxyNodes = xmlDoc.SelectNodes("//Galaxies/Galaxy");

                if (galaxyNodes == null || galaxyNodes.Count == 0)
                {
                    SeerGoLogger.LogError("未找到Galaxy节点！");
                    return galaxies;
                }

                foreach (XmlElement galaxyElement in galaxyNodes)
                {
                    GalaxyData galaxy = ParseGalaxyElement(galaxyElement);
                    galaxies.Add(galaxy);
                }

                //SeerGoLogger.Log($"成功解析 {galaxies.Count} 个星系数据");
            }
            catch (Exception e)
            {
                SeerGoLogger.LogError($"解析XML时出错: {e.Message}");
            }

            return galaxies;
        }

        public List<GalaxyData> ParseGalaxiesFromFile(string filePath)
        {
            TextAsset xmlFile = Resources.Load<TextAsset>(filePath);
            if (xmlFile == null)
            {
                SeerGoLogger.LogError($"找不到地图文件: {filePath}");
                return null;
            }

            //SeerGoLogger.Log($"找到世界地图文件: {filePath}, 文件大小: {xmlFile.bytes.Length} 字节");
            return ParseGalaxiesFromXml(xmlFile.text);
        }

        public GalaxyData ParseGalaxyElement(XmlElement galaxyElement)
        {
            GalaxyData galaxy = new GalaxyData();

            // 解析星系属性
            galaxy.Id = int.Parse(galaxyElement.GetAttribute("id"));
            galaxy.Name = galaxyElement.GetAttribute("name");
            galaxy.ResId = galaxyElement.GetAttribute("resId");
            galaxy.Size = ParseUtils.ParseVector2(galaxyElement.GetAttribute("size"));
            galaxy.Position = ParseUtils.ParseVector2(galaxyElement.GetAttribute("pos"));
            galaxy.Color = ParseUtils.ParseColor(galaxyElement.GetAttribute("color"));

            // SeerGoLogger.Log($"解析星系: ID={galaxy.Id}, 名称={galaxy.Name}, 资源={galaxy.ResId}");

            // 解析行星
            XmlNodeList planetNodes = galaxyElement.SelectNodes("Planet");
            if (planetNodes != null)
            {
                foreach (XmlElement planetElement in planetNodes)
                {
                    PlanetData planet = ParsePlanetElement(planetElement);
                    galaxy.Planets.Add(planet);

                    //SeerGoLogger.Log($"  行星: ID={planet.Id}, 名称={planet.Name}, 位置={planet.Position}");
                }
            }

            return galaxy;
        }

        public PlanetData ParsePlanetElement(XmlElement planetElement)
        {
            PlanetData planet = new PlanetData();

            planet.Id = int.Parse(planetElement.GetAttribute("id"));
            planet.Name = planetElement.GetAttribute("name");
            planet.ResId = planetElement.GetAttribute("resId");
            planet.Size = ParseUtils.ParseVector2(planetElement.GetAttribute("size"));
            planet.Position = ParseUtils.ParseVector2(planetElement.GetAttribute("pos"));
            planet.Color = ParseUtils.ParseColor(planetElement.GetAttribute("color"));

            return planet;
        }


     
    }

 
}