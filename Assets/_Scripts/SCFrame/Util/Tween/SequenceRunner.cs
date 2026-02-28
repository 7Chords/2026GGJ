using DG.Tweening;
using System;

namespace SCFrame
{
    public class SequenceRunner
    {
        private Sequence _m_seq;

        public SequenceRunner()
        {
            _m_seq = DOTween.Sequence();
        }
        public void AddTask(float _dealy, Action _callback)
        {
            if (_m_seq == null)
                return;
            _m_seq.AppendInterval(_dealy);
            _m_seq.AppendCallback(
                ()=>
                {
                    _callback?.Invoke();
                });
        }

        public void Kill()
        {
            _m_seq?.Kill();
        }
    }

}