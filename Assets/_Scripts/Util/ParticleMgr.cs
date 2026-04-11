using System.Collections;
using Coffee.UIExtensions;
using SCFrame;
using UnityEngine;

namespace GameCore
{
    /// <summary>
    /// UIParticle helpers: load prefabs via ResourcesHelper (Addressables), play/stop/clear, optional one-shot with auto destroy.
    /// </summary>
    public class ParticleMgr : Singleton<ParticleMgr>
    {
        public override void OnDiscard()
        {
            SCTaskHelper.instance.KillAllCoroutines(this);
        }

        /// <summary>
        /// Instantiate prefab and return UIParticle (searches children). Returns null if load fails or component missing.
        /// </summary>
        public UIParticle LoadUIParticle(string _assetName, Transform _parent = null, bool _automaticRelease = true)
        {
            Transform p = _parent != null ? _parent : GetDefaultParent();
            GameObject go = ResourcesHelper.LoadGameObject(_assetName, p, _automaticRelease);
            if (go == null)
                return null;

            UIParticle uip = go.GetComponentInChildren<UIParticle>(true);
            if (uip == null)
            {
                SCDebugHelper.LogError("ParticleMgr: UIParticle not found on prefab '" + _assetName + "'");
            }

            return uip;
        }

        /// <summary>
        /// Load and play once. Returns UIParticle instance or null.
        /// </summary>
        public UIParticle Play(string _assetName, Transform _parent = null, bool _automaticRelease = true)
        {
            UIParticle uip = LoadUIParticle(_assetName, _parent, _automaticRelease);
            if (uip == null)
                return null;
            Play(uip);
            return uip;
        }

        /// <summary>
        /// Load, set anchored position on root RectTransform (if any), then play.
        /// </summary>
        public UIParticle Play(string _assetName, Transform _parent, Vector2 _anchoredPosition, bool _automaticRelease = true)
        {
            UIParticle uip = LoadUIParticle(_assetName, _parent, _automaticRelease);
            if (uip == null)
                return null;

            RectTransform rt = uip.transform as RectTransform;
            if (rt != null)
                rt.anchoredPosition = _anchoredPosition;

            Play(uip);
            return uip;
        }

        public void Play(UIParticle _uiParticle)
        {
            if (_uiParticle == null)
                return;
            _uiParticle.gameObject.SetActive(true);
            _uiParticle.Play();
        }

        public void Stop(UIParticle _uiParticle, bool _alsoClear = false)
        {
            if (_uiParticle == null)
                return;
            _uiParticle.Stop();
            if (_alsoClear)
                _uiParticle.Clear();
        }

        public void Pause(UIParticle _uiParticle)
        {
            if (_uiParticle == null)
                return;
            _uiParticle.Pause();
        }

        public void Resume(UIParticle _uiParticle)
        {
            if (_uiParticle == null)
                return;
            _uiParticle.Resume();
        }

        public void Clear(UIParticle _uiParticle)
        {
            if (_uiParticle == null)
                return;
            _uiParticle.Clear();
        }

        /// <summary>
        /// Load, play, then destroy the instance after delay.
        /// If destroyAfterSeconds &lt; 0, duration is estimated from ParticleSystem modules (looping effects skip auto-destroy unless you pass a positive time).
        /// Returns coroutine id for <see cref="CancelOneShot"/>, or null if no destroy was scheduled.
        /// </summary>
        public string PlayOneShot(string _assetName, Transform _parent = null, float _destroyAfterSeconds = -1f,
            bool _automaticRelease = true)
        {
            UIParticle uip = LoadUIParticle(_assetName, _parent, _automaticRelease);
            if (uip == null)
                return null;

            Play(uip);

            float delay = _destroyAfterSeconds >= 0f ? _destroyAfterSeconds : EstimateDestroyDelay(uip);
            if (delay < 0f)
                return null;

            return SCTaskHelper.instance.CreateCoroutine(this, CoDestroyAfter(uip.gameObject, delay), "ParticleOneShot");
        }

        /// <summary>
        /// Same as <see cref="PlayOneShot(string, Transform, float, bool)"/>, but sets anchored position on root RectTransform (if any) before playing.
        /// </summary>
        public string PlayOneShot(string _assetName, Transform _parent, Vector2 _anchoredPosition, float _destroyAfterSeconds = -1f,
            bool _automaticRelease = true)
        {
            UIParticle uip = LoadUIParticle(_assetName, _parent, _automaticRelease);
            if (uip == null)
                return null;

            RectTransform rt = uip.transform as RectTransform;
            if (rt != null)
                rt.anchoredPosition = _anchoredPosition;

            Play(uip);

            float delay = _destroyAfterSeconds >= 0f ? _destroyAfterSeconds : EstimateDestroyDelay(uip);
            if (delay < 0f)
                return null;

            return SCTaskHelper.instance.CreateCoroutine(this, CoDestroyAfter(uip.gameObject, delay), "ParticleOneShot");
        }

        public void CancelOneShot(string _coroutineId)
        {
            if (string.IsNullOrEmpty(_coroutineId))
                return;
            SCTaskHelper.instance.KillCoroutine(_coroutineId);
        }

        public void DestroyEffect(GameObject _effectRoot)
        {
            if (_effectRoot == null)
                return;
            SCCommon.DestoryGameObject(_effectRoot);
        }

        static Transform GetDefaultParent()
        {
            if (SCGame.instance != null && SCGame.instance.topLayerRoot != null)
                return SCGame.instance.topLayerRoot.transform;
            return null;
        }

        static IEnumerator CoDestroyAfter(GameObject _go, float _delay)
        {
            yield return new WaitForSeconds(_delay);
            if (_go != null)
                SCCommon.DestoryGameObject(_go);
        }

        /// <summary>
        /// Returns seconds until safe destroy, or -1 if looping (caller should pass explicit lifetime).
        /// </summary>
        static float EstimateDestroyDelay(UIParticle _uip)
        {
            if (_uip == null || _uip.particles == null || _uip.particles.Count == 0)
                return 2f;

            bool anyLoop = false;
            float maxTotal = 0f;

            for (int i = 0; i < _uip.particles.Count; i++)
            {
                ParticleSystem ps = _uip.particles[i];
                if (ps == null)
                    continue;

                ParticleSystem.MainModule main = ps.main;
                if (main.loop)
                    anyLoop = true;

                float duration = main.duration;
                float lifeUpper = GetStartLifetimeUpper(main);
                maxTotal = Mathf.Max(maxTotal, duration + lifeUpper);
            }

            if (anyLoop)
                return -1f;

            return maxTotal > 0.01f ? maxTotal + 0.25f : 2f;
        }

        static float GetStartLifetimeUpper(ParticleSystem.MainModule _main)
        {
            ParticleSystem.MinMaxCurve life = _main.startLifetime;
            switch (life.mode)
            {
                case ParticleSystemCurveMode.Constant:
                    return life.constant;
                case ParticleSystemCurveMode.TwoConstants:
                    return Mathf.Max(life.constantMin, life.constantMax);
                case ParticleSystemCurveMode.Curve:
                    return life.curveMultiplier * life.curveMax.Evaluate(1f);
                case ParticleSystemCurveMode.TwoCurves:
                    return life.curveMultiplier *
                           Mathf.Max(life.curveMax.Evaluate(1f), life.curveMin.Evaluate(1f));
                default:
                    return life.constantMax;
            }
        }
    }
}
