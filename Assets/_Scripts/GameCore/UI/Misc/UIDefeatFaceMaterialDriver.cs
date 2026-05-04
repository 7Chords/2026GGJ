using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    /// <summary>
    /// Drives UI/DefeatFace on a face Image: load shader from Resources/Shaders/UIDefeatFace, optional progress tween.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Graphic))]
    public sealed class UIDefeatFaceMaterialDriver : MonoBehaviour
    {
        public const string ShaderResourcePath = "Shaders/UIDefeatFace";
        public const string ShaderName = "UI/DefeatFace";
        public static readonly int ProgressId = Shader.PropertyToID("_Progress");

        [SerializeField] Graphic _targetGraphic;
        [SerializeField] [Range(0f, 1f)] float _progress;

        Material _instanceMat;
        bool _ownsMaterial;

        void Awake()
        {
            if (_targetGraphic == null)
                _targetGraphic = GetComponent<Graphic>();
        }

        void OnDestroy()
        {
            if (_ownsMaterial && _instanceMat != null)
                Destroy(_instanceMat);
        }

        void OnValidate()
        {
            if (_targetGraphic == null)
                _targetGraphic = GetComponent<Graphic>();
            var m = _targetGraphic != null ? _targetGraphic.material : null;
            if (m != null && m.shader != null && m.shader.name == ShaderName && m.HasProperty(ProgressId))
                m.SetFloat(ProgressId, Mathf.Clamp01(_progress));
        }

        /// <summary> 0 = normal, 1 = full defeat look. </summary>
        public float Progress
        {
            get => _progress;
            set
            {
                _progress = Mathf.Clamp01(value);
                ApplyProgressToGraphic(_progress);
            }
        }

        public void EnsureDefeatMaterial()
        {
            if (_targetGraphic == null)
                _targetGraphic = GetComponent<Graphic>();
            if (_targetGraphic == null)
                return;

            Material m = _targetGraphic.material;
            if (m != null && m.shader != null && m.shader.name == ShaderName)
            {
                if (_ownsMaterial && _instanceMat != null && _targetGraphic.material == _instanceMat)
                {
                    ApplyProgressToGraphic(_progress);
                    return;
                }

                if (_ownsMaterial && _instanceMat != null)
                    Destroy(_instanceMat);
                _instanceMat = new Material(m);
                _ownsMaterial = true;
                _targetGraphic.material = _instanceMat;
                ApplyProgressToGraphic(_progress);
                return;
            }

            var sh = Resources.Load<Shader>(ShaderResourcePath);
            if (sh == null)
                sh = Shader.Find(ShaderName);
            if (sh == null)
            {
                Debug.LogError("[UIDefeatFaceMaterialDriver] Shader not found: " + ShaderResourcePath + " / " + ShaderName);
                return;
            }

            if (_ownsMaterial && _instanceMat != null)
                Destroy(_instanceMat);

            _instanceMat = new Material(sh);
            _ownsMaterial = true;
            _targetGraphic.material = _instanceMat;
            ApplyProgressToGraphic(_progress);
        }

        public void ResetToDefaultMaterial()
        {
            if (_targetGraphic == null)
                _targetGraphic = GetComponent<Graphic>();
            if (_ownsMaterial && _instanceMat != null)
            {
                Destroy(_instanceMat);
                _instanceMat = null;
                _ownsMaterial = false;
            }

            if (_targetGraphic != null)
                _targetGraphic.material = null;
        }

        public IEnumerator CoAnimateProgress(float from, float to, float duration, AnimationCurve curve = null)
        {
            EnsureDefeatMaterial();
            if (duration <= 0f)
            {
                Progress = to;
                yield break;
            }

            curve ??= AnimationCurve.Linear(0f, 0f, 1f, 1f);
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / duration);
                float k = curve.Evaluate(u);
                Progress = Mathf.Lerp(from, to, k);
                yield return null;
            }

            Progress = to;
        }

        void ApplyProgressToGraphic(float p)
        {
            if (_targetGraphic == null)
                return;
            var m = _targetGraphic.material;
            if (m != null && m.shader != null && m.shader.name == ShaderName && m.HasProperty(ProgressId))
                m.SetFloat(ProgressId, Mathf.Clamp01(p));
        }
    }
}
