using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;
using UnityEngine;
using SeerGoGame.Models;

/// <summary>
/// 直接从 SkillEffectInfo.xlsx 解析技能效果描述，无需中间JSON
/// xlsx本质是ZIP包，内部是XML
/// </summary>
public static class XlsxSkillEffectLoader
{
    private const string XLSX_PATH = "/Resources/Data/Pets/SkillEffectInfo.xlsx";

    /// <summary>
    /// 从 xlsx 加载技能效果数据
    /// </summary>
    public static List<SkillEffectInfoData> LoadFromXlsx()
    {
        string fullPath = Application.dataPath + XLSX_PATH;

        if (!File.Exists(fullPath))
        {
            Debug.LogError($"XlsxSkillEffectLoader: 找不到文件 {fullPath}");
            return null;
        }

        try
        {
            byte[] fileBytes = File.ReadAllBytes(fullPath);
            return ParseXlsxBytes(fileBytes);
        }
        catch (Exception e)
        {
            Debug.LogError($"XlsxSkillEffectLoader: 加载失败 - {e.Message}");
            return null;
        }
    }

    private static List<SkillEffectInfoData> ParseXlsxBytes(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

        // 1. 读取共享字符串表
        var stringsEntry = archive.GetEntry("xl/sharedStrings.xml");
        if (stringsEntry == null) throw new Exception("未找到 sharedStrings.xml");

        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        XDocument ssDoc;
        using (var entryStream = stringsEntry.Open())
            ssDoc = XDocument.Load(entryStream);

        var sharedStrings = new List<string>(capacity: 14000);
        foreach (var si in ssDoc.Descendants(ns + "si"))
        {
            string text = "";
            var t = si.Element(ns + "t");
            if (t != null && t.Value != null)
                text = t.Value;
            sharedStrings.Add(text);
        }
        ssDoc = null; // 释放 XML 树引用

        // 2. 读取 sheet1 数据
        var sheetEntry = archive.GetEntry("xl/worksheets/sheet1.xml");
        if (sheetEntry == null) throw new Exception("未找到 sheet1.xml");

        XDocument sheetDoc;
        using (var entryStream = sheetEntry.Open())
            sheetDoc = XDocument.Load(entryStream);

        var rows = sheetDoc.Descendants(ns + "row");
        var result = new List<SkillEffectInfoData>(capacity: 27000);

        foreach (var row in rows)
        {
            var cells = row.Elements(ns + "c");
            int? id = null;
            string help = "";

            foreach (var cell in cells)
            {
                string cellRef = (string)cell.Attribute("r");
                string cellType = (string)cell.Attribute("t");
                var valueEl = cell.Element(ns + "v");
                string value = valueEl?.Value;

                if (cellRef == null || value == null) continue;

                if (cellRef.StartsWith("A"))
                {
                    // A列 = id
                    if (int.TryParse(value, out int parsed))
                        id = parsed;
                }
                else if (cellRef.StartsWith("B"))
                {
                    // B列 = help
                    if (cellType == "s" && int.TryParse(value, out int ssIdx) && ssIdx >= 0 && ssIdx < sharedStrings.Count)
                        help = sharedStrings[ssIdx];
                    else
                        help = value;
                }
            }

            // 跳过表头行（id=0 或 null）和空help
            if (id.HasValue && id.Value > 0)
            {
                result.Add(new SkillEffectInfoData { id = id.Value, help = help });
            }
        }

        Debug.Log($"XlsxSkillEffectLoader: 成功加载 {result.Count} 条技能效果描述");
        return result;
    }
}
