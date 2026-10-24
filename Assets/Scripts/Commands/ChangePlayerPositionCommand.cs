using QFramework;
using SeerGoGame.Interfaces;
using UnityEngine;

namespace SeerGoGame.Commands
{
    public class ChangePlayerPositionCommand : AbstractCommand
    {
        private Vector3 _pos;

        public ChangePlayerPositionCommand(Vector3 pos)
        {
            _pos = pos;
        }
        protected override void OnExecute()
        {

            // 发送角色位置变更完成事件
            this.GetModel<IPlayerModel>().OnPositionChanged.Value = _pos;
        }

    }
}