using QFramework;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Interfaces
{

    public interface IUISystem : ISystem
    {
        void OpenUI(string name);
        void CloseUI(string name);
        void SetUIActive(string name, bool isEnabled);
        void SetScriptActive<T>(string tag, bool isEnabled) where T : Behaviour;
        void SetUICanvasGroup(string name, bool isEnabled);
        T GetTargetComponent<T>(string tag) where T : Behaviour;
    }
}