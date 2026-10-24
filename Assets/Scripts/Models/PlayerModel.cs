using QFramework;
using SeerGoGame.Events;
using SeerGoGame.Interfaces;
using SeerGoGame.Utilities;
using UnityEngine;
using UnityEngine.UIElements;

namespace SeerGoGame.Models
{
    public class PlayerModel : AbstractModel, IPlayerModel
    {
        public Vector3 Position { get; set; } = new Vector3(0,-3,0);
        public int CurrentMapId { get; set; } = 1;
        public BindableProperty<Vector3> OnPositionChanged { get; } = new BindableProperty<Vector3>();

        protected override void OnInit()
        {
            OnPositionChanged.Register(newPosition =>
            {
                Position = newPosition;
                // 发送角色位置变更完成事件
                this.SendEvent(new PlayerPositionResultEvent
                {
                    Position = $"{Position.x},{Position.y},{Position.z}",
                    Success = true
                });
            });
        }
    }
}