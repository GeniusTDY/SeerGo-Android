using QFramework;
using SeerGoGame.Commands;
using SeerGoGame.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace SeerGoGame
{
    public class BattleInitializer : BaseBattleController
    {
        public Image bgPanel;

        void Start()
        {
            // 初始化
            if (BattleArchitecture.Interface == null)
            {
                SeerGoLogger.LogError("BattleArchitecture架构初始化失败！");
                return;
            }
            // 加载地图背景（外部资源）
            bgPanel.sprite = ExternalResourcesManager.Instance.LoadSprite(ResourcePathBuilder.GetMapBackgroundPath(BattleDataManager.CurrentMapId));
            // 加载场景资源
            BattleLoadProgressController.Instance.RegisterQueue("battleData", 9);
            this.SendCommand<InitBattleDataCommand>();
        }
    }
}