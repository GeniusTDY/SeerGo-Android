using System;
using System.Collections.Generic;
using UnityEngine;
namespace SeerGoGame.Models
{
    // 对手类型数据
    [Serializable]
    public class OpponentData
    {
        [SerializeField] public string type;        // 对手类型
        [SerializeField] public float multiple;      // 伤害倍数
    }

    // 关系数据
    [Serializable]
    public class RelationData
    {
        [SerializeField] public List<OpponentData> Opponent; // 对手列表
        [SerializeField] public string type;                  // 关系类型
    }

    // 根对象
    [Serializable]
    public class RootObject
    {
        [SerializeField] public List<RelationData> Relation; // 关系列表
    }

    // JSON根对象
    [Serializable]
    public class TypeRelationRoot
    {
        [SerializeField] public RootObject Root; // 根对象
    }
}