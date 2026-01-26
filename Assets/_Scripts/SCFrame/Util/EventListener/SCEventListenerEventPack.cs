using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SCFrame
{
    interface ISCEventListenerEventPack
    {
        void RemoveAll();

    }

    /// <summary>
    /// 一类事件的数据包装类型：包含多个SCEventListenerEventInfo
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SCEventListenerEventPack<T> : ISCEventListenerEventPack
    {

        // 所有的事件
        private List<SCEventListenerEventInfo<T>> _m_eventList = new List<SCEventListenerEventInfo<T>>();

        /// <summary>
        /// 添加事件
        /// </summary>
        public void AddListener(Action<T, object[]> _action, params object[] _args)
        {
            SCEventListenerEventInfo<T> info = SCPoolMgr.instance.GetObject<SCEventListenerEventInfo<T>>();
            info.Init(_action, _args);
            _m_eventList.Add(info);
        }

        /// <summary>
        /// 移除事件
        /// </summary>
        public void RemoveListener(Action<T, object[]> _action, bool _checkArgs = false, params object[] _args)
        {
            for (int i = 0; i < _m_eventList.Count; i++)
            {
                // 找到这个事件
                if (_m_eventList[i].action.Equals(_action))
                {
                    // 是否需要检查参数
                    if (_checkArgs && _args.Length > 0)
                    {
                        // 参数如果相等
                        if (_args.ArraryEquals(_m_eventList[i].args))
                        {
                            // 移除
                            _m_eventList[i].Destory();
                            _m_eventList.RemoveAt(i);
                            return;
                        }
                    }
                    else
                    {
                        // 移除
                        _m_eventList[i].Destory();
                        _m_eventList.RemoveAt(i);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 移除全部，全部放进对象池
        /// </summary>
        public void RemoveAll()
        {
            for (int i = 0; i < _m_eventList.Count; i++)
            {
                _m_eventList[i].Destory();
            }
            _m_eventList.Clear();
            this.ObjectPushPool();
        }

        public void TriggerEvent(T evetData)
        {
            for (int i = 0; i < _m_eventList.Count; i++)
            {
                _m_eventList[i].Trigger(evetData);
            }
        }

    }
}

