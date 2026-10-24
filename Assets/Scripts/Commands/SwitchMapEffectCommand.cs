using QFramework;
using SeerGoGame.Events;
using SeerGoGame.Interfaces;
using UnityEngine;

/**
 * 地图切换加载与销毁动画
 */
namespace SeerGoGame.Commands
{
    public class SwitchMapEffectCommand : AbstractCommand
    {
        private int _TargetMapId;
        private Transform _ParentTransform;
        private LoadProgressController _ProgressController;

        public SwitchMapEffectCommand(int targetMapId, LoadProgressController progressController, Transform parentTransform = null)
        {
            _TargetMapId = targetMapId;
            _ParentTransform = parentTransform;
            _ProgressController = progressController;
        }

        protected override async void OnExecute()
        {
            var model = this.GetModel<IMapEffectModel>();
            var system = this.GetSystem<IMapEffectSystem>();

            // 如果切换的是同一张地图，则不做任何操作
            if (model.CurrentMapId == _TargetMapId) return;

            // 1. 卸载旧地图特效
            await system.UnloadEffectsAsync(model.CurrentMapId);

            // 2. 更新当前地图ID
            model.CurrentMapId = _TargetMapId;

            // 3. 加载新地图特效
             await system.LoadEffectsAsync(_TargetMapId, _ParentTransform);
            // 4.加载计数-1 
            _ProgressController.CompleteTask("mapnpc");
            // 可以发送一个事件，通知其他模块地图特效已切换完成
            this.SendEvent(new MapEffectsSwitchedEvent(_TargetMapId));
        }

    }
}