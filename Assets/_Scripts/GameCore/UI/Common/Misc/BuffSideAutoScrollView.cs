using System.Collections;
using SCFrame;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    /// <summary>
    /// 与侧栏 <see cref="ScrollRect"/> 配合：内容超过视口时，复制一屏子项，持续向上（或向配置方向）滚动，
    /// 每滚过一「块」高度后把位置回绕，实现「上面移出、下面接上的」无限循环滚动态。
    /// 协程挂在 <see cref="SCTaskHelper"/> 上。
    /// </summary>
    public class BuffSideAutoScrollView : MonoBehaviour
    {
        [Header("无溢出时：留在顶部")]
        [SerializeField]
        bool resetToTopWhenNoOverflow = true;

        [Header("循环滚动：像素/秒（沿 Content 的 anchored Y 负向为「内容向上走」；若反了请勾 Invert）")]
        [SerializeField]
        float scrollSpeedPixelsPerSecond = 40f;

        [SerializeField]
        bool useUnscaledTime = true;

        [Tooltip("若滚动方向与预期相反，请勾选")]
        [SerializeField]
        bool invertScrollDirection;

        [Header("布局稳定前多等的完整帧数")]
        [SerializeField]
        int waitLayoutFrames = 1;

        [Header("子项算不出高度时的保底行高")]
        [SerializeField]
        float fallbackRowHeight = 48f;

        ScrollRect _scroll;

        void Awake()
        {
            ResolveScrollRect();
        }

        void OnDisable()
        {
            StopScrollTween();
        }

        void OnDestroy()
        {
            StopScrollTween();
        }

        void ResolveScrollRect()
        {
            if (_scroll != null)
                return;
            _scroll = GetComponent<ScrollRect>();
            if (_scroll == null)
                _scroll = GetComponentInChildren<ScrollRect>(true);
            if (_scroll == null)
                _scroll = GetComponentInParent<ScrollRect>();
            if (_scroll != null)
                _scroll.horizontal = false;
        }

        public void StopScrollTween()
        {
            if (SCTaskHelper.instance != null)
                SCTaskHelper.instance.KillAllCoroutines(this);
            if (_scroll != null)
                _scroll.enabled = true;
        }

        /// <param name="layoutRootHint">Tooltip 根，整表重算布局用。 </param>
        public void RefreshAfterItemsChanged(RectTransform layoutRootHint = null)
        {
            StopScrollTween();
            if (SCTaskHelper.instance == null)
            {
                Debug.LogWarning("[BuffSideAutoScrollView] SCTaskHelper 未初始化。");
                return;
            }
            SCTaskHelper.instance.CreateCoroutine(
                this,
                CoRunInfiniteScroll(layoutRootHint),
                "BuffSideInfiniteScroll");
        }

        IEnumerator CoRunInfiniteScroll(RectTransform layoutRootHint)
        {
            ResolveScrollRect();
            if (this == null) yield break;
            if (_scroll == null || _scroll.content == null) yield break;

            if (_scroll.viewport == null && _scroll.content.parent != null)
                _scroll.viewport = _scroll.content.parent as RectTransform;
            if (_scroll.viewport == null) yield break;

            for (int f = 0; f < Mathf.Max(0, waitLayoutFrames); f++)
                yield return null;
            yield return new WaitForEndOfFrame();

            if (this == null) yield break;
            if (layoutRootHint != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(layoutRootHint);
            RebuildRelevantLayout();

            var content = _scroll.content;
            float viewH = _scroll.viewport.rect.height;
            if (viewH < 0.1f) yield break;

            int originalCount = content.childCount;
            if (originalCount == 0) yield break;

            TryForceFirstBlockTall(content, _scroll.viewport, originalCount);
            RebuildRelevantLayout();
            if (this == null) yield break;
            Canvas.ForceUpdateCanvases();

            // 一屏高：以子物体在 content 下包围盒为准，避免纯数字累加与真实布局有偏差、循环几圈后错位露空
            float blockH = MeasureFirstBlockHeightFromLayout(content, originalCount);
            if (blockH < 1f)
                blockH = SumHeightsForFirstNChildren(content, originalCount);
            if (blockH < 1f)
                blockH = content.rect.height;

            if (blockH <= viewH + 0.5f)
            {
                if (resetToTopWhenNoOverflow)
                    _scroll.verticalNormalizedPosition = 1f;
                yield break;
            }

            // 复制一屏，形成 [原][副本]，滚过一块高后回绕
            for (int i = 0; i < originalCount; i++)
            {
                if (this == null) yield break;
                if (i >= content.childCount) break;
                var src = content.GetChild(i) as RectTransform;
                if (src == null) continue;
                var go = Object.Instantiate(src.gameObject, content);
                go.name = go.name + "_loop";
            }

            RebuildRelevantLayout();
            if (this == null) yield break;
            Canvas.ForceUpdateCanvases();
            // 两屏总高，避免 VLG/CSF 未撑开第二块
            {
                float totalH = MeasureFirstBlockHeightFromLayout(content, content.childCount);
                if (totalH < 0.1f) totalH = blockH * 2f;
                try
                {
                    content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalH);
                }
                catch (System.Exception) { }
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            }

            // 保持从顶部看第一块
            _scroll.vertical = true;
            _scroll.movementType = ScrollRect.MovementType.Clamped;
            _scroll.verticalNormalizedPosition = 1f;
            Canvas.ForceUpdateCanvases();
            if (this == null) yield break;
            if (_scroll == null) yield break;
            _scroll.enabled = false;

            // 复制后仍以「子 0 → 子 n 顶部/包围盒」量一屏实际像素，保证与 VLG 画出来的一致
            blockH = MeasureFirstBlockHeightFromLayout(content, originalCount);
            if (blockH < 1f) blockH = MeasureSpanChild0ToChildN(content, originalCount);
            if (blockH < 1f)
            {
                _scroll.enabled = true;
                yield break;
            }

            float sign = invertScrollDirection ? 1f : -1f; // 默认：Y 负向 = 内容视觉向上走
            for (;;)
            {
                if (this == null) yield break;
                if (content == null) yield break;
                if (_scroll == null) yield break;

                float fromY = content.anchoredPosition.y;
                float endY = fromY + sign * blockH;
                float dist = Mathf.Abs(blockH);
                if (dist < 0.5f) yield break;
                float duration = dist / Mathf.Max(1f, scrollSpeedPixelsPerSecond);
                float t = 0f;
                while (t < duration)
                {
                    if (this == null) yield break;
                    if (content == null) yield break;
                    float u = t / duration;
                    float y = Mathf.LerpUnclamped(fromY, endY, u);
                    var p = content.anchoredPosition;
                    p.y = y;
                    content.anchoredPosition = p;
                    t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                    yield return null;
                }
                if (content == null) yield break;
                {
                    // 先对齐到本圈结束位置，再回绕一屏，减少浮点误差
                    {
                        var p2 = content.anchoredPosition;
                        p2.y = endY;
                        content.anchoredPosition = p2;
                    }
                    var p = content.anchoredPosition;
                    p.y = fromY; // 与首屏副本齐平，无空隙
                    content.anchoredPosition = p;
                }
            }
        }

        void RebuildRelevantLayout()
        {
            if (_scroll == null) return;
            var c = _scroll.content;
            var v = _scroll.viewport;
            var s = _scroll.transform as RectTransform;
            if (c != null) LayoutRebuilder.ForceRebuildLayoutImmediate(c);
            if (v != null) LayoutRebuilder.ForceRebuildLayoutImmediate(v);
            if (s != null) LayoutRebuilder.ForceRebuildLayoutImmediate(s);
            for (int i = 0; c != null && i < c.childCount; i++)
            {
                var ch = c.GetChild(i) as RectTransform;
                if (ch != null)
                    LayoutRebuilder.ForceRebuildLayoutImmediate(ch);
            }
            Canvas.ForceUpdateCanvases();
        }

        void TryForceFirstBlockTall(RectTransform content, RectTransform viewport, int originalCount)
        {
            if (content == null || viewport == null) return;
            var vlg = content.GetComponent<VerticalLayoutGroup>();
            float paddingY = vlg != null ? vlg.padding.top + vlg.padding.bottom : 0f;
            float spacing = vlg != null ? vlg.spacing : 0f;
            float sumH = paddingY;
            for (int i = 0; i < originalCount; i++)
            {
                if (!(content.GetChild(i) is RectTransform child)) continue;
                LayoutRebuilder.ForceRebuildLayoutImmediate(child);
                float h = LayoutUtility.GetPreferredHeight(child);
                if (h < 0.1f) h = child.rect.height;
                if (h < 0.1f) h = LayoutUtility.GetMinHeight(child);
                if (h < 0.1f) h = fallbackRowHeight;
                sumH += h;
                if (i < originalCount - 1) sumH += spacing;
            }
            float vh = viewport.rect.height;
            if (sumH < vh + 2f) return;
            try
            {
                content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sumH);
            }
            catch (System.Exception) { }
            if (content.rect.height < sumH * 0.9f)
            {
                var le = content.GetComponent<LayoutElement>();
                if (le == null) le = content.gameObject.AddComponent<LayoutElement>();
                le.minHeight = sumH;
                le.preferredHeight = sumH;
            }
        }

        /// <summary> 第一块（前 n 个子在 content 下的并集）在 y 向长度，和布局器画出来一致。 </summary>
        static float MeasureFirstBlockHeightFromLayout(RectTransform content, int n)
        {
            if (content == null || n <= 0) return 0f;
            n = Mathf.Min(n, content.childCount);
            if (n == 0) return 0f;
            float minY = float.MaxValue, maxY = float.MinValue;
            for (int i = 0; i < n; i++)
            {
                if (!(content.GetChild(i) is RectTransform ch))
                    continue;
                Bounds b = RectTransformUtility.CalculateRelativeRectTransformBounds(content, ch);
                if (b.size == Vector3.zero) continue;
                minY = Mathf.Min(minY, b.min.y);
                maxY = Mathf.Max(maxY, b.max.y);
            }
            if (minY == float.MaxValue) return 0f;
            return maxY - minY;
        }

        /// <summary> 用第 0 个与第 n 个子（第二块头）的相对位置作为一屏滚动量。 </summary>
        static float MeasureSpanChild0ToChildN(RectTransform content, int n)
        {
            if (content == null || n <= 0 || n >= content.childCount)
                return 0f;
            var c0 = content.GetChild(0) as RectTransform;
            var cN = content.GetChild(n) as RectTransform;
            if (c0 == null || cN == null) return 0f;
            Bounds b0 = RectTransformUtility.CalculateRelativeRectTransformBounds(content, c0);
            Bounds bN = RectTransformUtility.CalculateRelativeRectTransformBounds(content, cN);
            return Mathf.Abs(b0.max.y - bN.max.y);
        }

        static float SumHeightsForFirstNChildren(RectTransform content, int n)
        {
            if (content == null || n <= 0) return 0f;
            n = Mathf.Min(n, content.childCount);
            var vlg = content.GetComponent<VerticalLayoutGroup>();
            float t = 0f;
            if (vlg != null)
            {
                t = vlg.padding.top + vlg.padding.bottom;
                t += (n - 1) * vlg.spacing;
            }
            for (int i = 0; i < n; i++)
            {
                if (content.GetChild(i) is RectTransform ch)
                {
                    float h = LayoutUtility.GetPreferredHeight(ch);
                    if (h < 0.1f) h = ch.rect.height;
                    t += h;
                }
            }
            return t;
        }
    }
}
