using System;
using System.Collections.Generic;
using UnityEngine;

namespace SCFrame
{
    public class AnimationEventTrigger : MonoBehaviour
    {
        private Dictionary<string, Action> _m_eventDic = new Dictionary<string, Action>();


        /// <summary>
        /// 动画事件触发
        /// </summary>
        /// <param name="_eventName">事件名</param>
        public void AnimationEvent(string _eventName)
        {
            if (_m_eventDic.TryGetValue(_eventName, out Action action))
                action?.Invoke();
        }
        public void AddAnimationEvent(string _eventName, Action _action)
        {
            if (_m_eventDic.TryGetValue(_eventName, out Action action))
                action += _action;
            else
                _m_eventDic.Add(_eventName, _action);
        }

        public void RemoveAnimationEvent(string _eventName)
        {
            _m_eventDic.Remove(_eventName);
        }

        public void RemoveAnimationEvent(string _eventName, Action _action)
        {
            if (_m_eventDic.TryGetValue(_eventName, out Action action))
                action -= _action;
        }

        public void CleanAllActionEvent()
        {
            _m_eventDic.Clear();
        }

    }
}
