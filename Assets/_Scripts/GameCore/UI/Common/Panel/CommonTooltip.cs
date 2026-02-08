using DG.Tweening;
using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    /// <summary>
    /// ͨ��������ʾ��Mono��
    /// ������ʾ��Ʒ/���ߵ����ơ�������Ԥ����Ϣ���Դ���Ļ����Ӧλ�ã�
    /// </summary>
    public class CommonTooltip : MonoBehaviour
    {
        [Header("标题文本")]
        public Text txtName;
        [Header("描述文本")]
        public Text txtDesc;

        [Header("画布")]
        public CanvasGroup canvasGroup;
        public float fadeInDuratin = 0.2f;
        public float fadeOutDuratin = 0.2f;

        [Header("屏幕边缘间距")]
        public float screenPadding = 10f;

        private TweenContainer _m_tweenContainer;
        private RectTransform _tooltipRect;
        private RectTransform _canvasRect;

        private void Awake()
        {
            _tooltipRect = GetComponent<RectTransform>();
            _m_tweenContainer = new TweenContainer();

            _canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            if (_canvasRect == null)
            {
                Debug.LogWarning("CommonTooltipδ�ҵ�����Canvas������㼶�ṹ��");
            }
        }

        public void SetBaseInfo(string name, string desc)
        {
            if (txtName != null)
                txtName.text = string.IsNullOrEmpty(name) ? "δ֪��Ʒ" : name;

            if (txtDesc != null)
                txtDesc.text = string.IsNullOrEmpty(desc) ? "��������" : desc;
        }


        private Vector2 CalculateAdaptivePosition(Vector2 targetLocalPos)
        {
            if (_tooltipRect == null || _canvasRect == null)
            {
                Debug.LogWarning("Tooltip/Canvas RectTransformΪ�գ�����ԭʼλ��");
                return targetLocalPos;
            }


            LayoutRebuilder.ForceRebuildLayoutImmediate(_tooltipRect);

            Vector3[] corners = new Vector3[4];
            _tooltipRect.GetWorldCorners(corners);
            float tooltipWidth = corners[3].x - corners[0].x;
            float tooltipHeight = corners[1].y - corners[0].y;

            Rect canvasRect = _canvasRect.rect;
            float canvasLeft = canvasRect.xMin + screenPadding;
            float canvasRight = canvasRect.xMax - screenPadding;
            float canvasBottom = canvasRect.yMin + screenPadding;
            float canvasTop = canvasRect.yMax - screenPadding;

            Vector2 adaptivePos = targetLocalPos;

            if (adaptivePos.x + tooltipWidth > canvasRight)
            {
                adaptivePos.x = canvasRight - tooltipWidth;
            }

            if (adaptivePos.x < canvasLeft)
            {
                adaptivePos.x = canvasLeft;
            }


            if (adaptivePos.y > canvasTop)
            {
                adaptivePos.y = canvasTop - tooltipHeight;
            }

            if (adaptivePos.y - tooltipHeight < canvasBottom)
            {
                adaptivePos.y = canvasBottom + tooltipHeight;
            }

            return adaptivePos;
        }


        public void SetLocalPosition(Vector2 localPos)
        {
            if (_tooltipRect != null)
            {
                _tooltipRect.localPosition = localPos;
            }
        }

        public void Discard()
        {
            _m_tweenContainer.RegDoTween(canvasGroup.DOFade(0, fadeOutDuratin)
                .OnComplete(() =>
                {
                    SCCommon.DestoryGameObject(gameObject);
                }));
        }

        private void OnDestroy()
        {
            _m_tweenContainer?.KillAllDoTween();
            _m_tweenContainer = null;
        }


        public void ShowTooltip(string name, string desc, Vector2 targetLocalPos)
        {

            SetBaseInfo(name, desc);

            Vector2 adaptivePos = CalculateAdaptivePosition(targetLocalPos);

            SetLocalPosition(adaptivePos);

            canvasGroup.alpha = 0;
            gameObject.SetActive(true);
            _m_tweenContainer.RegDoTween(canvasGroup.DOFade(1, fadeInDuratin));
        }
    }
}