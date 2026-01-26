using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCFrame
{
    public class SCEventListenerEventInfo<T>
    {
        // T：事件本身的参数（PointerEventData、Collision） object[]:事件的参数
        public Action<T, object[]> action;
        public object[] args;
        public void Init(Action<T, object[]> _action, object[] _args)
        {
            this.action = _action;
            this.args = _args;
        }
        public void Destory()
        {
            this.action = null;
            this.args = null;
            this.ObjectPushPool();
        }
        public void Trigger(T _eventData)
        {
            action?.Invoke(_eventData, args);
        }
    }
}
