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
        private readonly List<PartInfo> _queue = new List<PartInfo>();
        private int _currentIndex = -1;
        private bool _isExecuting;
        private Action _onFinish;

        /// <summary> 当前队列（只读），用于 UI 或查询 </summary>
        public IReadOnlyList<PartInfo> Queue => _queue;

        public bool IsExecuting => _isExecuting;
        public int CurrentIndex => _currentIndex;

        /// <summary> 开始执行一组部位；onFinish 在全部执行完后回调。parts 为 null 时仅清空队列 </summary>
        public void Start(IReadOnlyList<PartInfo> parts, Action onFinish = null)
        {
            _queue.Clear();
            if (parts != null)
            {
                foreach (var p in parts)
                    if (p != null) _queue.Add(p);
            }
            _onFinish = onFinish;
            _isExecuting = parts != null && _queue.Count > 0;
            _currentIndex = -1;
        }

        /// <summary> 插入到指定索引；index 为 queue.Count 表示追加 </summary>
        public void InsertAt(int index, PartInfo part)
        {
            if (part == null) return;
            if (index < _currentIndex) return;
            index = UnityEngine.Mathf.Clamp(index, 0, _queue.Count);
            _queue.Insert(index, part);
        }

        public void AddLast(PartInfo part) => InsertAt(_queue.Count, part);

        /// <summary> 插到当前执行项后面 </summary>
        public void InsertAfterCurrent(PartInfo part) => InsertAt(_currentIndex + 1, part);

        /// <summary> 插到目标部位后面；若找不到则追加 </summary>
        public void InsertAfter(PartInfo target, PartInfo part)
        {
            int i = _queue.IndexOf(target);
            InsertAt(i < 0 ? _queue.Count : i + 1, part);
        }

        /// <summary> 按引用从队列中移除（如部位死亡） </summary>
        public bool Remove(PartInfo part)
        {
            return part != null && _queue.Remove(part);
        }

        public int IndexOf(PartInfo part) => _queue.IndexOf(part);

        /// <summary> 驱动执行下一个；返回当前要执行的 PartInfo，若已全部执行完返回 null 并触发 onFinish </summary>
        public PartInfo MoveNext()
        {
            _currentIndex++;
            if (_currentIndex >= _queue.Count)
            {
                _isExecuting = false;
                var cb = _onFinish;
                _onFinish = null;
                cb?.Invoke();
                return null;
            }
            return _queue[_currentIndex];
        }
    }
}
