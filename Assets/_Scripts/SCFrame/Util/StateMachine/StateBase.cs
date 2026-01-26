namespace SCFrame
{
    /// <summary>
    /// 状态机状态基类
    /// </summary>
    public abstract class StateBase
    {
        protected StateMachine stateMachine;


        /// <summary>
        /// 初始化状态
        /// 只在状态第一次创建时执行
        /// </summary>
        /// <param name="_owner">宿主</param>
        /// <param name="stateType">状态类型枚举的值</param>
        /// <param name="_stateMachine">所属状态机</param>
        public virtual void Init(StateMachine _stateMachine)
        {
            stateMachine = _stateMachine;
        }

        /// <summary>
        /// 反初始化
        /// </summary>
        public virtual void Discard()
        {
            // 放回对象池
            this.ObjectPushPool();
        }

        public virtual void Enter() 
        {
            OnEnter();
        }
        public virtual void Exit()
        {
            OnExit();
        }
        public virtual void Update() 
        {
            OnUpdate();
        }
        public virtual void LateUpdate() 
        {
            OnLateUpdate();
        }
        public virtual void FixedUpdate()
        {
            OnFixedUpdate();
        }





        public abstract void OnEnter();
        public abstract void OnExit();

        public abstract void OnUpdate();
        public abstract void OnLateUpdate();
        public abstract void OnFixedUpdate();

    }
}
