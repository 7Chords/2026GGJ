using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SCFrame
{
    /// <summary>
    /// 事件类型
    /// </summary>
    public enum ESCEventType
    {
        ON_MOUSE_ENTER = -10001,
        ON_MOUSE_EXIT = -10002,
        ON_MOUSE_LEFT_CLICK = -10003,
        ON_MOUSE_LEFT_CLICK_DOWN = -10004,
        ON_MOUSE_LEFT_CLICK_UP = -10005,
        ON_DRAG = -10006,
        ON_BEGIN_DRAG = -10007,
        ON_END_DRAG = -10008,
        ON_COLLISION_ENTER = -10009,
        ON_COLLISION_STAY = -10010,
        ON_COLLISION_EXIT = -10011,
        ON_COLLISION_ENTER_2D = -10012,
        ON_COLLISION_STAY_2D = -10013,
        ON_COLLISION_EXIT_2D = -10014,
        ON_TRIGGER_ENTER = -10015,
        ON_TRIGGER_STAY = -10016,
        ON_TRIGGER_EXIT = -10017,
        ON_TRIGGER_ENTER_2D = -10018,
        ON_TRIGGER_STAY_2D = -10019,
        ON_TRIGGER_EXIT_2D = -10020,
        ON_RELEASE_ADDRESSABLE_ASSET = -10021,
        //ON_HOVER_ENTER = -10022,
        //ON_HOVER_EXIT = -10023,
    }

    public interface ISCMouseEvent : 
        IPointerEnterHandler, 
        IPointerExitHandler, 
        IPointerClickHandler, 
        IPointerDownHandler, 
        IPointerUpHandler, 
        IBeginDragHandler, 
        IEndDragHandler, 
        IDragHandler
    { }




    public class SCEventListener : MonoBehaviour, ISCMouseEvent
    {
        //value是接口 具体执行的转
        private Dictionary<int, ISCEventListenerEventPack> _m_eventPackDic = new Dictionary<int, ISCEventListenerEventPack>();

        /// <summary>
        /// 添加事件
        /// </summary>
        public void AddListener<T>(int _eventTypeInt, Action<T, object[]> _action, params object[] _args)
        {
            if (_m_eventPackDic.TryGetValue(_eventTypeInt, out ISCEventListenerEventPack info))
            {
                (info as SCEventListenerEventPack<T>).AddListener(_action, _args);
            }
            else
            {
                SCEventListenerEventPack<T> infos = SCPoolMgr.instance.GetObject<SCEventListenerEventPack<T>>();
                infos.AddListener(_action, _args);
                _m_eventPackDic.Add(_eventTypeInt, infos);
            }
        }
        /// <summary>
        /// 添加事件
        /// </summary>
        public void AddListener<T>(ESCEventType _eventType, Action<T, object[]> _action, params object[] _args)
        {
            AddListener<T>((int)_eventType, _action, _args);
        }

        /// <summary>
        /// 移除事件
        /// </summary>
        public void RemoveListener<T>(int _eventTypeInt, Action<T, object[]> _action, bool _checkArgs = false, params object[] _args)
        {
            if (_m_eventPackDic.TryGetValue(_eventTypeInt, out ISCEventListenerEventPack info))
            {
                (info as SCEventListenerEventPack<T>).RemoveListener(_action, _checkArgs, _args);
            }
        }
        /// <summary>
        /// 移除事件
        /// </summary>
        public void RemoveListener<T>(ESCEventType _eventType, Action<T, object[]> _action, bool _checkArgs = false, params object[] _args)
        {
            RemoveListener((int)_eventType, _action, _checkArgs, _args);
        }

        /// <summary>
        /// 移除某一个事件类型下的全部事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_eventType"></param>
        public void RemoveAllListener(ESCEventType _eventType)
        {
            if (_m_eventPackDic.TryGetValue((int)_eventType, out ISCEventListenerEventPack infos))
            {
                infos.RemoveAll();
                _m_eventPackDic.Remove((int)_eventType);
            }
        }
        /// <summary>
        /// 移除全部事件
        /// </summary>
        public void RemoveAllListener()
        {
            foreach (ISCEventListenerEventPack infos in _m_eventPackDic.Values)
            {
                infos.RemoveAll();
            }

            _m_eventPackDic.Clear();
        }

        /// <summary>
        /// 触发事件
        /// </summary>
        public void TriggerAction<T>(int _eventTypeInt, T _eventData)
        {
            if (_m_eventPackDic.TryGetValue(_eventTypeInt, out ISCEventListenerEventPack infos))
            {
                (infos as SCEventListenerEventPack<T>).TriggerEvent(_eventData);
            }
        }
        /// <summary>
        /// 触发事件
        /// </summary>
        public void TriggerAction<T>(ESCEventType _eventType, T _eventData)
        {
            TriggerAction<T>((int)_eventType, _eventData);
        }

        #region UI
        public void OnPointerEnter(PointerEventData _eventData)
        {
            TriggerAction(ESCEventType.ON_MOUSE_ENTER, _eventData);
        }

        public void OnPointerExit(PointerEventData _eventData)
        {
            TriggerAction(ESCEventType.ON_MOUSE_EXIT, _eventData);
        }

        public void OnPointerClick(PointerEventData _eventData)
        {
            if(_eventData.button == PointerEventData.InputButton.Left)
                TriggerAction(ESCEventType.ON_MOUSE_LEFT_CLICK, _eventData);
        }

        public void OnPointerDown(PointerEventData _eventData)
        {
            if (_eventData.button == PointerEventData.InputButton.Left)
                TriggerAction(ESCEventType.ON_MOUSE_LEFT_CLICK_DOWN, _eventData);
        }

        public void OnPointerUp(PointerEventData _eventData)
        {
            if (_eventData.button == PointerEventData.InputButton.Left)
                TriggerAction(ESCEventType.ON_MOUSE_LEFT_CLICK_UP, _eventData);
        }

        public void OnBeginDrag(PointerEventData _eventData)
        {
            TriggerAction(ESCEventType.ON_BEGIN_DRAG, _eventData);
        }

        public void OnEndDrag(PointerEventData _eventData)
        {
            TriggerAction(ESCEventType.ON_END_DRAG, _eventData);
        }

        public void OnDrag(PointerEventData _eventData)
        {
            TriggerAction(ESCEventType.ON_DRAG, _eventData);
        }


        #endregion

        #region 碰撞
        private void OnCollisionEnter(Collision collision)
        {
            TriggerAction(ESCEventType.ON_COLLISION_ENTER, collision);
        }
        private void OnCollisionStay(Collision collision)
        {
            TriggerAction(ESCEventType.ON_COLLISION_STAY, collision);
        }
        private void OnCollisionExit(Collision collision)
        {
            TriggerAction(ESCEventType.ON_COLLISION_EXIT, collision);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            TriggerAction(ESCEventType.ON_COLLISION_ENTER_2D, collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            TriggerAction(ESCEventType.ON_COLLISION_STAY_2D, collision);
        }
        private void OnCollisionExit2D(Collision2D collision)
        {
            TriggerAction(ESCEventType.ON_COLLISION_EXIT_2D, collision);
        }
        #endregion

        #region 触发
        private void OnTriggerEnter(Collider other)
        {
            TriggerAction(ESCEventType.ON_TRIGGER_ENTER, other);
        }
        private void OnTriggerStay(Collider other)
        {
            TriggerAction(ESCEventType.ON_TRIGGER_STAY, other);
        }
        private void OnTriggerExit(Collider other)
        {
            TriggerAction(ESCEventType.ON_TRIGGER_EXIT, other);
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            TriggerAction(ESCEventType.ON_TRIGGER_ENTER_2D, collision);
        }
        private void OnTriggerStay2D(Collider2D collision)
        {
            TriggerAction(ESCEventType.ON_TRIGGER_STAY_2D, collision);
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            TriggerAction(ESCEventType.ON_TRIGGER_EXIT_2D, collision);
        }
        #endregion

        #region 销毁
        private void OnDestroy()
        {
            TriggerAction(ESCEventType.ON_RELEASE_ADDRESSABLE_ASSET, gameObject);
        }
        #endregion
    }
}
