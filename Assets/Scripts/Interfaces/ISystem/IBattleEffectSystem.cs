using QFramework;
using SeerGoGame.Models;
using System.Collections.Generic;

// 战斗效果系统接口
namespace SeerGoGame.Interfaces
{
    public interface IBattleEffectSystem : ISystem
    {
        BattleEffectData GetBattleEffectByType(int type);
    }
}