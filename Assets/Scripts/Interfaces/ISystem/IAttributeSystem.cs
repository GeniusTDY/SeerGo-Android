using QFramework;
using SeerGoGame.Models;
using System.Collections.Generic;

/// <summary>
/// 属性系统接口
/// </summary>
namespace SeerGoGame.Interfaces
{

public interface IAttributeSystem : ISystem
{
    AttributeData GetAttributeById(int id);

    string GetAttributeCombination(int id1, int id2);
    bool IsDoubleAttribute(int attributeId);
    List<int> GetAttributeComponents(int doubleAttributeId);
}
}