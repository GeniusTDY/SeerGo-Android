using QFramework;

// 动画帧系统接口
namespace SeerGoGame.Interfaces
{
    public interface IAnimationFrameSystem : ISystem
    {
        int GetFrameByLabel(int animationId, string label);
        bool HasAnimation(int animationId);
        bool HasLabel(int animationId, string label);
    }
}