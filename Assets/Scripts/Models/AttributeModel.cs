using QFramework;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using UnityEngine;
namespace SeerGoGame.Models
{
    /// <summary>
    /// 属性数据模型实现
    /// </summary>
    public class AttributeModel : AbstractModel, IAttributeModel
    {
        private Dictionary<int, AttributeData> m_attributeDict;
        private Dictionary<string, List<AttributeData>> m_doubleAttributesByType;

        public Dictionary<int, AttributeData> AttributeDict => m_attributeDict;
        public Dictionary<string, List<AttributeData>> DoubleAttributesByType => m_doubleAttributesByType;
  

        protected override void OnInit()
        {
            m_attributeDict = new Dictionary<int, AttributeData>();
            m_doubleAttributesByType = new Dictionary<string, List<AttributeData>>();
        }

        public async Task LoadAttributeConfigAsync()
        {
            string path = ResourcePathBuilder.GetPetsInfoDataPath();
            string fullPath = $"{path}/AttributeType";

            var result = await JsonResourceLoadManager.LoadConfigAsync<AttributeDataRoot>(fullPath);

            if (result.Success && result.Data?.root?.item != null)
            {
                foreach (var attribute in result.Data.root.item)
                {

                    if (!m_attributeDict.ContainsKey(attribute.id))
                    {
                        m_attributeDict[attribute.id] = attribute;

                        // 如果是双属性，按类型分类
                        if (attribute.is_dou == 1 && !string.IsNullOrEmpty(attribute.att))
                        {
                            string typeKey = attribute.att.Replace(" ", "_");
                            if (!m_doubleAttributesByType.ContainsKey(typeKey))
                            {
                                m_doubleAttributesByType[typeKey] = new List<AttributeData>();
                            }
                            m_doubleAttributesByType[typeKey].Add(attribute);
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"重复的属性ID: {attribute.id}");
                    }
                }
            }
        }

        /// <summary>
        /// 根据ID获取属性数据
        /// </summary>
        public AttributeData GetAttributeById(int id)
        {
            return m_attributeDict.TryGetValue(id, out var attribute) ? attribute : null;
        }

    }
}