using DG.Tweening;
using SCFrame;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    /// <summary>
    /// TBS伤害飘字
    /// </summary>
    public class DamageFloatText : MonoBehaviour
    {
        [Header("伤害文本")]
        public Text damageText;
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
        [Header("是伤害文本的颜色")]
        public Color colorIsDamage;
        [Header("是治疗文本的颜色")]
        public Color colorIsHeal;

        private TweenContainer _m_tweenContainer;
        private Vector3 _originalPosition;

        public void Initialize(int _damage, bool _isDamageText = true)
        {
            _m_tweenContainer = new TweenContainer();
            _originalPosition = gameObject.GetRectTransform().localPosition;

            if (_isDamageText)
            {
                damageText.text = "-" + _damage.ToString();
                damageText.color = colorIsDamage;
            }
            else
            {
                damageText.text = "+" + _damage.ToString();
                damageText.color = colorIsHeal;
            }
            //初始状态
            gameObject.GetRectTransform().localScale = Vector3.zero;
            canvasGroup.alpha = 1f;

            //创建放大动画
            Tween biggerTween = gameObject.GetRectTransform().DOScale(Vector3.one, biggerDuration);

            //创建抖动动画
            Tween shakeTween = gameObject.GetRectTransform().DOShakePosition(shakeDuration, shakeStrength);

            //创建上浮动画
            Tween floatTween = gameObject.GetRectTransform().DOLocalMoveY(
                _originalPosition.y + floatHeight, floatDuration);

            //创建淡出动画
            Tween fadeOutTween = canvasGroup.DOFade(0, fadeOutDuration).OnComplete(() =>
            {
                SCCommon.DestoryGameObject(gameObject);
            });

            //组合动画序列
            Sequence mainSequence = DOTween.Sequence();

            //第一阶段：同时执行放大和抖动
            mainSequence.Append(biggerTween);
            mainSequence.Join(shakeTween); // 与放大动画同时执行

            //第二阶段：静止一段时间
            mainSequence.AppendInterval(stopDuration);

            //第三阶段：同时执行上浮和淡出
            mainSequence.Append(floatTween);
            mainSequence.Join(fadeOutTween); // 与上浮动画同时执行

            _m_tweenContainer.RegDoTween(mainSequence);
        }

        private void OnDestroy()
        {
            _m_tweenContainer?.KillAllDoTween();
            _m_tweenContainer = null;
        }
    }
}