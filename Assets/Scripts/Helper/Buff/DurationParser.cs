using SeerGoGame.Utilities;

namespace SeerGoGame.Helper
{
    // buff持续时间解析器
    public static class DurationParser
    {
        public static int Parse(string duration)
        {
            if (string.IsNullOrEmpty(duration)) return 1;

            // 尝试直接解析为数字
            if (int.TryParse(duration, out int num)) return num;

            // 尝试解析范围格式
            if (duration.Contains("-"))
            {
                string[] parts = duration.Split('-');
                if (parts.Length == 2 &&
                    int.TryParse(parts[0], out int min) &&
                    int.TryParse(parts[1], out int max))
                {
                    if (min > max) (min, max) = (max, min);
                    return UnityEngine.Random.Range(min, max + 1);
                }
            }

            SeerGoLogger.LogError($"无法解析duration: {duration}");
            return 1;
        }
    }
}