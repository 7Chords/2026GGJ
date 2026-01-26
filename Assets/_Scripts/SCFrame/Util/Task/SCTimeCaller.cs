using DG.Tweening;
using System;

namespace SCFrame
{
    public class SCTimeCaller : Singleton<SCTimeCaller>
    {
        private TweenContainer _m_timeTweenContainer;

        public override void OnInitialize()
        {
            _m_timeTweenContainer = new TweenContainer();
        }

        public override void OnDiscard()
        {
            _m_timeTweenContainer?.KillAllDoTween();
            _m_timeTweenContainer = null;
        }


        public void CallDealy(float _delayTime, Action _action,int _loopCount = 1)
        {
            Tween tween = DOVirtual.DelayedCall(_delayTime, () =>
            {
                _action?.Invoke();
            }).SetLoops(_loopCount);

            _m_timeTweenContainer?.RegDoTween(tween);
        }


    }
}
