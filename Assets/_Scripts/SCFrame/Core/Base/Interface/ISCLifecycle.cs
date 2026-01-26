namespace SCFrame
{
    /// <summary>
    /// 框架中具有手动控制生命周期的接口
    /// </summary>
    public interface ISCLifecycle
    {
        bool hasInitialized { get; }
        bool hasDiscarded { get; }
        bool isRunning { get; }

        //初始化入口
        void Initialize(); 

        //初始化实现方法
        void OnInitialize();

        // 销毁入口
        void Discard();

        // 销毁实现方法
        void OnDiscard();

        //恢复入口
        void Resume();
        //恢复实现方法
        void OnResume();
        //挂起入口
        void Suspend();
        //挂起实现方法
        void OnSuspend();
    }
}
