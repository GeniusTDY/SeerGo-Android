using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 修正后的数据结构 - 完全匹配JSON结构
/// </summary>
namespace SeerGoGame.Models
{
    [System.Serializable]
    public class AttributeData
    {
        public int id;
        public string cn;
        public string en;
        public int is_dou;
        public string att;
    }

    [System.Serializable]
    public class AttributeItemWrapper
    {
        public List<AttributeData> item;
    }

    [System.Serializable]
    public class AttributeDataRoot
    {
        public AttributeItemWrapper root;
    }
}