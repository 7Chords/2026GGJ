
namespace SCFrame
{
    public abstract class _ASCLifeObjBase : ISCLifecycle
    {
        private bool m_hasInitialized;
        private bool m_hasDiscarded;
        private bool m_isRunning;

        public bool hasInitialized { get => m_hasInitialized; }
        public bool hasDiscarded { get => m_hasDiscarded; }
        public bool isRunning { get => m_isRunning; }

        //简单地对入口和实现进行包装
        public void Initialize()
        {
            m_hasInitialized = true;
            m_hasDiscarded = false;
            m_isRunning = true;

            OnInitialize();
        }
        public abstract void OnInitialize();

        public void Discard()
        {
            m_isRunning = false;
            m_hasDiscarded = true;
            OnDiscard();
        }
        public abstract void OnDiscard();

        public void Resume()
        {
            m_isRunning = true;
            OnResume();
        }
        public abstract void OnResume();

        public void Suspend()
        {
            m_isRunning = false;
            OnSuspend();
        }
        public abstract void OnSuspend();
    }
}