using QFramework;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using SeerGoGame.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 属性系统实现
/// </summary>
namespace SeerGoGame.Systems
{
    public class AttributeSystem : AbstractSystem, IAttributeSystem
    {
        private IAttributeModel m_model;

        protected override void OnInit()
        {
            m_model = this.GetModel<IAttributeModel>();
        }

        /// <summary>
        /// 根据ID获取属性数据
        /// </summary>
        public AttributeData GetAttributeById(int id)
        {
            var attribute = m_model.GetAttributeById(id);
            if (attribute == null)
            {
                SeerGoLogger.LogWarning($"未找到ID为 {id} 的属性数据");
            }
            return attribute;
        }

        /// <summary>
        /// 获取属性组合描述
        /// </summary>
        public string GetAttributeCombination(int id1, int id2)
        {
            var attr1 = GetAttributeById(id1);
            var attr2 = GetAttributeById(id2);

            if (attr1 == null || attr2 == null) return "无效的属性组合";

            return $"{attr1.cn}+{attr2.cn} ({attr1.en}+{attr2.en})";
        }

        /// <summary>
        /// 判断是否为双属性
        /// </summary>
        public bool IsDoubleAttribute(int attributeId)
        {
            var attribute = GetAttributeById(attributeId);
            return attribute?.is_dou == 1;
        }

        /// <summary>
        /// 获取双属性的组件属性ID
        /// </summary>
        public List<int> GetAttributeComponents(int doubleAttributeId)
        {
            var attribute = GetAttributeById(doubleAttributeId);
            if (attribute == null || attribute.is_dou != 1 || string.IsNullOrEmpty(attribute.att))
            {
                return new List<int>();
            }

            try
            {
                return attribute.att.Split(' ')
                    .Select(idStr => int.Parse(idStr.Trim()))
                    .ToList();
            }
            catch (System.Exception e)
            {
                SeerGoLogger.LogError($"解析双属性组件时发生错误: {e.Message}");
                return new List<int>();
            }
        }
    }
}