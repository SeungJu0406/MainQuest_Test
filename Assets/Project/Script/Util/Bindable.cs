using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    public class Bindable<T>
    {
        private T _value;

        /// <summary>
        /// Bindable<TStatus>의 값입니다. 값이 변경될 때마다 OnValueChanged 이벤트가 호출됩니다.
        /// </summary>
        public T Value
        {
            get { return _value; }
            set
            {
                if (_isAnyWayEvent == false)
                {
                    if (EqualityComparer<T>.Default.Equals(_value, value) == true) // Default를 통해 해당 타입 T에 맞는 기본 비교기 반환                   
                        return;
                }
                _value = value;
                OnValueChanged?.Invoke(_value);
            }
        }
        public Action<T> OnValueChanged;

        private bool _isAnyWayEvent = false;


        public static implicit operator T(Bindable<T> bindable)
        {
            return bindable.Value;
        }
        /// <summary>
        /// 바인딩된 콜백을 호출하고, 값이 변경될 때마다 콜백을 호출합니다.
        /// </summary>
        /// <param name="callback"></param>
        public void Bind(Action<T> callback, bool IsInvoke = true)
        {
            OnValueChanged += callback;
            if(IsInvoke == true)
                callback?.Invoke(Value);
        }

        /// <summary>
        /// 바인딩된 콜백을 호출하고, 초기값을 설정합니다. 이후 값이 변경될 때마다 콜백을 호출합니다.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="initialValue"></param>
        public void Bind(Action<T> callback, T initialValue)
        {
            Value = initialValue;
            Bind(callback);
        }

        public void UnBind(Action<T> callback)
        {
            OnValueChanged -= callback;
        }

        public Bindable(T initialValue = default, bool isAnyWayEvent = false)
        {
            _value = initialValue;
            _isAnyWayEvent = isAnyWayEvent;
        }
    }
}