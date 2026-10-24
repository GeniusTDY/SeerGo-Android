using System;
using System.Collections.Generic;

namespace SeerGoGame.Models
{
    /// <summary>
    /// 试炼之塔每层怪物数据
    /// </summary>
    [Serializable]
    public class TrialTowerMonsterData : ITowerMonsterData
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
    /// 试炼之塔每层配置（楼层号 + 怪物列表）
    /// </summary>
    [Serializable]
    public class TrialTowerFloorData
    {
        public int floor;
        public TrialTowerMonsterData[] monsters;

        public override string ToString()
        {
            var monsterCount = monsters != null ? monsters.Length : 0;
            return $"Floor[{floor}, Monsters={monsterCount}]";
        }
    }

    /// <summary>
    /// 试炼之塔奖励物品
    /// </summary>
    [Serializable]
    public class TrialTowerRewardItem
    {
        public string itemId;
        public int count;

        public override string ToString()
        {
            return $"Item[Id={itemId}, Count={count}]";
        }
    }

    /// <summary>
    /// 试炼之塔单层奖励（数组格式，含floor字段，兼容JsonUtility）
    /// </summary>
    [Serializable]
    public class TrialTowerRewardEntry
    {
        public int floor;
        public int coins;
        public TrialTowerRewardItem[] items;
        public int goldBeans;
        public int milestoneGoldBeans;
        public int petRewardId;
        public int titleRewardId;
        public int expPool;

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
    public class TrialTowerRewardArrayContainer
    {
        public TrialTowerRewardEntry[] entries;
    }

    /// <summary>
    /// 楼层数组容器（用于JsonUtility解析）
    /// </summary>
    [Serializable]
    public class TrialTowerFloorsContainer
    {
        public TrialTowerFloorData[] floors;
    }

    /// <summary>
    /// 试炼之塔配置主类
    /// </summary>
    public class TrialTowerConfig
    {
        /// <summary>楼层配置列表（按floor号索引）</summary>
        public List<TrialTowerFloorData> Floors = new List<TrialTowerFloorData>();

        /// <summary>奖励配置（key=楼层号）</summary>
        public Dictionary<int, TrialTowerRewardEntry> FloorRewards = new Dictionary<int, TrialTowerRewardEntry>();

        /// <summary>总楼层数</summary>
        public int FloorCount => Floors.Count;

        /// <summary>
        /// 根据楼层号获取楼层配置
        /// </summary>
        public TrialTowerFloorData GetFloorData(int floorNumber)
        {
            return Floors.Find(f => f.floor == floorNumber);
        }

        /// <summary>
        /// 根据楼层号获取奖励配置
        /// </summary>
        public TrialTowerRewardEntry GetRewardData(int floorNumber)
        {
            FloorRewards.TryGetValue(floorNumber, out var reward);
            return reward;
        }

        public override string ToString()
        {
            return $"TrialTowerConfig[Floors={FloorCount}, Rewards={FloorRewards.Count}]";
        }
    }
}
