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
        [Header("���������UI���")]
        public Text txtName;      // ��Ʒ�����ı�
        public Text txtDesc;      // ��Ʒ�����ı�

        [Header("�������")]
        public CanvasGroup canvasGroup;
        public float fadeInDuratin = 0.2f;
        public float fadeOutDuratin = 0.2f;

        [Header("��Ļ�������")]
        [Tooltip("����������Ļ��Ե����С����")]
        public float screenPadding = 10f;

        // �������
        private TweenContainer _m_tweenContainer;
        private RectTransform _tooltipRect;
        private RectTransform _canvasRect; // ����Canvas��RectTransform

        private void Awake()
        {
            // ��ʼ���������
            _tooltipRect = GetComponent<RectTransform>();
            _m_tweenContainer = new TweenContainer();

            // ��ȡ����Canvas��RectTransform������Overlayģʽ��
            _canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            if (_canvasRect == null)
            {
                Debug.LogWarning("CommonTooltipδ�ҵ�����Canvas������㼶�ṹ��");
            }
        }

        /// <summary>
        /// ������������ʾ�Ļ�����Ϣ������+������
        /// </summary>
        /// <param name="name">��Ʒ����</param>
        /// <param name="desc">��Ʒ����</param>
        public void SetBaseInfo(string name, string desc)
        {
            if (txtName != null)
                txtName.text = string.IsNullOrEmpty(name) ? "δ֪��Ʒ" : name;

            if (txtDesc != null)
                txtDesc.text = string.IsNullOrEmpty(desc) ? "��������" : desc;
        }

        /// <summary>
        /// ��������Ӧλ�ã����ģ�ȷ����������Ļ��
        /// </summary>
        /// <param name="targetLocalPos">Ŀ�걾�����꣨Canvas�£�</param>
        /// <returns>�����ı�������</returns>
        private Vector2 CalculateAdaptivePosition(Vector2 targetLocalPos)
        {
            if (_tooltipRect == null || _canvasRect == null)
            {
                Debug.LogWarning("Tooltip/Canvas RectTransformΪ�գ�����ԭʼλ��");
                return targetLocalPos;
            }

            // ǿ��ˢ�²��֣����⶯̬�ı����³ߴ�������
            LayoutRebuilder.ForceRebuildLayoutImmediate(_tooltipRect);

            // ��ȡTooltip��ʵ�ʿ��ߣ�ͨ������ǵ���㣬��������ê��/���ţ�
            Vector3[] corners = new Vector3[4];
            _tooltipRect.GetWorldCorners(corners);
            float tooltipWidth = corners[3].x - corners[0].x;
            float tooltipHeight = corners[1].y - corners[0].y;

            // ��ȡCanvas�Ŀ�������Χ��Overlayģʽ�µ�����Ļ�ֱ��ʣ�
            Rect canvasRect = _canvasRect.rect;
            float canvasLeft = canvasRect.xMin + screenPadding;
            float canvasRight = canvasRect.xMax - screenPadding;
            float canvasBottom = canvasRect.yMin + screenPadding;
            float canvasTop = canvasRect.yMax - screenPadding;

            // ����Tooltip��Ŀ��λ�ã���ʼΪ����λ�ã�
            Vector2 adaptivePos = targetLocalPos;

            // ========== ˮƽ�������� ==========
            // �ұ߽糬�����������
            if (adaptivePos.x + tooltipWidth > canvasRight)
            {
                adaptivePos.x = canvasRight - tooltipWidth;
            }
            // ��߽糬�������ҵ���
            if (adaptivePos.x < canvasLeft)
            {
                adaptivePos.x = canvasLeft;
            }

            // ========== ��ֱ�������� ==========
            // �ϱ߽糬�������µ���
            if (adaptivePos.y > canvasTop)
            {
                adaptivePos.y = canvasTop - tooltipHeight;
            }
            // �±߽糬�������ϵ���
            if (adaptivePos.y - tooltipHeight < canvasBottom)
            {
                adaptivePos.y = canvasBottom + tooltipHeight;
            }

            return adaptivePos;
        }

        /// <summary>
        /// �������������ʾλ�ã��������꣩
        /// </summary>
        /// <param name="localPos">Canvas�µı�������</param>
        public void SetLocalPosition(Vector2 localPos)
        {
            if (_tooltipRect != null)
            {
                _tooltipRect.localPosition = localPos;
            }
        }

        /// <summary>
        /// ���ز�����������
        /// </summary>
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

        /// <summary>
        /// �������ò���ʾ������һ�����ã��Զ�������Ļλ�ã�
        /// </summary>
        /// <param name="name">����</param>
        /// <param name="desc">����</param>
        /// <param name="targetLocalPos">Ŀ�걾�����꣨Canvas�£�</param>
        public void ShowTooltip(string name, string desc, Vector2 targetLocalPos)
        {
            // 1. ���û�����Ϣ
            SetBaseInfo(name, desc);

            // 2. ��������Ӧλ�ã����ģ����ⳬ����Ļ��
            Vector2 adaptivePos = CalculateAdaptivePosition(targetLocalPos);

            // 3. ����������λ��
            SetLocalPosition(adaptivePos);

            // 4. ������ʾ
            canvasGroup.alpha = 0;
            gameObject.SetActive(true); // ȷ���������������
            _m_tweenContainer.RegDoTween(canvasGroup.DOFade(1, fadeInDuratin));
        }
    }
}