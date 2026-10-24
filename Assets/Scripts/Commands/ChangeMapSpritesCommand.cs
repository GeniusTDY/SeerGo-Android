using QFramework;
using SeerGoGame.Events;
using SeerGoGame.Interfaces;
using SeerGoGame.Models;
using System.Collections.Generic;
using UnityEngine;

/**
 * 地图切换加载与销毁精灵
 */
namespace SeerGoGame.Commands
{
    public class ChangeMapSpritesCommand : AbstractCommand
    {
        private Transform _ParentTransform;
        private List<GameObject> _spritesList;
        private LoadProgressController _ProgressController;

        public ChangeMapSpritesCommand(List<GameObject> spritesList, LoadProgressController progressController, Transform parentTransform = null)
        {
            _ParentTransform = parentTransform;
            _spritesList = spritesList;
            _ProgressController = progressController;
        }


        protected override async void OnExecute()
        {
            var mapModel = this.GetModel<IMapModel>();

            var system = this.GetSystem<ICreateSpriteEntitySystem>();

        
            // 1. 卸载旧地图精灵
            await system.UnloadAllSpritesAsync();

            // 2. 加载新地图精灵
            foreach (MapNpc npc in mapModel.CurrentMap.AllNpcs)
            {
                 system.CreateSpriteAsync(npc, _ParentTransform, _spritesList);
            }
            //创建传送点
            foreach (MapTeleport teleport in mapModel.CurrentMap.AllTeleports)
            {
                system.CreateTeleporterSprite(teleport, _ParentTransform, _spritesList);
            }
            // 4.加载计数-1 
           // await Task.Delay(200);
            _ProgressController.CompleteTask("mapnpc");
            // 通知其他模块地图精灵已加载完成
             this.SendEvent(new PetLoadedEvent());
        }
    }
}