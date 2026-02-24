using System;
using System.Collections.Generic;

namespace GameCore.Battle
{
    /// <summary>
    /// 单方部位执行队列：支持按序执行、任意位置插入、按引用删除。
    /// 与具体战斗流程解耦，只负责队列与“执行下一个”的驱动。
    /// </summary>
    public class BattlePartExecutionQueue
    {
        private List<PartInfo> _m_queue = new List<PartInfo>();
        private int _m_currentIndex = -1;
        private bool _m_isExecuting;
        private Action _m_onFinish;

        /// <summary> 当前队列（只读），用于 UI 或查询 </summary>
        public List<PartInfo> queue => _m_queue;

        public bool isExecuting => _m_isExecuting;
        public int currentIndex => _m_currentIndex;

        /// <summary> 开始执行一组部位；onFinish 在全部执行完后回调。parts 为 null 时仅清空队列 </summary>
        public void Start(List<PartInfo> _parts, Action _onFinish = null)
        {
            _m_queue.Clear();
            if (_parts != null)
            {
                foreach (var p in _parts)
                    if (p != null) _m_queue.Add(p);
            }
            //_m_queue = _parts;
            _m_onFinish = _onFinish;
            _m_isExecuting = _parts != null && _m_queue.Count > 0;
            _m_currentIndex = -1;
        }

        /// <summary> 插入到指定索引；index 为 queue.Count 表示追加 </summary>
        public void InsertAt(int _index, PartInfo _part)
        {
            if (_part == null) return;
            if (_index < _m_currentIndex) return;
            int index = UnityEngine.Mathf.Clamp(_index, 0, _m_queue.Count);
            _m_queue.Insert(index, _part);
        }

        public void AddLast(PartInfo _part) => InsertAt(_m_queue.Count, _part);

        /// <summary> 插到当前执行项后面 </summary>
        public void InsertAfterCurrent(PartInfo _part) => InsertAt(_m_currentIndex + 1, _part);

        /// <summary> 插到目标部位后面；若找不到则追加 </summary>
        public void InsertAfter(PartInfo _target, PartInfo _part)
        {
            int i = _m_queue.IndexOf(_target);
            InsertAt(i < 0 ? _m_queue.Count : i + 1, _part);
        }

        /// <summary> 按引用从队列中移除（如部位死亡） </summary>
        public bool Remove(PartInfo _part)
        {
            return _part != null && _m_queue.Remove(_part);
        }

        public int IndexOf(PartInfo _part) => _m_queue.IndexOf(_part);

        /// <summary> 驱动执行下一个；返回当前要执行的 PartInfo，若已全部执行完返回 null 并触发 onFinish </summary>
        public PartInfo MoveNext()
        {
            _m_currentIndex++;
            if (_m_currentIndex >= _m_queue.Count)
            {
                _m_isExecuting = false;
                var cb = _m_onFinish;
                _m_onFinish = null;
                cb?.Invoke();
                return null;
            }
            return _m_queue[_m_currentIndex];
        }
    }
}
