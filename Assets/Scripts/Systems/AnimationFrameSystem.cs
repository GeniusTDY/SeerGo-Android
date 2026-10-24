using QFramework;
using SeerGoGame.Interfaces;
using SeerGoGame.Utilities;

// 动画帧系统实现
namespace SeerGoGame.Systems
{
    public class AnimationFrameSystem : AbstractSystem, IAnimationFrameSystem
    {
        private IAnimationFrameModel _animationFrameModel;

        protected override void OnInit()
        {
            _animationFrameModel = this.GetModel<IAnimationFrameModel>();
        }

        // 根据动画ID和标签获取帧数
        public int GetFrameByLabel(int animationId, string label)
        {
            if (_animationFrameModel.AnimationFrames.TryGetValue(animationId, out var labelFrames))
            {
                if (labelFrames.TryGetValue(label, out int frame))
                {
                    return frame;
                }
                else
                {
                    SeerGoLogger.LogWarning($"动画ID {animationId} 中没有找到标签 '{label}'");
                }
            }
            else
            {
                SeerGoLogger.LogWarning($"没有找到动画ID: {animationId}");
            }
            return 0; // 默认返回0
        }

        // 检查是否存在指定动画ID
        public bool HasAnimation(int animationId)
        {
            return _animationFrameModel.AnimationFrames.ContainsKey(animationId);
        }

        // 检查指定动画ID中是否存在指定标签
        public bool HasLabel(int animationId, string label)
        {
            return _animationFrameModel.AnimationFrames.TryGetValue(animationId, out var labelFrames) &&
                   labelFrames.ContainsKey(label);
        }
    }
}