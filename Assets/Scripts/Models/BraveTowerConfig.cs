using System;
using System.Collections.Generic;

namespace SeerGoGame.Models
{
    /// <summary>
    /// 塔怪物数据公共接口（勇者之塔/试炼之塔共用）
    /// </summary>
    public interface ITowerMonsterData
    {
        int id { get; }
        int level { get; }
        int[] skills { get; }
    }

    /// <summary>
    /// 勇者之塔每层怪物数据
    /// </summary>
    [Serializable]
    public class BraveTowerMonsterData : ITowerMonsterData
    {
        public int id;
        public int level;
        public int[] skills;

        int ITowerMonsterData.id => id;
        int ITowerMonsterData.level => level;
        int[] ITowerMonsterData.skills => skills;

        public override string ToString()
        {
            var skillsStr = skills != null ? string.Join(",", skills) : "无";
            return $"Monster[Id={id}, Level={level}, Skills=[{skillsStr}]]";
        }
    }

    /// <summary>
    /// 勇者之塔每层配置（楼层号 + 怪物列表）
    /// </summary>
    [Serializable]
    public class BraveTowerFloorData
    {
        public int floor;
        public BraveTowerMonsterData[] monsters;

        public override string ToString()
        {
            var monsterCount = monsters != null ? monsters.Length : 0;
            return $"Floor[{floor}, Monsters={monsterCount}]";
        }
    }

    /// <summary>
    /// 勇者之塔奖励物品
    /// </summary>
    [Serializable]
    public class BraveTowerRewardItem
    {
        public string itemId;
        public int count;

        public override string ToString()
        {
            return $"Item[Id={itemId}, Count={count}]";
        }
    }

    /// <summary>
    /// 勇者之塔单层奖励（数组格式，含floor字段，兼容JsonUtility）
    /// </summary>
    [Serializable]
    public class BraveTowerRewardEntry
    {
        public int floor;
        public int coins;
        public BraveTowerRewardItem[] items;
        public int goldBeans;
        public int milestoneGoldBeans;
        public int petRewardId;
        public int titleRewardId;

        public override string ToString()
        {
            var itemCount = items != null ? items.Length : 0;
            return $"Reward[Floor={floor}, Coins={coins}, Items={itemCount}, GoldBeans={goldBeans}, MilestoneGoldBeans={milestoneGoldBeans}, PetRewardId={petRewardId}, TitleRewardId={titleRewardId}]";
        }
    }

    /// <summary>
    /// 奖励数组容器（兼容JsonUtility）
    /// </summary>
    [Serializable]
    public class BraveTowerRewardArrayContainer
    {
        public BraveTowerRewardEntry[] entries;
    }

    /// <summary>
    /// 楼层数组容器（用于JsonUtility解析）
    /// </summary>
    [Serializable]
    public class BraveTowerFloorsContainer
    {
        public BraveTowerFloorData[] floors;
    }

    /// <summary>
    /// 勇者之塔配置主类
    /// </summary>
    public class BraveTowerConfig
    {
        /// <summary>楼层配置列表（按floor号索引）</summary>
        public List<BraveTowerFloorData> Floors = new List<BraveTowerFloorData>();

        /// <summary>奖励配置（key=楼层号）</summary>
        public Dictionary<int, BraveTowerRewardEntry> FloorRewards = new Dictionary<int, BraveTowerRewardEntry>();

        /// <summary>总楼层数</summary>
        public int FloorCount => Floors.Count;

        /// <summary>
        /// 根据楼层号获取楼层配置
        /// </summary>
        public BraveTowerFloorData GetFloorData(int floorNumber)
        {
            return Floors.Find(f => f.floor == floorNumber);
        }

        /// <summary>
        /// 根据楼层号获取奖励配置
        /// </summary>
        public BraveTowerRewardEntry GetRewardData(int floorNumber)
        {
            FloorRewards.TryGetValue(floorNumber, out var reward);
            return reward;
        }

        public override string ToString()
        {
            return $"BraveTowerConfig[Floors={FloorCount}, Rewards={FloorRewards.Count}]";
        }
    }
}
