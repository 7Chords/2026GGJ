using System;

namespace SCFrame
{
    /// <summary>
    /// SCFrame计步器
    /// </summary>
    public class SCStepCounter
    {
        //所有步骤完成后的回调对象
        private Action _m_onAllStepDone;


        //总的需要的步骤数量
        private int _m_totalStepCount;
        //目前完成的步骤数量
        private int _m_curDoneStepCount;
        //是否已经完成了所有步骤
        private bool _m_isAllStepDone;

        public SCStepCounter()
        {
            _m_onAllStepDone = null;

            ResetStepInfo();
        }

        /// <summary>
        /// 重置所有步骤的统计信息
        /// </summary>
        public void ResetStepInfo()
        {
            //初始化步骤数量变量
            lock (this)
            {
                _m_totalStepCount = 0;
                _m_curDoneStepCount = 0;
                _m_isAllStepDone = false;
            }
        }

        /// <summary>
        /// 重置
        /// </summary>
        public void ResetAll()
        {
            ResetStepInfo();
            _m_onAllStepDone = null;
        }

        public bool isAllStepDone { get { return _m_isAllStepDone; } }
        public int totalStep { get { return _m_totalStepCount; } }
        public int doneStep { get { return _m_curDoneStepCount; } }

        /// <summary>
        /// 修改总的需要完成的步骤数
        /// </summary>
        /// <param name="_chgStepCount"></param>
        public void ChgTotalStepCount(int _chgStepCount)
        {
            _m_totalStepCount += _chgStepCount;
        }


        /// <summary>
        /// 增加一个已经完成的步骤数
        /// </summary>
        public void AddDoneStepCount()
        {
            Action needDealAction = null;

            lock (this)
            {
                _m_curDoneStepCount++;

                //判断步骤数量是否达到总数量，达到则需要调用加载完成的事件函数
                if (_m_curDoneStepCount == _m_totalStepCount && !_m_isAllStepDone)
                {
                    //设置判断参数
                    _m_isAllStepDone = true;
                    //获取当前需要处理的回调对象
                    needDealAction = _m_onAllStepDone;
                    _m_onAllStepDone = null;
                }
            }

            //判断是否需要执行操作
            if (null != needDealAction)
                needDealAction();
            needDealAction = null;
        }


        /// <summary>
        /// 注册监听是否全部加载完的回调函数
        /// </summary>
        /// <param name="_delegate"></param>
        public void RegAllDoneDelegate(Action _delegate)
        {
            if (null == _delegate)
                return;

            if (isAllStepDone)
                _delegate();
            else if (null == _m_onAllStepDone)
                _m_onAllStepDone = _delegate;
            else
                _m_onAllStepDone += _delegate;
        }
    }
}
