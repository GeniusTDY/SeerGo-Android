using QFramework;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using System.Collections.Generic;

// 战斗效果系统实现
namespace SeerGoGame.Systems
{
    public class BattleEffectSystem : AbstractSystem, IBattleEffectSystem
    {
        private IBattleEffectModel _battleEffectModel;

        protected override void OnInit()
        {
            _battleEffectModel = this.GetModel<IBattleEffectModel>();
        }

        // 根据类型获取战斗效果
        public BattleEffectData GetBattleEffectByType(int type)
        {
            return _battleEffectModel.GetBattleEffectByType(type);
        }

    }
}