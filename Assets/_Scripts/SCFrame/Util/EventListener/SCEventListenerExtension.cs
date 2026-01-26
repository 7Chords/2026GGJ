using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SCFrame
{
    /// <summary>
    /// SCFrame事件监听拓展
    /// </summary>
    public static class SCEventListenerExtension
    {
        #region 工具函数
        private static SCEventListener GetOrAddSCEventListener(Component _com)
        {
            SCEventListener lis = _com.GetComponent<SCEventListener>();
            if (lis == null) return _com.gameObject.AddComponent<SCEventListener>();
            else return lis;
        }
        public static void AddEventListener<T>(this Component _com, ESCEventType _eventType, Action<T, object[]> _action, params object[] _args)
        {
            AddEventListener(_com, (int)_eventType, _action, _args);
        }
        public static void AddEventListener<T>(this Component _com, int _customEventTypeInt, Action<T, object[]> _action, params object[] _args)
        {
            SCEventListener lis = GetOrAddSCEventListener(_com);
            lis.AddListener(_customEventTypeInt, _action, _args);
        }
        public static void RemoveEventListener<T>(this Component _com, int _customEventTypeInt, Action<T, object[]> _action, bool _checkArgs = false, params object[] _args)
        {
            SCEventListener lis = GetOrAddSCEventListener(_com);
            lis.RemoveListener(_customEventTypeInt, _action, _checkArgs, _args);
        }
        public static void RemoveEventListener<T>(this Component _com, ESCEventType _eventType, Action<T, object[]> _action, bool _checkArgs = false, params object[] _args)
        {
            RemoveEventListener(_com, (int)_eventType, _action, _checkArgs, _args);
        }
        public static void RemoveAllListener(this Component _com, int _customEventTypeInt)
        {
            SCEventListener lis = GetOrAddSCEventListener(_com);
            lis.RemoveAllListener(_customEventTypeInt);
        }
        public static void RemoveAllListener(this Component _com, ESCEventType _eventType)
        {
            RemoveAllListener(_com, (int)_eventType);
        }
        public static void RemoveAllListener(this Component _com)
        {
            SCEventListener lis = GetOrAddSCEventListener(_com);
            lis.RemoveAllListener();
        }
        public static void TriggerCustomEvent<T>(this Component _com, int _customEventTypeInt, T _eventData)
        {
            SCEventListener lis = GetOrAddSCEventListener(_com);
            lis.TriggerAction<T>(_customEventTypeInt, _eventData);
        }
        #endregion

        #region 鼠标相关事件
        public static void AddMouseEnter(this Component _com, Action<PointerEventData, object[]> _action, params object[] _args)
        {
            AddEventListener(_com, ESCEventType.ON_MOUSE_ENTER, _action, _args);
        }
        public static void AddMouseExit(this Component _com, Action<PointerEventData, object[]> _action, params object[] _args)
        {
            AddEventListener(_com, ESCEventType.ON_MOUSE_EXIT, _action, _args);
        }
        public static void AddMouseLeftClick(this Component _com, Action<PointerEventData, object[]> _action, params object[] _args)
        {
            AddEventListener(_com, ESCEventType.ON_MOUSE_LEFT_CLICK, _action, _args);
        }
        public static void AddMouseLeftClickDown(this Component _com, Action<PointerEventData, object[]> _action, params object[] _args)
        {
            AddEventListener(_com, ESCEventType.ON_MOUSE_LEFT_CLICK_DOWN, _action, _args);
        }
        public static void AddMouseLeftClickUp(this Component _com, Action<PointerEventData, object[]> _action, params object[] _args)
        {
            AddEventListener(_com, ESCEventType.ON_MOUSE_LEFT_CLICK_UP, _action, _args);
        }
        public static void AddDrag(this Component _com, Action<PointerEventData, object[]> _action, params object[] _args)
        {
            AddEventListener(_com, ESCEventType.ON_DRAG, _action, _args);
        }
        public static void AddBeginDrag(this Component _com, Action<PointerEventData, object[]> _action, params object[] _args)
        {
            AddEventListener(_com, ESCEventType.ON_BEGIN_DRAG, _action, _args);
        }
        public static void AddEndDrag(this Component _com, Action<PointerEventData, object[]> _action, params object[] _args)
        {
            AddEventListener(_com, ESCEventType.ON_END_DRAG, _action, _args);
        }
        public static void RemoveClick(this Component _com, Action<PointerEventData, object[]> _action, bool _checkArgs = false, params object[] _args)
        {
            RemoveEventListener(_com, ESCEventType.ON_MOUSE_LEFT_CLICK, _action, _checkArgs, _args);
        }
        public static void RemoveClickDown(this Component _com, Action<PointerEventData, object[]> _action, bool _checkArgs = false, params object[] _args)
        {
            RemoveEventListener(_com, ESCEventType.ON_MOUSE_LEFT_CLICK_DOWN, _action, _checkArgs, _args);
        }
        public static void RemoveClickUp(this Component _com, Action<PointerEventData, object[]> _action, bool _checkArgs = false, params object[] _args)
        {
            RemoveEventListener(_com, ESCEventType.ON_MOUSE_LEFT_CLICK_UP, _action, _checkArgs, _args);
        }
        public static void RemoveDrag(this Component _com, Action<PointerEventData, object[]> _action, bool _checkArgs = false, params object[] _args)
        {
            RemoveEventListener(_com, ESCEventType.ON_DRAG, _action, _checkArgs, _args);
        }
        public static void RemoveBeginDrag(this Component _com, Action<PointerEventData, object[]> _action, bool _checkArgs = false, params object[] _args)
        {
            RemoveEventListener(_com, ESCEventType.ON_BEGIN_DRAG, _action, _checkArgs, _args);
        }
        public static void RemoveEndDrag(this Component _com, Action<PointerEventData, object[]> _action, bool _checkArgs = false, params object[] _args)
        {
            RemoveEventListener(_com, ESCEventType.ON_END_DRAG, _action, _checkArgs, _args);
        }
        public static void RemoveMouseEnter(this Component _com, Action<PointerEventData, object[]> _action, bool _checkArgs = false, params object[] _args)
        {
            RemoveEventListener(_com, ESCEventType.ON_MOUSE_ENTER, _action, _checkArgs, _args);
        }

        public static void RemoveMouseExit(this Component _com, Action<PointerEventData, object[]> _action, bool _checkArgs = false, params object[] _args)
        {
            RemoveEventListener(_com, ESCEventType.ON_MOUSE_EXIT, _action, _checkArgs, _args);
        }

        #endregion

        #region 碰撞相关事件

        public static void AddCollisionEnter(this Component _com, Action<Collision, object[]> _action, params object[] _args)
        {
            _com.AddEventListener(ESCEventType.ON_COLLISION_ENTER, _action, _args);
        }


        public static void AddCollisionStay(this Component _com, Action<Collision, object[]> _action, params object[] _args)
        {
            AddEventListener(_com, ESCEventType.ON_COLLISION_STAY, _action, _args);
        }
        public static void AddCollisionExit(this Component _com, Action<Collision, object[]> _action, params object[] _args)
        {
            AddEventListener(_com, ESCEventType.ON_COLLISION_EXIT, _action, _args);
        }
        public static void AddCollisionEnter2D(this Component _com, Action<Collision, object[]> _action, params object[] _args)
        {
            AddEventListener(_com, ESCEventType.ON_COLLISION_ENTER_2D, _action, _args);
        }
        public static void AddCollisionStay2D(this Component _com, Action<Collision, object[]> _action, params object[] _args)
        {
            AddEventListener(_com, ESCEventType.ON_COLLISION_STAY_2D, _action, _args);
        }
        public static void AddCollisionExit2D(this Component _com, Action<Collision, object[]> _action, params object[] _args)
        {
            AddEventListener(_com, ESCEventType.ON_COLLISION_EXIT_2D, _action, _args);
        }
        public static void RemoveCollisionEnter(this Component _com, Action<Collision, object[]> _action, bool _checkArgs = false, params object[] _args)
        {
            RemoveEventListener(_com, ESCEventType.ON_COLLISION_ENTER, _action, _checkArgs, _args);
        }
        public static void RemoveCollisionStay(this Component _com, Action<Collision, object[]> _action, bool _checkArgs = false, params object[] _args)
        {
            RemoveEventListener(_com, ESCEventType.ON_COLLISION_STAY, _action, _checkArgs, _args);
        }
        public static void RemoveCollisionExit(this Component _com, Action<Collision, object[]> _action, bool _checkArgs = false, params object[] _args)
        {
            RemoveEventListener(_com, ESCEventType.ON_COLLISION_EXIT, _action, _checkArgs, _args);
        }
        public static void RemoveCollisionEnter2D(this Component _com, Action<Collision2D, object[]> _action, bool _checkArgs = false, params object[] _args)
        {
            RemoveEventListener(_com, ESCEventType.ON_COLLISION_ENTER_2D, _action, _checkArgs, _args);
        }
        public static void RemoveCollisionStay2D(this Component _com, Action<Collision2D, object[]> _action, bool _checkArgs = false, params object[] _args)
        {
            RemoveEventListener(_com, ESCEventType.ON_COLLISION_STAY_2D, _action, _checkArgs, _args);
        }
        public static void RemoveCollisionExit2D(this Component _com, Action<Collision2D, object[]> _action, bool _checkArgs = false, params object[] _args)
        {
            RemoveEventListener(_com, ESCEventType.ON_COLLISION_EXIT_2D, _action, _checkArgs, _args);
        }
        #endregion

        #region 触发相关事件
        public static void AddTriggerEnter(this Component _com, Action<Collider, object[]> _action, bool _checkArgs = false, params object[] _args)
        {
            AddEventListener(_com, ESCEventType.ON_TRIGGER_ENTER, _action, _checkArgs, _args);
        }
        public static void AddTriggerStay(this Component _com, Action<Collider, object[]> _action, bool _checkArgs = false, params object[] _args)
        {
            AddEventListener(_com, ESCEventType.ON_TRIGGER_STAY, _action, _checkArgs, _args);
        }
        public static void AddTriggerExit(this Component _com, Action<Collider, object[]> _action, bool _checkArgs = false, params object[] _args)
        {
            AddEventListener(_com, ESCEventType.ON_TRIGGER_EXIT, _action, _checkArgs, _args);
        }
        public static void AddTriggerEnter2D(this Component _com, Action<Collider, object[]> _action, bool _checkArgs = false, params object[] _args)
        {
            AddEventListener(_com, ESCEventType.ON_TRIGGER_ENTER_2D, _action, _checkArgs, _args);
        }
        public static void AddTriggerStay2D(this Component _com, Action<Collider, object[]> _action, bool _checkArgs = false, params object[] _args)
        {
            AddEventListener(_com, ESCEventType.ON_TRIGGER_STAY_2D, _action, _checkArgs, _args);
        }
        public static void AddTriggerExit2D(this Component _com, Action<Collider, object[]> _action, bool _checkArgs = false, params object[] _args)
        {
            AddEventListener(_com, ESCEventType.ON_TRIGGER_EXIT_2D, _action, _checkArgs, _args);
        }
        public static void RemoveTriggerEnter(this Component _com, Action<Collider, object[]> _action, bool _checkArgs = false, params object[] _args)
        {
            RemoveEventListener(_com, ESCEventType.ON_TRIGGER_ENTER, _action, _checkArgs, _args);
        }
        public static void RemoveTriggerStay(this Component _com, Action<Collider, object[]> _action, bool _checkArgs = false, params object[] _args)
        {
            RemoveEventListener(_com, ESCEventType.ON_TRIGGER_STAY, _action, _checkArgs, _args);
        }
        public static void RemoveTriggerExit(this Component _com, Action<Collider, object[]> _action, bool _checkArgs = false, params object[] _args)
        {
            RemoveEventListener(_com, ESCEventType.ON_TRIGGER_EXIT, _action, _checkArgs, _args);
        }
        public static void RemoveTriggerEnter2D(this Component _com, Action<Collider2D, object[]> _action, bool _checkArgs = false, params object[] _args)
        {
            RemoveEventListener(_com, ESCEventType.ON_TRIGGER_ENTER_2D, _action, _checkArgs, _args);
        }
        public static void RemoveTriggerStay2D(this Component _com, Action<Collider2D, object[]> _action, bool _checkArgs = false, params object[] _args)
        {
            RemoveEventListener(_com, ESCEventType.ON_TRIGGER_STAY_2D, _action, _checkArgs, _args);
        }
        public static void RemoveTriggerExit2D(this Component _com, Action<Collider2D, object[]> _action, bool _checkArgs = false, params object[] _args)
        {
            RemoveEventListener(_com, ESCEventType.ON_TRIGGER_EXIT_2D, _action, _checkArgs, _args);
        }
        #endregion

        #region 资源相关事件
        public static void AddReleaseAddressableAsset(this Component _com, Action<GameObject, object[]> _action, params object[] _args)
        {
            AddEventListener(_com, ESCEventType.ON_RELEASE_ADDRESSABLE_ASSET, _action, _args);
        }
        public static void RemoveReleaseAddressableAsset(this Component _com, Action<GameObject, object[]> _action, bool _checkArgs = false, params object[] _args)
        {
            RemoveEventListener(_com, ESCEventType.ON_RELEASE_ADDRESSABLE_ASSET, _action, _checkArgs, _args);
        }
        #endregion
    }
}