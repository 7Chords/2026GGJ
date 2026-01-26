using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCFrame
{
    /// <summary>
    /// SCFrame任务帮助器 为非Mono提供Unity生命周期 跳帧执行方法 携程控制方法等
    /// </summary>
    public class SCTaskHelper : SingletonPersistent<SCTaskHelper>
    {
        private Action _m_updateEvent;
        private Action _m_lateUpdateEvent;
        private Action _m_fixedUpdateEvent;

        // 用于存储需要在下一帧执行的方法
        private Queue<Action> _m_nextUpdateActionQueue;
        private Queue<Action> _m_nextLateUpdateActionQueue;
        private Queue<Action> _m_nextFixedUpdateActionQueue;

        private TweenContainer _m_tweenContainer;

        // 协程相关字段
        private Dictionary<string, CoroutineItem> _m_coroutineDict;
        private Dictionary<object, List<string>> _m_ownerCoroutineMap;
        private long _m_coroutineIdCounter;
        public override void OnInitialize()
        {
            _m_nextUpdateActionQueue = new Queue<Action>();
            _m_nextLateUpdateActionQueue = new Queue<Action>();
            _m_nextFixedUpdateActionQueue = new Queue<Action>();
            _m_tweenContainer = new TweenContainer();

            // 初始化协程相关字段
            _m_coroutineDict = new Dictionary<string, CoroutineItem>();
            _m_ownerCoroutineMap = new Dictionary<object, List<string>>();
            _m_coroutineIdCounter = 0;
        }

        public override void OnDiscard()
        {
            _m_updateEvent = null;
            _m_lateUpdateEvent = null;
            _m_fixedUpdateEvent = null;
            _m_nextUpdateActionQueue.Clear();
            _m_nextLateUpdateActionQueue.Clear();
            _m_nextFixedUpdateActionQueue.Clear();
            _m_nextUpdateActionQueue = null;
            _m_nextLateUpdateActionQueue = null;
            _m_nextFixedUpdateActionQueue = null;

            _m_tweenContainer?.KillAllDoTween();
            _m_tweenContainer = null;

            // 清理所有协程
            ClearAllCoroutines();
            _m_coroutineDict = null;
            _m_ownerCoroutineMap = null;
        }


        // =============== 监听相关方法 ===============

        /// <summary>
        /// 添加Update监听
        /// </summary>
        /// <param name="_action"></param>
        public void AddUpdateListener(Action _action)
        {
            _m_updateEvent += _action;
        }
        /// <summary>
        /// 移除Update监听
        /// </summary>
        /// <param name="_action"></param>
        public void RemoveUpdateListener(Action _action)
        {
            _m_updateEvent -= _action;
        }

        /// <summary>
        /// 添加LateUpdate监听
        /// </summary>
        /// <param name="_action"></param>
        public void AddLateUpdateListener(Action _action)
        {
            _m_lateUpdateEvent += _action;
        }
        /// <summary>
        /// 移除LateUpdate监听
        /// </summary>
        /// <param name="_action"></param>
        public void RemoveLateUpdateListener(Action _action)
        {
            _m_lateUpdateEvent -= _action;
        }

        /// <summary>
        /// 添加FixedUpdate监听
        /// </summary>
        /// <param name="_action"></param>
        public void AddFixedUpdateListener(Action _action)
        {
            _m_fixedUpdateEvent += _action;
        }
        /// <summary>
        /// 移除FixedUpdate监听
        /// </summary>
        /// <param name="_action"></param>
        public void RemoveFixedUpdateListener(Action _action)
        {
            _m_fixedUpdateEvent -= _action;
        }

        public void ClearAllUpdateListener()
        {
            _m_updateEvent = null;
        }
        public void ClearAllFixedUpdateListener()
        {
            _m_fixedUpdateEvent = null;
        }
        public void ClearAllLateUpdateListener()
        {
            _m_lateUpdateEvent = null;
        }

        private void Update()
        {
            _m_updateEvent?.Invoke();
            if (_m_nextUpdateActionQueue == null) return;
            executeQueuedActions(_m_nextUpdateActionQueue);
        }
        private void LateUpdate()
        {
            _m_lateUpdateEvent?.Invoke();
            if (_m_nextLateUpdateActionQueue == null) return;
            executeQueuedActions(_m_nextLateUpdateActionQueue);

        }
        private void FixedUpdate()
        {
            _m_fixedUpdateEvent?.Invoke();
            if (_m_nextFixedUpdateActionQueue == null) return;
            executeQueuedActions(_m_nextFixedUpdateActionQueue);
        }

        /// <summary>
        /// 在update下一帧执行
        /// </summary>
        /// <param name="_action"></param>
        public void DoInNextUpdate(Action _action)
        {
            if (_action != null)
            {
                _m_nextUpdateActionQueue.Enqueue(_action);
            }
        }

        /// <summary>
        /// 在fixedupdate下一帧执行
        /// </summary>
        /// <param name="_action"></param>
        public void DoInNextFixedUpdate(Action _action)
        {
            if (_action != null)
            {
                _m_nextFixedUpdateActionQueue.Enqueue(_action);
            }
        }

        /// <summary>
        /// 在lateupdate下一帧执行
        /// </summary>
        /// <param name="_action"></param>
        public void DoInNextLateUpdate(Action _action)
        {
            if (_action != null)
            {
                _m_nextLateUpdateActionQueue.Enqueue(_action);
            }
        }

        public void DoDelay(Action _action, float _delay)
        {
            if (_action == null)
                return;

            Tween tween = DOTween.Sequence().AppendInterval(_delay).OnComplete(() =>
            {
                _action.Invoke();
            });
            _m_tweenContainer?.RegDoTween(tween);

        }

        /// <summary>
        /// 执行队列中的所有方法并清空队列
        /// </summary>
        /// <param name="_actionsQueue">方法队列</param>
        private void executeQueuedActions(Queue<Action> _actionsQueue)
        {
            if (_actionsQueue.Count > 0)
            {
                // 先复制队列内容，避免在执行过程中修改队列
                var actionsToExecute = new List<Action>(_m_nextUpdateActionQueue);
                _actionsQueue.Clear();

                // 执行所有方法
                foreach (var action in actionsToExecute)
                {
                    try
                    {
                        action?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"SCTaskHelper:Error executing queued action: {ex.Message}");
                    }
                }
            }
        }

        // =============== 协程相关方法 ===============

        /// <summary>
        /// 启动协程
        /// </summary>
        /// <param name="_owner">协程拥有者，用于生命周期管理</param>
        /// <param name="_enumerator">协程迭代器</param>
        /// <param name="_coroutineName">协程名称（可选）</param>
        /// <returns>协程ID</returns>
        public string CreateCoroutine(object _owner, IEnumerator _enumerator, string _coroutineName = null)
        {
            if (_owner == null || _enumerator == null)
            {
                Debug.LogError("SCTaskHelper: Owner or enumerator is null!");
                return null;
            }

            string coroutineId = generateCoroutineId(_coroutineName);

            // 创建协程项
            var coroutineItem = new CoroutineItem(_owner, _enumerator, coroutineId);
            _m_coroutineDict[coroutineId] = coroutineItem;

            // 建立拥有者映射
            if (!_m_ownerCoroutineMap.ContainsKey(_owner))
            {
                _m_ownerCoroutineMap[_owner] = new List<string>();
            }
            _m_ownerCoroutineMap[_owner].Add(coroutineId);

            // 启动协程
            coroutineItem.Start();

            return coroutineId;
        }

        /// <summary>
        /// 停止指定协程
        /// </summary>
        /// <param name="_coroutineId">协程ID</param>
        public void KillCoroutine(string _coroutineId)
        {
            if (string.IsNullOrEmpty(_coroutineId) || !_m_coroutineDict.ContainsKey(_coroutineId))
                return;

            var coroutineItem = _m_coroutineDict[_coroutineId];
            coroutineItem.Stop();

            removeCoroutineInternal(_coroutineId, coroutineItem.owner);
        }

        /// <summary>
        /// 停止指定拥有者的所有协程
        /// </summary>
        /// <param name="_owner">协程拥有者</param>
        public void KillAllCoroutines(object _owner)
        {
            if (_owner == null || !_m_ownerCoroutineMap.ContainsKey(_owner))
                return;

            var coroutineIds = new List<string>(_m_ownerCoroutineMap[_owner]);
            foreach (var coroutineId in coroutineIds)
            {
                if (_m_coroutineDict.ContainsKey(coroutineId))
                {
                    _m_coroutineDict[coroutineId].Stop();
                }
                removeCoroutineInternal(coroutineId, _owner);
            }
        }

        /// <summary>
        /// 停止所有协程
        /// </summary>
        public void ClearAllCoroutines()
        {
            foreach (var coroutineItem in _m_coroutineDict.Values)
            {
                coroutineItem.Stop();
            }

            _m_coroutineDict.Clear();
            _m_ownerCoroutineMap.Clear();
        }

        /// <summary>
        /// 检查协程是否正在运行
        /// </summary>
        /// <param name="_coroutineId">协程ID</param>
        /// <returns>是否正在运行</returns>
        public bool IsCoroutineRunning(string _coroutineId)
        {
            return !string.IsNullOrEmpty(_coroutineId) && _m_coroutineDict.ContainsKey(_coroutineId);
        }

        /// <summary>
        /// 生成唯一的协程ID
        /// </summary>
        /// <param name="_name">自定义名称</param>
        /// <returns>协程ID</returns>
        private string generateCoroutineId(string _name = null)
        {
            _m_coroutineIdCounter++;
            string id = string.IsNullOrEmpty(_name) ?
                $"Coroutine_{_m_coroutineIdCounter}" :
                $"{_name}_{_m_coroutineIdCounter}";

            // 确保ID唯一
            while (_m_coroutineDict.ContainsKey(id))
            {
                _m_coroutineIdCounter++;
                id = string.IsNullOrEmpty(_name) ?
                    $"Coroutine_{_m_coroutineIdCounter}" :
                    $"{_name}_{_m_coroutineIdCounter}";
            }

            return id;
        }

        /// <summary>
        /// 移除协程（内部使用）
        /// </summary>
        /// <param name="_coroutineId">协程ID</param>
        /// <param name="_owner">拥有者</param>
        private void removeCoroutineInternal(string _coroutineId, object _owner)
        {
            _m_coroutineDict.Remove(_coroutineId);

            // 清理拥有者映射
            if (_m_ownerCoroutineMap.ContainsKey(_owner))
            {
                _m_ownerCoroutineMap[_owner].Remove(_coroutineId);
                if (_m_ownerCoroutineMap[_owner].Count == 0)
                {
                    _m_ownerCoroutineMap.Remove(_owner);
                }
            }
        }
    }
}