using DG.Tweening;
using System.Collections.Generic;

namespace SCFrame
{
    /// <summary>
    /// Tween容器类 discard时技能调用killall
    /// </summary>
    public class TweenContainer
    {
        private List<TweenHolder> _m_allTween = new List<TweenHolder>();

        public void RegDoTween(Tween _tween)
        {
            _m_allTween.Add(new TweenHolder(_tween));
        }

        public void KillAllDoTween()
        {
            for (var i = 0; i < _m_allTween.Count; i++)
            {
                _m_allTween[i].Kill();
            }
            _m_allTween.Clear();
        }

        public void CompleteAllDoTween()
        {
            //for (var i = 0; i < _m_allTween.Count; i++)
            //{
            //    _m_allTween[i].();
            //}
            //_m_allTween.Clear();
        }

        /// <summary>
        /// 暂停所有DoTween
        /// </summary>
        public void PauseAllDoTween()
        {
            for (var i = 0; i < _m_allTween.Count; i++)
            {
                _m_allTween[i].Pause();
            }
        }
        /// <summary>
        /// 恢复播放所有DoTween
        /// </summary>
        public void ResumeAllDoTween()
        {
            for (var i = 0; i < _m_allTween.Count; i++)
            {
                _m_allTween[i].Resume();
            }
        }

        private class TweenHolder
        {
            private Tween _m_tween;
            private bool _m_hasKill = false;

            public TweenHolder(Tween _tween)
            {
                _m_tween = _tween;
                _m_tween.OnKill(() => { _m_hasKill = true; });
            }

            public void Kill()
            {
                if (_m_hasKill == false)
                {
                    _m_tween.Kill();
                }
            }

            public void Pause()
            {
                if (_m_tween != null)
                    _m_tween.timeScale = 0;
            }

            public void Resume()
            {
                if (_m_tween != null)
                    _m_tween.timeScale = 1;
            }
        }
    }
}
