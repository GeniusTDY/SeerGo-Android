using QFramework;
using SeerGoGame;
using UnityEngine;

public class BaseBattleController : MonoBehaviour, IController
{
    public IArchitecture GetArchitecture()
    {
        return BattleArchitecture.Interface;  // 连接到对战架构
    }
}
