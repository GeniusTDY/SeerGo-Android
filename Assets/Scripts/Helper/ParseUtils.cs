using UnityEngine;

public static class ParseUtils
{
    public static Vector2 ParseVector2(string str)
    {
        string[] parts = str.Split(',');
        if (parts.Length >= 2)
        {
            return new Vector2(float.Parse(parts[0]), float.Parse(parts[1]));
        }
        return Vector2.zero;
    }
    // 辅助方法：解析Vector3
    public static Vector3 ParseVector3(string str)
    {
        string[] parts = str.Split(',');
        if (parts.Length >= 3)
        {
            return new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
        }
        return Vector3.zero;
    }
    // 辅助方法：解析Color
    public static Color ParseColor(string str)
    {
        string[] parts = str.Split(',');
        if (parts.Length >= 4)
        {
            return new Color(
                float.Parse(parts[0]) / 255f,
                float.Parse(parts[1]) / 255f,
                float.Parse(parts[2]) / 255f,
                float.Parse(parts[3]) / 255f
            );
        }
        return Color.white;
    }

    public static Vector2 ParsePosition(string posStr)
    {
        if (string.IsNullOrEmpty(posStr)) return Vector2.zero;

        string[] parts = posStr.Split(',');
        if (parts.Length == 2)
        {
            float x = float.Parse(parts[0]);
            float y = float.Parse(parts[1]);
            return new Vector2(x, y);
        }
        return Vector2.zero;
    }
}
