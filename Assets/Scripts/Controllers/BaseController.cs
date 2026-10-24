using QFramework;
using UnityEngine;

namespace SeerGoGame.Controller
{
    public class BaseController : MonoBehaviour, IController
    {
        public IArchitecture GetArchitecture()
        {
            return SeerGoArchitecture.Interface;
        }
    }
}