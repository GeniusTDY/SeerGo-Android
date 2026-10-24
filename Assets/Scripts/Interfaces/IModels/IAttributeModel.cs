using QFramework;
using SeerGoGame.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace SeerGoGame.Interfaces
{
    /// <summary>
    /// 属性模型接口
    /// </summary>
    public interface IAttributeModel : IModel
    {
        Dictionary<int, AttributeData> AttributeDict { get; }
        Dictionary<string, List<AttributeData>> DoubleAttributesByType { get; }
        Task LoadAttributeConfigAsync();
        AttributeData GetAttributeById(int id);
    }
}