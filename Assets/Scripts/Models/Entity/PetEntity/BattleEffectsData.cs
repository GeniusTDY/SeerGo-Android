using System;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Models
{
    // 子效果数据结构
    [Serializable]
    public class SubEffectData
    {
        [SerializeField] public int ID;           // 子效果ID
        [SerializeField] public string Name;       // 子效果名称
        [SerializeField] public int Efftype;      // 子效果类型
    }

    // 战斗效果数据结构
    [Serializable]
    public class BattleEffectData
    {
        [SerializeField] public List<SubEffectData> SubEffect;  // 子效果列表
        [SerializeField] public int Type;                       // 效果类型
        [SerializeField] public string Name;                   // 效果名称
    }

    // 战斗效果根数据结构
    [Serializable]
    public class BattleEffectsRoot
    {
        [SerializeField] public List<BattleEffectData> BattleEffect;  // 战斗效果列表
    }

    // JSON根对象
    [Serializable]
    public class BattleEffectJsonRoot
    {
        [SerializeField] public BattleEffectsRoot BattleEffects;  // 战斗效果根数据
    }
}