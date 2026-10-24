using QFramework;

// 类型关系系统接口
namespace SeerGoGame.Interfaces
{
    public interface ITypeRelationSystem : ISystem
    {
        float GetDamageMultiplier(string attackerType, string defenderType);
    }
}