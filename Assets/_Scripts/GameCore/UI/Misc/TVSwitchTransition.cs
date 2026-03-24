using System;
using DG.Tweening;
using SCFrame;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    /// <summary>
    /// Fullscreen UI: two black bars (top-down + bottom-up) meet at center, then open. Uses shader Resources/Shaders/UITVSwitchTransition.
    /// </summary>
    public sealed class TVSwitchTransition : MonoBehaviour
    {
        static TVSwitchTransition _inst;

        Material _mat;
        Sequence _seq;

        [SerializeField] float closeDuration = 0.6f;
        [SerializeField] float openDuration = 0.6f;

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
            _seq.Append(DOTween.To(() => 0f, v => _mat.SetFloat("_CloseAmount", v), 1f, closeDuration).SetEase(Ease.InQuad).SetUpdate(true));
            _seq.AppendCallback(() =>
            {
                try { onMidBlack?.Invoke(); }
                catch (Exception e) { Debug.LogException(e); }
            });
            _seq.Append(DOTween.To(() => 1f, v => _mat.SetFloat("_CloseAmount", v), 0f, openDuration).SetEase(Ease.OutQuad).SetUpdate(true));
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
            if (_inst == this)
                _inst = null;
        }
    }
}
