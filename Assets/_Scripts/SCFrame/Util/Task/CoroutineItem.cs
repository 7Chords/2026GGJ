using System.Collections;
using UnityEngine;

namespace SCFrame
{
    /// <summary>
    /// 协程项，用于管理单个协程的生命周期
    /// </summary>
    public class CoroutineItem
    {
        public object owner { get; private set; }
        public IEnumerator enumerator { get; private set; }
        public string coroutineId { get; private set; }
        public bool isRunning { get; private set; }

        private Coroutine _m_unityCoroutine;

        public CoroutineItem(object _owner, IEnumerator _enumerator, string _coroutineId)
        {
            this.owner = _owner;
            this.enumerator = _enumerator;
            this.coroutineId = _coroutineId;
            isRunning = false;
        }

        /// <summary>
        /// 启动协程
        /// </summary>
        public void Start()
        {
            if (isRunning || SCTaskHelper.instance == null)
                return;

            isRunning = true;
            _m_unityCoroutine = SCTaskHelper.instance.StartCoroutine(RunWrapper());
        }

        /// <summary>
        /// 运行协程的迭代器
        /// </summary>
        private IEnumerator RunWrapper()
        {
            yield return enumerator;
            isRunning = false;
            SCTaskHelper.instance?.KillCoroutine(coroutineId);
            //while (isRunning && enumerator != null)
            //{
            //    if (enumerator == null) break;
            //    if (enumerator.MoveNext())
            //    {
            //        yield return enumerator.Current;
            //    }
            //    else
            //    {
            //        // 协程自然结束
            //        isRunning = false;
            //        SCTaskHelper.instance?.KillCoroutine(coroutineId);
            //        break;
            //    }
            //}
        }

        /// <summary>
        /// 停止协程
        /// </summary>
        public void Stop()
        {
            isRunning = false;
            if (_m_unityCoroutine != null && SCTaskHelper.instance != null)
            {
                SCTaskHelper.instance.StopCoroutine(_m_unityCoroutine);
            }
            enumerator = null;
        }
    }
}