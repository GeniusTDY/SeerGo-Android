using System.Collections.Generic;

public static class MoleculeMapping
{
    // 精元ID与精灵第一形态ID映射表
    private static readonly Dictionary<int, int> moleculeToPetMap = new Dictionary<int, int>()
    {
        {900, 48},   // 阿克西亚精元 -> 索拉
        {901, 68},   // 提亚斯精元 -> 米拉美
        {902, 70},   // 雷伊精元 -> 雷伊
        {903, 86},   // 纳多雷精元 -> 纳格
        {904, 111},  // 雷纳多精元 -> 雷格
        {905, 41},   // 里奥斯精元 -> 胡里亚
        {906, 131},  // 尤纳斯精元 -> 影球
        {907, 168},  // 卡特斯精元 -> 黑刺球
        {908, 170},  // 魔牙鲨精元 -> 利齿鱼
        {909, 172},  // 贝鲁基德精元 -> 黑火贝
        {910, 175},  // 巴弗洛精元 -> 希洛
        {911, 181},  // 奇拉塔顿精元 -> 洛奇
        {912, 186},  // 魔狮迪露精元 -> 露露
        {913, 193},  // 西萨拉斯精元 -> 奇达
        {914, 190},  // 克林卡修精元 -> 林奇
        {915, 215},  // 哈莫雷特精元 -> 哈莫
        {916, 222},  // 卡库精元 -> 卡库
        {917, 224},  // 赫德卡精元 -> 赫德卡
        {918, 227},  // 伊兰罗尼精元 -> 伊兰罗尼
        {919, 228},  // 斯加尔卡精元 -> 斯加尔卡
        {920, 359},  // 布林克克精元 -> 布林克克
        {921, 273},  // 塔克林精元 -> 塔克林
        {922, 390},  // 塔西亚精元 -> 塔西亚
        {923, 412},  // 塞维尔精元 -> 塞维尔
        {924, 436},   // 魔花使者精元 -> 蘑菇怪
        {925, 439},    // 莫尔加斯精元 -> 未知（暂时设为0）
        {926, 0},    // 莫萨诺拉斯精元 -> 未知（暂时设为0）
        {927, 300}   // 谱尼精元 -> 谱尼
    };

    /// <summary>
    /// 根据精元ID获取对应精灵的第一形态ID
    /// </summary>
    /// <param name="itemId">精元ID</param>
    /// <returns>精灵第一形态ID，如果不存在返回0</returns>
    public static int GetPetIdByItemId(int itemId)
    {
        if (moleculeToPetMap.TryGetValue(itemId, out int petId))
        {
            return petId;
        }
        return 0; // 如果未找到对应映射，返回0表示未知
    }

    /// <summary>
    /// 获取所有精元与精灵的映射关系
    /// </summary>
    /// <returns>精元ID到精灵ID的完整映射字典</returns>
    public static Dictionary<int, int> GetAllMappings()
    {
        // 返回一个新的字典副本，避免外部修改原始数据
        return new Dictionary<int, int>(moleculeToPetMap);
    }

    /// <summary>
    /// 添加或更新映射关系
    /// </summary>
    /// <param name="itemId">精元ID</param>
    /// <param name="petId">精灵ID</param>
    public static void AddOrUpdateMapping(int itemId, int petId)
    {
        moleculeToPetMap[itemId] = petId;
    }

    /// <summary>
    /// 检查精元ID是否存在映射
    /// </summary>
    /// <param name="itemId">精元ID</param>
    /// <returns>是否存在映射关系</returns>
    public static bool ContainsItemId(int itemId)
    {
        return moleculeToPetMap.ContainsKey(itemId);
    }

    /// <summary>
    /// 根据精元ID获取精灵名称（如果需要）
    /// </summary>
    /// <param name="itemId">精元ID</param>
    /// <returns>精灵名称</returns>
    public static string GetPetNameByItemId(int itemId)
    {
        // 这里可以扩展为返回精灵名称，需要额外的映射表
        // 暂时返回空字符串
        return "";
    }
}