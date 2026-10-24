using System;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Models
{
    /// <summary>
    /// 技能效果介绍数据（新版，从 SkillEffectInfo.xlsx 生成）
    /// id 为技能ID（int），help 为技能效果描述
    /// </summary>
    [Serializable]
    public class SkillEffectInfoData
    {
        [SerializeField] public int id;
        [SerializeField] public string help = "";
    }

    /// <summary>
    /// SkillEffectInfo.json 根结构
    /// </summary>
    [Serializable]
    public class SkillEffectInfoRoot
    {
        public List<SkillEffectInfoData> info = new List<SkillEffectInfoData>();
    }

    // ===== 旧版，保留兼容 =====

    [Serializable]
    public class SkillSideEffectInfoData
    {
        [SerializeField] public string id = "-1"; // 技能ID
        [SerializeField] public string help = ""; // 技能效果介绍
    }

    // JSON文件根结构
    [Serializable]
    public class SkillSideEffectInfoRoot
    {
        public List<SkillSideEffectInfoData> info = new List<SkillSideEffectInfoData>();
    }
}