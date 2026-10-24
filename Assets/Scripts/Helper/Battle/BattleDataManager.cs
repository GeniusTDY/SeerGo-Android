using SeerGoGame.Models;
using System.Collections.Generic;
using UnityEngine;

public static class BattleDataManager
{
    // 使用静态字典缓存NPC数据
    private static Dictionary<string, MapNpc> _npcCache = new Dictionary<string, MapNpc>();

    // 缓存存档数据，用于场景切换
    public static ArchiveData CachedArchiveData { get; set; }

    // 当前地图ID
    public static int CurrentMapId { get; set; } = 0;

    // 最后上场精灵下标
    public static int LastPetIndex { get; set; } = -1;

    // 捕捉的宠物ID
    public static int CatchPetId { get; set; } = -1;

    // 战斗结果
    public static bool BattleResult { get; set; }

    // 当前对战类型
    public static BattleType CurrentBattleType { get; set; }

    // 勇者之塔：当前挑战是否为首次通关（战斗发起时判断，用于奖励发放）
    public static bool BraveTowerIsFirstClear { get; set; }
    public static bool TrialTowerIsFirstClear { get; set; }

    /// <summary>
    /// 塔类战斗达到上限层数时，记录实际通关层数（>0时有效，用于奖励服务计算beatenFloor）
    /// </summary>
    public static int TowerBeatenFloorOverride { get; set; }

    // 当前战斗胜利类型
    public static BattleWonType CurrentBattleWonType { get; set; }

    // 缓存键常量
    private const string SELECTED_NPC_KEY = "SelectedNpc";
    private const string CURRENT_ENEMY_KEY = "CurrentEnemy";

    /// <summary>
    /// 对战类型枚举
    /// </summary>
    public enum BattleType
    {
        NONE,
        NORMAL,
        BOSS,
        BRAVETOWER,
        TRIALTOWER,
        CHALLENGE
    }

    /// <summary>
    /// 战斗胜利类型枚举
    /// </summary>
    public enum BattleWonType
    {
        CATCHPET,  // 抓宠
        NORMAL     // 普通战斗
    }

    // 获取或设置当前选中的NPC
    public static MapNpc SelectedNpc
    {
        get => GetCachedNpc(SELECTED_NPC_KEY);
        set => CacheNpc(SELECTED_NPC_KEY, value);
    }

    // 获取或设置当前战斗敌人
    public static MapNpc CurrentEnemy
    {
        get => GetCachedNpc(CURRENT_ENEMY_KEY);
        set => CacheNpc(CURRENT_ENEMY_KEY, value);
    }

    /// <summary>
    /// 是否是普通战斗
    /// </summary>
    public static bool IsNormalBattle => CurrentBattleType == BattleType.NORMAL;

    /// <summary>
    /// 结束战斗
    /// </summary>
    /// <param name="isVictory">是否胜利</param>
    public static void EndBattle(bool isVictory)
    {
        BattleResult = isVictory;
    }

    /// <summary>
    /// 缓存NPC数据
    /// </summary>
    public static void CacheNpc(string key, MapNpc npc)
    {
        if (npc != null)
        {
            _npcCache[key] = CreateDeepCopy(npc);
        }
        else
        {
            _npcCache.Remove(key);
        }
    }

    /// <summary>
    /// 获取缓存的NPC数据
    /// </summary>
    public static MapNpc GetCachedNpc(string key)
    {
        return _npcCache.TryGetValue(key, out MapNpc npc) ? npc : null;
    }

    /// <summary>
    /// 检查是否缓存了指定键的NPC
    /// </summary>
    public static bool HasCachedNpc(string key) => _npcCache.ContainsKey(key);

    /// <summary>
    /// 移除缓存的NPC
    /// </summary>
    public static void RemoveCachedNpc(string key) => _npcCache.Remove(key);

    /// <summary>
    /// 清空所有缓存
    /// </summary>
    public static void ClearCache() => _npcCache.Clear();

    /// <summary>
    /// 创建深拷贝
    /// </summary>
    private static MapNpc CreateDeepCopy(MapNpc original)
    {
        if (original == null) return null;

        try
        {
            // 尝试使用JSON序列化/反序列化创建深拷贝
            string json = JsonUtility.ToJson(original);
            return JsonUtility.FromJson<MapNpc>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"创建MapNpc深拷贝失败: {e.Message}");

            // 回退：创建新实例并手动复制字段
            return ManualCopy(original);
        }
    }

    /// <summary>
    /// 手动复制MapNpc对象
    /// </summary>
    private static MapNpc ManualCopy(MapNpc original)
    {
        return new MapNpc
        {
            Id = original.Id,
            ResId = original.ResId,
            Type = original.Type,
            Sort = original.Sort,
            PolygonEnable = original.PolygonEnable,
            Name = original.Name,
            Size = original.Size,
            Position = original.Position,
            Rotation = original.Rotation,
            Color = original.Color,
            NamePosition = original.NamePosition,
            description = original.description,
            transport = original.transport,
            eventHandler = original.eventHandler,
            battle = original.battle
        };
    }

    /// <summary>
    /// 重置所有战斗相关数据
    /// </summary>
    public static void ResetBattleData()
    {
        CurrentBattleType = BattleType.NONE;
        CurrentBattleWonType = BattleWonType.NORMAL;
        BattleResult = false;
        LastPetIndex = -1;
        CurrentMapId = 0;
        CatchPetId = -1;
        BraveTowerIsFirstClear = false;
        TrialTowerIsFirstClear = false;
        TowerBeatenFloorOverride = 0;

        // 可选：清空缓存
        // ClearCache();
    }

    /// <summary>
    /// 重置NPC相关数据
    /// </summary>
    public static void ResetNpcData()
    {
        SelectedNpc = null;
        CurrentEnemy = null;
    }

    /// <summary>
    /// 开始新战斗
    /// </summary>
    public static void StartBattle(BattleType battleType, int petIndex)
    {
        CurrentBattleType = battleType;
        LastPetIndex = petIndex;
        BattleResult = false;
    }
}