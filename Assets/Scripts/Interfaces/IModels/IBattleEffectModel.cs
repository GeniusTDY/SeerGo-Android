using System.Collections.Generic;
using System.Threading.Tasks;
using QFramework;
using SeerGoGame.Models;

// 战斗效果模型接口
namespace SeerGoGame.Interfaces
{
    public interface IBattleEffectModel : IModel
    {
        Dictionary<int, BattleEffectData> BattleEffectDictionary { get; }
        Task LoadBattleEffectInfoConfigAsync();
        BattleEffectData GetBattleEffectByType(int type);
    }
}