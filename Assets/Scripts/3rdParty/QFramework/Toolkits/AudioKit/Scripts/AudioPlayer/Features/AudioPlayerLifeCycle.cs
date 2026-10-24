using System;

namespace QFramework
{
    internal class AudioPlayerLifeCycle
    {
        internal Action OnStart = null;
        internal Action OnFinish = null;

        internal void RegisterOnStartOnce(Action onStart)
        {
            if (onStart == null) return;
            
            if (OnStart == null)
            {
                OnStart = onStart;
            }
            else
            {
                OnStart += onStart;
            }
        }

        internal void RegisterOnFinishOnce(Action onFinish)
        {
            if (onFinish == null) return;
            
            if (OnFinish == null)
            {
                OnFinish = onFinish;
            }
            else
            {
                OnFinish += onFinish;
            }
        }
        

        internal void Clear()
        {
            OnStart = null;
            OnFinish = null;
        }

        internal void CallOnStartOnce()
        {
            OnStart?.Invoke();
            OnStart = null;
        }

        internal void CallOnFinishOnce()
        {
            OnFinish?.Invoke();
            OnFinish = null;
        }
        
    }
}