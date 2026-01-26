using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SCFrame
{
    /// <summary>
    /// 状态机拥有者接口
    /// </summary>
    public interface IStateMachineOwner { }

    /// <summary>
    /// SCFrame状态机
    /// </summary>
    public class StateMachine : _ASCLifeObjBase
    {
        // 当前状态
        public int currStateType { get; private set; } = -1;

        // 当前生效中的状态
        private StateBase _m_currStateObj;

        // 宿主
        private IStateMachineOwner _m_owner;

        // 所有的状态 Key:状态枚举的值 Value:具体的状态
        private Dictionary<int, StateBase> _m_stateDic;

        public override void OnInitialize()
        {
            _m_stateDic = new Dictionary<int, StateBase>();
        }

        public override void OnDiscard()
        {
            // 处理当前状态的额外逻辑
            _m_currStateObj.Exit();
            _m_currStateObj.RemoveUpdate(_m_currStateObj.Update);
            _m_currStateObj.RemoveLateUpdate(_m_currStateObj.LateUpdate);
            _m_currStateObj.RemoveFixedUpdate(_m_currStateObj.FixedUpdate);
            currStateType = -1;
            _m_currStateObj = null;
            // 处理缓存中所有状态的逻辑
            var enumerator = _m_stateDic.GetEnumerator();
            while (enumerator.MoveNext())
            {
                enumerator.Current.Value.Discard();
            }
            _m_stateDic.Clear();
            // 放弃所有资源的引用
            _m_owner = null;
            // 放进对象池
            this.ObjectPushPool();
        }

        public override void OnResume()
        {
            _m_currStateObj.Enter();
            _m_currStateObj.RemoveUpdate(_m_currStateObj.Update);
            _m_currStateObj.RemoveLateUpdate(_m_currStateObj.LateUpdate);
            _m_currStateObj.RemoveFixedUpdate(_m_currStateObj.FixedUpdate);
        }

        public override void OnSuspend()
        {
            _m_currStateObj.Exit();
            _m_currStateObj.RemoveUpdate(_m_currStateObj.Update);
            _m_currStateObj.RemoveLateUpdate(_m_currStateObj.LateUpdate);
            _m_currStateObj.RemoveFixedUpdate(_m_currStateObj.FixedUpdate);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="_owner">宿主</param>
        public void SetOwner(IStateMachineOwner _owner)
        {
            _m_owner = _owner;
        }
        public IStateMachineOwner GetOwner()
        {
            return _m_owner;
        }

        /// <summary>
        /// 切换状态
        /// </summary>
        /// <typeparam name="T">具体要切换到的状态脚本类型</typeparam>
        /// <param name="newState">新状态</param>
        /// <param name="_reCurrstate">新状态和当前状态一致的情况下，是否也要切换</param>
        /// <returns></returns>
        public bool ChangeState<T>(int _newStateType, bool _reCurrstate = false) where T : StateBase, new()
        {
            // 状态一致，并且不需要刷新状态，则切换失败
            if (_newStateType == currStateType && !_reCurrstate) return false;

            // 退出当前状态
            if (_m_currStateObj != null)
            {
                _m_currStateObj.Exit();
                _m_currStateObj.RemoveUpdate(_m_currStateObj.Update);
                _m_currStateObj.RemoveLateUpdate(_m_currStateObj.LateUpdate);
                _m_currStateObj.RemoveFixedUpdate(_m_currStateObj.FixedUpdate);
            }
            // 进入新状态
            _m_currStateObj = GetState<T>(_newStateType);
            currStateType = _newStateType;
            _m_currStateObj.Enter();
            _m_currStateObj.OnUpdate(_m_currStateObj.Update);
            _m_currStateObj.OnLateUpdate(_m_currStateObj.LateUpdate);
            _m_currStateObj.OnFixedUpdate(_m_currStateObj.FixedUpdate);

            return true;
        }

        /// <summary>
        /// 从对象池获取一个状态
        /// </summary>
        private StateBase GetState<T>(int _stateType) where T : StateBase, new()
        {
            if (_m_stateDic.ContainsKey(_stateType)) return _m_stateDic[_stateType];
            StateBase state = SCPoolMgr.instance.GetObject<T>();
            state.Init(this);
            _m_stateDic.Add(_stateType, state);
            return state;
        }
    }
}

