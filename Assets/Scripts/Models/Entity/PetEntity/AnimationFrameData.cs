using System;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Models
{

    // 单个动画项数据结构
    [Serializable]
    public class AnimationItemData
    {
        [SerializeField] public string lable;  // 动画标签
        [SerializeField] public int frame;     // 动画帧数
    }

    // 动画列表项数据结构
    [Serializable]
    public class AnimationListData
    {
        [SerializeField] public List<AnimationItemData> item;  // 动画项列表
        [SerializeField] public int id;                        // 动画ID
    }

    // 根数据结构
    [Serializable]
    public class AnimationRootData
    {
        [SerializeField] public List<AnimationListData> list;  // 动画列表
    }

    // JSON根包装器
    [Serializable]
    public class AnimationFrameRootWrapper
    {
        [SerializeField] public AnimationRootData root;       // 根数据
    }
}