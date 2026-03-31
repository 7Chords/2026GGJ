using DG.Tweening;
using SCFrame;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    /// <summary> Lightweight hover intro (title + body ). Loaded under topLayerRoot via GameCommon. </summary>
    public class CommonIntroTip : MonoBehaviour
    {
        [Header("Title (optional)")]
        public Text txtTitle;
        public GameObject goTitleRow;

        [Header("Body")]
        public Text txtDesc;

        [Header("Canvas")]
        public CanvasGroup canvasGroup;
        public float fadeInDuration = 0.15f;
        public float fadeOutDuration = 0.12f;

        [Header("Screen edge padding")]
        public float screenPadding = 10f;

        private TweenContainer _m_tweenContainer;
        private RectTransform _m_rect;
        private RectTransform _m_canvasRect;

        private void Awake()
        {
            _m_rect = GetComponent<RectTransform>();
            _m_tweenContainer = new TweenContainer();
            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
                _m_canvasRect = canvas.GetComponent<RectTransform>();
        }

        private void OnDestroy()
        {
            _m_tweenContainer?.KillAllDoTween();
            _m_tweenContainer = null;
        }

        public void Show(string title, string desc, Vector2 localPosInTopLayer)
        {
            bool hasTitle = !string.IsNullOrEmpty(title);
            if (goTitleRow != null)
                SCCommon.SetGameObjectEnable(goTitleRow, hasTitle);
            if (txtTitle != null)
                txtTitle.text = hasTitle ? title : string.Empty;
            if (txtDesc != null)
                txtDesc.text = desc ?? string.Empty;

            Vector2 pos = calculateAdaptivePosition(localPosInTopLayer);
            if (_m_rect != null)
                _m_rect.localPosition = pos;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                _m_tweenContainer?.KillAllDoTween();
                _m_tweenContainer.RegDoTween(canvasGroup.DOFade(1f, fadeInDuration));
            }
        }

        public void Discard()
        {
            if (canvasGroup == null)
            {
                SCCommon.DestoryGameObject(gameObject);
                return;
            }
            _m_tweenContainer?.KillAllDoTween();
            _m_tweenContainer.RegDoTween(canvasGroup.DOFade(0f, fadeOutDuration)
                .OnComplete(() => SCCommon.DestoryGameObject(gameObject)));
        }

        private Vector2 calculateAdaptivePosition(Vector2 targetLocal)
        {
            if (_m_rect == null || _m_canvasRect == null)
                return targetLocal;

            LayoutRebuilder.ForceRebuildLayoutImmediate(_m_rect);

            Vector3[] corners = new Vector3[4];
            _m_rect.GetWorldCorners(corners);
            float w = corners[3].x - corners[0].x;
            float h = corners[1].y - corners[0].y;

            Rect cr = _m_canvasRect.rect;
            float left = cr.xMin + screenPadding;
            float right = cr.xMax - screenPadding;
            float bottom = cr.yMin + screenPadding;
            float top = cr.yMax - screenPadding;

            Vector2 p = targetLocal;
            if (p.x + w > right)
                p.x = right - w;
            if (p.x < left)
                p.x = left;
            if (p.y > top)
                p.y = top - h;
            if (p.y - h < bottom)
                p.y = bottom + h;
            return p;
        }
    }
}
