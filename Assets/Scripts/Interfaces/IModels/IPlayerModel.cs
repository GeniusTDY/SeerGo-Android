using QFramework;
using UnityEngine;

namespace SeerGoGame.Interfaces
{
    public interface IPlayerModel : IModel
    {
        Vector3 Position { get; set; }
        int CurrentMapId { get; set; }
        BindableProperty<Vector3> OnPositionChanged { get; }
    }
}