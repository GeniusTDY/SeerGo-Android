using SeerGoGame.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

// 技能数据结构 - 根据JSON字段完整定义
namespace SeerGoGame.Models
{
    [Serializable]
    public class SkillData
    {
        [SerializeField] public string ID; // 技能ID
        [SerializeField] public string Name; // 技能名称
        [SerializeField] public int Category; // 技能类别  1物理攻击， 2特殊攻击， 4属性技能
        [SerializeField] public int Type; // 技能类型
        [SerializeField] public int Power; // 威力
        [SerializeField] public int MaxPP; // 最大PP值
        [SerializeField] public int Accuracy; // 命中率
        [SerializeField] public string SideEffect; // 副作用类型
        [SerializeField] public string SideEffectArg; // 副作用参数
        [SerializeField] public string Url; // 技能图标URL
        [SerializeField] public int CD; // 冷却时间
        [SerializeField] public int CritRate = 1; // 暴击率
        [SerializeField] public int Priority = 0; // 优先级
        [SerializeField] public bool MustHit; // 必中标志
        [SerializeField] public int AtkNum; // 攻击次数
        [SerializeField] public bool PwrDouble; // 威力加倍标志
        [SerializeField] public string Ordinary; // 普通技能标志

        public List<int> GetSideEffectIds()
        {
            var ids = new List<int>();
            if (!string.IsNullOrEmpty(SideEffect))
            {
                string[] idStrings = SideEffect.Split(' ');
                foreach (string idStr in idStrings)
                {
                    if (int.TryParse(idStr.Trim(), out int id))
                    {
                        ids.Add(id);
                    }
                }
            }
            return ids;
        }
    }

    // JSON文件根结构
    [Serializable]
    public class MovesDataRoot
    {
        public List<SkillData> moves;
    }
}