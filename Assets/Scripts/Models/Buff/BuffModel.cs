
using QFramework;
using SeerGoGame.Helper;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Models
{
    // Buff目标类型枚举
    public enum BuffTargetType
    {
        Self,     // 作用于施法者自己
        Enemy,    // 作用于敌人（目标）
        Ally      // 作用于盟友
    }

    // Buff叠加策略枚举
    public enum BuffStackType
    {
        None,      // 不可叠加,也不刷新(保留第一次的)
        Stack,     // 可叠加效果(效果值累加,持续时间刷新)
        Refresh,   // 刷新持续时间(效果值不变,只刷新持续时间)
        Replace    // 替换(移除旧的,添加新的)
    }

    [Serializable]
    public class BuffConfigRoot
    {
        public List<BuffData> buffs;
    }

    [Serializable]
    public class BuffData
    {
        public int id;
        public string name;
        public string description;
        public string type;
        public string targetType;
        public string duration;
        public float effectValue;
        public string stackType = "refresh"; // 默认为刷新模式
        public int maxStackCount; // 最大叠加次数
        public bool keep;//换场是否保留
        public int switchCount;//换场保留次数
        public int icon; // 图标索引
        public bool isShow; // 是否显示图标
        public int tab;  // 标签分类   1控制类异常 2弱化类异常 3附属类异常 4增益 5减益 6护盾 7道具物品 8治疗 9能力变化

        // 获取作用方策略
        public BuffTargetType GetTargetType()
        {
            switch (targetType)
            {
                case "Self": return BuffTargetType.Self;
                case "Enemy": return BuffTargetType.Enemy;
                case "Ally": return BuffTargetType.Ally;
                default: return BuffTargetType.Self; // 默认自己
            }
        }

        // 获取叠加策略
        public BuffStackType GetStackType()
        {
            // 优先使用新的stackType字段
            if (!string.IsNullOrEmpty(stackType))
            {
                switch (stackType.ToLower())
                {
                    case "none": return BuffStackType.None;
                    case "stack": return BuffStackType.Stack;
                    case "refresh": return BuffStackType.Refresh;
                    case "replace": return BuffStackType.Replace;
                    default: return BuffStackType.Refresh; // 默认刷新
                }
            }

            return BuffStackType.None;
        }

    }

    public interface IBuffModel : IModel
    {
        Dictionary<int, BuffData> BuffConfigs { get; }
    }

    public class BuffModel : AbstractModel, IBuffModel
    {
        protected override void OnInit()
        {
            var config = ConfigHelper.LoadJsonConfig<BuffConfigRoot>("Config/BuffConfig");
            BuffConfigs = new Dictionary<int, BuffData>();
            if (config != null && config.buffs != null)
            {
                foreach (var buff in config.buffs)
                {
                    BuffConfigs[buff.id] = buff;
                }
            }
        }

        public Dictionary<int, BuffData> BuffConfigs { get; private set; }
    }
}