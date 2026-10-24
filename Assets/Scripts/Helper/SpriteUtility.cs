using UnityEngine;

/// <summary>
/// Sprite工具类，提供属性文本Sprite加载等实用方法
/// </summary>
public static class SpriteUtility
{
    private const string TYPE_TEXT_PATH = "Sprites/Pets/PetTypeText/{0}";

    /// <summary>
    /// 加载属性文本Sprite，自动处理单/双属性
    /// 单属性返回 (sprite, null)，双属性返回 (sprite1, sprite2)
    /// </summary>
    /// <param name="attrSystem">属性系统</param>
    /// <param name="attributeId">属性ID</param>
    /// <param name="loadFunc">Sprite加载委托</param>
    /// <returns>(icon, icon2) 元组，单属性时icon2为null</returns>
    public static (Sprite icon, Sprite icon2) LoadAttributeTextSprites(SeerGoGame.Interfaces.IAttributeSystem attrSystem, int attributeId, System.Func<string, Sprite> loadFunc)
    {
        if (!attrSystem.IsDoubleAttribute(attributeId))
        {
            // 单属性：只加载一张
            return (loadFunc(string.Format(TYPE_TEXT_PATH, attributeId)), null);
        }

        // 双属性：获取两个组件属性ID
        var components = attrSystem.GetAttributeComponents(attributeId);
        if (components.Count != 2)
        {
            // 异常情况，回退
            return (loadFunc(string.Format(TYPE_TEXT_PATH, attributeId)), null);
        }

        var sprite1 = loadFunc(string.Format(TYPE_TEXT_PATH, components[0]));
        var sprite2 = loadFunc(string.Format(TYPE_TEXT_PATH, components[1]));

        return (sprite1, sprite2);
    }
}
