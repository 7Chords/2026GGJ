using System;
using DG.Tweening;
using SCFrame;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    /// <summary>
    /// Fullscreen UI: CRT-style transition; screen copy via RenderTexture + Camera.Render (no GrabPass — avoids Canvas _GrabTexture_ST errors). Shader: Resources/Shaders/UITVSwitchTransition.
    /// </summary>
    public sealed class TVSwitchTransition : MonoBehaviour
    {
        static TVSwitchTransition _inst;

        static readonly int ScreenTexId = Shader.PropertyToID("_ScreenTex");

        Material _mat;
        Sequence _seq;
        RenderTexture _screenRT;

        [SerializeField] float closeDuration = 0.3f;
        [SerializeField] float openDuration = 0.3f;

        public static void Run(Action onMidBlack, Action onComplete = null)
        {
            Instance.RunInternal(onMidBlack, onComplete);
        }

        static TVSwitchTransition Instance
        {
            get
            {
                if (_inst == null)
                {
                    var go = new GameObject("TVSwitchTransition");
                    if (SCGame.instance != null)
                        go.transform.SetParent(SCGame.instance.transform, false);
                    else
                        DontDestroyOnLoad(go);
                    _inst = go.AddComponent<TVSwitchTransition>();
                    _inst.Build();
                }
                return _inst;
            }
        }

        void Build()
        {
            var root = gameObject;
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50000;
            canvas.overrideSorting = true;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            root.AddComponent<GraphicRaycaster>();

            var imgGo = new GameObject("TVSwitchImage");
            imgGo.transform.SetParent(root.transform, false);
            var rt = imgGo.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var img = imgGo.AddComponent<Image>();
            img.raycastTarget = true;
            img.color = Color.white;
            var tex = Texture2D.whiteTexture;
            img.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);

            Shader sh = Resources.Load<Shader>("Shaders/UITVSwitchTransition");
            if (sh == null)
                sh = Shader.Find("UI/TVSwitchTransition");
            if (sh == null)
            {
                Debug.LogError("[TVSwitchTransition] Shader UITVSwitchTransition not found (Resources/Shaders or name).");
                return;
            }

            _mat = new Material(sh);
            img.material = _mat;
            _mat.SetFloat("_CloseAmount", 0f);
            root.SetActive(false);
        }

        void LateUpdate()
        {
            if (_mat == null || !gameObject.activeInHierarchy)
                return;
            if (_mat.GetFloat("_CloseAmount") < 0.0001f)
                return;

            var cam = Camera.main;
            if (cam == null)
                return;

            EnsureScreenRT();
            var prev = cam.targetTexture;
            cam.targetTexture = _screenRT;
            cam.Render();
            cam.targetTexture = prev;
            _mat.SetTexture(ScreenTexId, _screenRT);
        }

        void EnsureScreenRT()
        {
            int w = Screen.width;
            int h = Screen.height;
            if (_screenRT != null && _screenRT.width == w && _screenRT.height == h && _screenRT.IsCreated())
                return;

            ReleaseScreenRT();
            _screenRT = new RenderTexture(w, h, 24, RenderTextureFormat.Default)
            {
                name = "TVSwitchTransitionScreen",
                antiAliasing = 1,
                filterMode = FilterMode.Bilinear
            };
            _screenRT.Create();
        }

        static void DestroyRenderTexture(ref RenderTexture rt)
        {
            if (rt == null)
                return;
            rt.Release();
            Destroy(rt);
            rt = null;
        }

        void ReleaseScreenRT()
        {
            DestroyRenderTexture(ref _screenRT);
        }

        void RunInternal(Action onMidBlack, Action onComplete)
        {
            if (_mat == null)
            {
                try { onMidBlack?.Invoke(); }
                catch (Exception e) { Debug.LogException(e); }
                try { onComplete?.Invoke(); }
                catch (Exception e) { Debug.LogException(e); }
                return;
            }

            _seq?.Kill();
            gameObject.SetActive(true);
            _mat.SetFloat("_CloseAmount", 0f);

            _seq = DOTween.Sequence();
            _seq.Append(DOTween.To(() => 0f, v => _mat.SetFloat("_CloseAmount", v), 1f, closeDuration).SetEase(Ease.InCubic).SetUpdate(true));
            _seq.AppendCallback(() =>
            {
                try { onMidBlack?.Invoke(); }
                catch (Exception e) { Debug.LogException(e); }
            });
            _seq.Append(DOTween.To(() => 1f, v => _mat.SetFloat("_CloseAmount", v), 0f, openDuration).SetEase(Ease.OutCubic).SetUpdate(true));
            _seq.OnComplete(() =>
            {
                gameObject.SetActive(false);
                try { onComplete?.Invoke(); }
                catch (Exception e) { Debug.LogException(e); }
            });
        }

        void OnDestroy()
        {
            _seq?.Kill();
            ReleaseScreenRT();
            if (_inst == this)
                _inst = null;
        }
    }
}
