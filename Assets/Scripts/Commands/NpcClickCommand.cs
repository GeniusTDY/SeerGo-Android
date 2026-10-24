using QFramework;
using SeerGoGame.Events;
using SeerGoGame.Models;
using SeerGoGame.Utilities;
using System;

namespace SeerGoGame.Commands
{
    public class NpcClickCommand : AbstractCommand
    {
        private Object _mapEntity;

        public NpcClickCommand(Object mapEntity)
        {
            _mapEntity = mapEntity;
        }
        protected override void OnExecute()
        {
            if (_mapEntity is MapNpc npc)
            {
                this.SendEvent(new NpcClickRequestEvent
                {
                    NpcId = npc.Id,
                    ActionType = npc.eventHandler.button.action,
                    Parameters = npc.eventHandler.button.param,
                });
            }
            else if (_mapEntity is MapTeleport teleport)
            {
                this.SendEvent(new NpcClickRequestEvent
                {
                    NpcId = teleport.Id,
                    ActionType = "teleport",
                    Parameters = teleport.TargetMapId,
                });
            }
            else
            {
                // 处理未知类型或意外类型
                SeerGoLogger.Log($"未知实体类型: {_mapEntity.GetType()}");
            }
            
        }
    }
}