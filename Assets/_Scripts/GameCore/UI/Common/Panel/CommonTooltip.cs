using DG.Tweening;
using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    /// <summary>
    /// 通用悬浮提示框Mono类
    /// 用于显示商品/道具的名称、描述等预览信息（自带屏幕自适应位置）
    /// </summary>
    public class CommonTooltip : MonoBehaviour
    {
        [Header("悬浮框核心UI组件")]
        public Text txtName;      // 商品名称文本
        public Text txtDesc;      // 商品描述文本

        [Header("渐变参数")]
        public CanvasGroup canvasGroup;
        public float fadeInDuratin = 0.2f;
        public float fadeOutDuratin = 0.2f;

        [Header("屏幕适配参数")]
        [Tooltip("悬浮框与屏幕边缘的最小距离")]
        public float screenPadding = 10f;

        // 缓存组件
        private TweenContainer _m_tweenContainer;
        private RectTransform _tooltipRect;
        private RectTransform _canvasRect; // 顶层Canvas的RectTransform

        private void Awake()
        {
            // 初始化缓存组件
            _tooltipRect = GetComponent<RectTransform>();
            _m_tweenContainer = new TweenContainer();

            // 获取顶层Canvas的RectTransform（适配Overlay模式）
            _canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            if (_canvasRect == null)
            {
                Debug.LogWarning("CommonTooltip未找到父级Canvas，请检查层级结构！");
            }
        }

        /// <summary>
        /// 设置悬浮框显示的基础信息（名称+描述）
        /// </summary>
        /// <param name="name">商品名称</param>
        /// <param name="desc">商品描述</param>
        public void SetBaseInfo(string name, string desc)
        {
            if (txtName != null)
                txtName.text = string.IsNullOrEmpty(name) ? "未知商品" : name;

            if (txtDesc != null)
                txtDesc.text = string.IsNullOrEmpty(desc) ? "暂无描述" : desc;
        }

        /// <summary>
        /// 计算自适应位置（核心：确保不超出屏幕）
        /// </summary>
        /// <param name="targetLocalPos">目标本地坐标（Canvas下）</param>
        /// <returns>适配后的本地坐标</returns>
        private Vector2 CalculateAdaptivePosition(Vector2 targetLocalPos)
        {
            if (_tooltipRect == null || _canvasRect == null)
            {
                Debug.LogWarning("Tooltip/Canvas RectTransform为空，返回原始位置");
                return targetLocalPos;
            }

            // 强制刷新布局（避免动态文本导致尺寸计算错误）
            LayoutRebuilder.ForceRebuildLayoutImmediate(_tooltipRect);

            // 获取Tooltip的实际宽高（通过世界角点计算，适配任意锚点/缩放）
            Vector3[] corners = new Vector3[4];
            _tooltipRect.GetWorldCorners(corners);
            float tooltipWidth = corners[3].x - corners[0].x;
            float tooltipHeight = corners[1].y - corners[0].y;

            // 获取Canvas的可视区域范围（Overlay模式下等于屏幕分辨率）
            Rect canvasRect = _canvasRect.rect;
            float canvasLeft = canvasRect.xMin + screenPadding;
            float canvasRight = canvasRect.xMax - screenPadding;
            float canvasBottom = canvasRect.yMin + screenPadding;
            float canvasTop = canvasRect.yMax - screenPadding;

            // 计算Tooltip的目标位置（初始为传入位置）
            Vector2 adaptivePos = targetLocalPos;

            // ========== 水平方向适配 ==========
            // 右边界超出：向左调整
            if (adaptivePos.x + tooltipWidth > canvasRight)
            {
                adaptivePos.x = canvasRight - tooltipWidth;
            }
            // 左边界超出：向右调整
            if (adaptivePos.x < canvasLeft)
            {
                adaptivePos.x = canvasLeft;
            }

            // ========== 垂直方向适配 ==========
            // 上边界超出：向下调整
            if (adaptivePos.y > canvasTop)
            {
                adaptivePos.y = canvasTop - tooltipHeight;
            }
            // 下边界超出：向上调整
            if (adaptivePos.y - tooltipHeight < canvasBottom)
            {
                adaptivePos.y = canvasBottom + tooltipHeight;
            }

            return adaptivePos;
        }

        /// <summary>
        /// 设置悬浮框的显示位置（本地坐标）
        /// </summary>
        /// <param name="localPos">Canvas下的本地坐标</param>
        public void SetLocalPosition(Vector2 localPos)
        {
            if (_tooltipRect != null)
            {
                _tooltipRect.localPosition = localPos;
            }
        }

        /// <summary>
        /// 隐藏并销毁悬浮框
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
        /// 快速设置并显示悬浮框（一键调用，自动适配屏幕位置）
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="desc">描述</param>
        /// <param name="targetLocalPos">目标本地坐标（Canvas下）</param>
        public void ShowTooltip(string name, string desc, Vector2 targetLocalPos)
        {
            // 1. 设置基础信息
            SetBaseInfo(name, desc);

            // 2. 计算自适应位置（核心：避免超出屏幕）
            Vector2 adaptivePos = CalculateAdaptivePosition(targetLocalPos);

            // 3. 设置适配后的位置
            SetLocalPosition(adaptivePos);

            // 4. 渐变显示
            canvasGroup.alpha = 0;
            gameObject.SetActive(true); // 确保激活后再做渐变
            _m_tweenContainer.RegDoTween(canvasGroup.DOFade(1, fadeInDuratin));
        }
    }
}