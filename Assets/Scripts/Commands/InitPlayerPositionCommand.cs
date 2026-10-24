using QFramework;
using SeerGoGame.Events;
using SeerGoGame.Interfaces;

/**
 * 地图变更初始化赛尔位置
 */
namespace SeerGoGame.Commands
{
    public class InitPlayerPositionCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            this.SendEvent(new InitPlayerPositionResultEvent
            {
                Position = this.GetModel<IMapModel>().CurrentMap.initialPoint,
                Success = true
            });
        }

    }
}