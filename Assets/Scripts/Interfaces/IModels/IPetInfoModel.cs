using QFramework;
using SeerGoGame.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// 精灵数据模型接口 - 只提供数据访问
/// </summary>
namespace SeerGoGame.Interfaces
{
    public interface IPetInfoModel : IModel
    {
        // Model只提供原始数据访问，不包含业务逻辑
        Dictionary<int, MonsterData> MonsterDict { get; }
        Dictionary<string, MonsterData> NameToMonsterDict { get; }

        Task LoadMonsterConfigAsync();
    }
}