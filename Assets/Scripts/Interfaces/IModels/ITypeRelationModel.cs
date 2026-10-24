using QFramework;
using SeerGoGame.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

// 类型关系模型接口
namespace SeerGoGame.Interfaces
{
    public interface ITypeRelationModel : IModel
    {
        Dictionary<string, Dictionary<string, float>> TypeRelations { get; }
        Task LoaTypeRelationConfigAsync();
        float GetDamageMultiplier(string attackerType, string defenderType);
    }
}