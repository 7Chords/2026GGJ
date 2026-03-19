using DG.Tweening;
using SCFrame;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class CommonPopTip : MonoBehaviour
    {
        [Header("Tip文本")]
        public Text tipText;
        [Header("画布组件")]
        public CanvasGroup canvasGroup;
        [Header("变大持续时间")]
        public float biggerDuration;
        [Header("上浮持续时间")]
        public float floatDuration;
        [Header("淡出持续时间")]
        public float fadeOutDuration;
        [Header("上浮高度")]
        public float floatHeight;
        [Header("静止持续时间")]
        public float stopDuration;
        [Header("振动强度")]
        public float shakeStrength;
        [Header("振动持续时间")]
        public float shakeDuration;

        private TweenContainer _m_tweenContainer;
        private Vector3 _originalPosition;

        public void Initialize(string _content)
        {
            _m_tweenContainer = new TweenContainer();
            _originalPosition = gameObject.GetRectTransform().localPosition;


            tipText.text = _content;

            //初始状态
            gameObject.GetRectTransform().localScale = Vector3.zero;
            canvasGroup.alpha = 1f;


            //组合动画序列
            Sequence mainSequence = DOTween.Sequence();
            if (biggerDuration > 0)
            {
                //创建放大动画
                Tween biggerTween = gameObject.GetRectTransform().DOScale(Vector3.one, biggerDuration);
                mainSequence.Append(biggerTween);
            }
            if (shakeDuration > 0 && shakeStrength > 0)
            {
                //创建抖动动画
                Tween shakeTween = gameObject.GetRectTransform().DOShakePosition(shakeDuration, shakeStrength);
                mainSequence.Join(shakeTween); // 与放大动画同时执行
            }

            //第二阶段：静止一段时间
            mainSequence.AppendInterval(stopDuration);

            if (floatHeight > 0 && floatDuration > 0)
            {

                //创建上浮动画
                Tween floatTween = gameObject.GetRectTransform().DOLocalMoveY(
                    _originalPosition.y + floatHeight, floatDuration);
                //第三阶段：同时执行上浮和淡出
                mainSequence.Append(floatTween);
            }


            if (fadeOutDuration > 0)
            {
                //创建淡出动画
                Tween fadeOutTween = canvasGroup.DOFade(0, fadeOutDuration).OnComplete(() =>
                {
                    SCCommon.DestoryGameObject(gameObject);
                });


                mainSequence.Join(fadeOutTween); // 与上浮动画同时执行
            }

            _m_tweenContainer.RegDoTween(mainSequence);
        }

        private void OnDestroy()
        {
            _m_tweenContainer?.KillAllDoTween();
            _m_tweenContainer = null;
        }
    }

}
