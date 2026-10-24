using SeerGoGame.Commands;
using SeerGoGame.Utilities;
using UnityEngine;

namespace SeerGoGame
{
    public class GameInitializer : MonoBehaviour
    {
        void Start()
        {
            // 初始化
            if (SeerGoArchitecture.Interface == null)
            {
                Debug.LogError("架构初始化失败！");
                return;
            }
            // 日志类
            // 初始化日志系统
            SeerGoLogger.Init();
            // 设置日志开关
            // SeerGoLogger.SetLogEnable(false, true, true); // 需要添加此方法到XxLogger类
        }
    }
}