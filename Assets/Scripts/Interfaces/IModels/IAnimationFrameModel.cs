using System.Collections.Generic;
using System.Threading.Tasks;
using QFramework;

// 动画帧模型接口
namespace SeerGoGame.Interfaces
{
    public interface IAnimationFrameModel : IModel
    {
        Dictionary<int, Dictionary<string, int>> AnimationFrames { get; }
        Task LoadAnimationConfigAsync();
    }
}