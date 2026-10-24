using QFramework;
using SeerGoGame.Interfaces;
using SeerGoGame.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace SeerGoGame.Systems
{
    public class UISystem : AbstractSystem, IUISystem
    {
        private Transform canvasTrans;
        private CanvasGroup canvasGroup;
        private Dictionary<string, GameObject> openedWindow = new Dictionary<string, GameObject>();
        protected override void OnInit()
        {
            canvasTrans = Object.FindObjectOfType<Canvas>().transform;
        }
        public void CloseUI(string name)
        {
            if (!openedWindow.TryGetValue(name, out var window)) return;
            Object.Destroy(window);
            openedWindow.Remove(name);
        }

        public void OpenUI(string name)
        {
            if (openedWindow.ContainsKey(name)) return;
            var go = Object.Instantiate(Resources.Load<GameObject>($"Prefabs/{name}"), canvasTrans);

            if (go == null)
            {
                SeerGoLogger.LogError($"Prefab加载失败！");
                return;
            }
            openedWindow[name] = go;
        }

        public void SetUIActive(string name, bool isEnabled)
        {

            if (openedWindow.ContainsKey(name))
            {
                openedWindow[name].SetActive(isEnabled);
                return;
            }
            GameObject targetCanvas = GameObject.Find(name);
            if (targetCanvas != null)
            {
                openedWindow[name] = targetCanvas;
                targetCanvas.SetActive(isEnabled);
            }
            else
            {
                SeerGoLogger.LogError("未找到指定名称的Canvas");
            }

        }

        public void SetScriptActive<T>(string tag, bool isEnabled) where T : Behaviour
        {
            
            T component = GameObject.Find(tag).GetComponent<T>();
            if (component != null)
            {
                component.enabled = isEnabled;
            }
            else
            {
                SeerGoLogger.LogWarning($"在当前物体 {component.name} 上未找到 {typeof(T).Name} 组件。");
            }
        }

        // Canvasgroup控制显示隐藏
        public void SetUICanvasGroup(string name, bool isEnabled)
        {
            // 从已打开的窗口中查找
            if (openedWindow.TryGetValue(name, out GameObject targetWindow))
            {
                CanvasGroup canvasGroup = GetOrAddCanvasGroup(targetWindow);
                SetCanvasGroupProperties(canvasGroup, isEnabled);
                return;
            }

            // 查找新窗口
            GameObject targetCanvas = GameObject.Find(name);
            if (targetCanvas == null)
            {
                SeerGoLogger.LogError($"未找到指定名称的Canvas: {name}");
                return;
            }

            // 添加到已打开窗口字典并设置属性
            openedWindow[name] = targetCanvas;
            CanvasGroup newCanvasGroup = GetOrAddCanvasGroup(targetCanvas);
            SetCanvasGroupProperties(newCanvasGroup, isEnabled);
        }

        private CanvasGroup GetOrAddCanvasGroup(GameObject target)
        {
            CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = target.AddComponent<CanvasGroup>();
                SeerGoLogger.LogWarning($"为{target.name}自动添加了CanvasGroup组件");
            }
            return canvasGroup;
        }

        private void SetCanvasGroupProperties(CanvasGroup canvasGroup, bool isEnabled)
        {
            canvasGroup.alpha = isEnabled ? 1 : 0;
            canvasGroup.interactable = isEnabled;
            canvasGroup.blocksRaycasts = isEnabled;
        }


        // 封装获取组件并返回的方法
        public T GetTargetComponent<T>(string tag) where T : Behaviour
        {
            return GameObject.FindGameObjectWithTag(tag).GetComponent<T>();
        }

        public void ClearAllWindows()
        {
            // 遍历并销毁所有已打开的窗口对象
            foreach (var windowPair in openedWindow)
            {
                if (windowPair.Value != null)
                {
                    Object.Destroy(windowPair.Value);
                }
            }

            // 清空字典中的所有键值对
            openedWindow.Clear();

        }

        public void Deinit()
        {
            ClearAllWindows();
        }
    }
}