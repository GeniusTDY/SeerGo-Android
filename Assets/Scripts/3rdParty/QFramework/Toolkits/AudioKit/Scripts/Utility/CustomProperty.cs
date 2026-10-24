/****************************************************************************
 * Copyright (c) 2016 ~ 2024 liangxiegame UNDER MIT LICENSE
 * 
 * https://qframework.cn
 * https://github.com/liangxiegame/QFramework
 * https://gitee.com/liangxiegame/QFramework
 ****************************************************************************/

using System;

using UnityEngine.Events;

namespace QFramework
{
    public class OnPropertyChangedEvent<T> : UnityEvent<T>
    {

    }
    
    public class CustomProperty<T> 
    {
        protected bool mSetted = false;
        
        public T Value
        {
            get => GetValue();
            set => SetValue(value);
        }
        
        protected virtual bool IsValueChanged(T value) => value == null || !value.Equals(mValue) || !mSetted;

        
        protected virtual void DispatchValueChangeEvent()
        {
            if (mSetter != null)
            {
                mSetter.Invoke(mValue);

                OnValueChanged.Invoke(mValue);

            }
        }
        
        protected T mValue;
        

        private event Action<T> mSetter = t => { };

        public readonly UnityEvent<T> OnValueChanged = new OnPropertyChangedEvent<T>();
  
        private readonly Func<T> mValueGetter = null;

        private readonly Action<T> mValueSetter = null;
        
        public CustomProperty(Func<T> valueGetter, Action<T> valueSetter = null)
        {
            mValueGetter = valueGetter;
            mValueSetter = valueSetter;
        }

        public void Bind(UnityAction<T> onValueChanged)
        {
            OnValueChanged.AddListener(onValueChanged);
        }
        
        
        private T GetValue()
        {
            mValue = mValueGetter.Invoke();
            return mValue;
        }
        
        private void SetValue(T value)
        {
            if (IsValueChanged(value))
            {
                mValue = value;

                DispatchValueChangeEvent();

                mSetted = true;

                if (mValueSetter != null) mValueSetter(value);
            }
        }
    }
}