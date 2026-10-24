using QFramework;
using SeerGoGame.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Interfaces
{
    public interface IWorldMapXmlParser : IUtility
    {
        List<GalaxyData> ParseGalaxiesFromXml(string xmlContent);
        List<GalaxyData> ParseGalaxiesFromFile(string filePath);
    }
}