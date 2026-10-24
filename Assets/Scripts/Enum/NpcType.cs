public enum NpcType
{
    Prefab,
    Sprite,
    Effect,
    // 可以轻松添加新类型
}

// 或者在类中定义常量
public static class NpcTypes
{
    public const string Prefab = "prefab";
    public const string Sprite = "sprite";
    public const string Effect = "effect";
}