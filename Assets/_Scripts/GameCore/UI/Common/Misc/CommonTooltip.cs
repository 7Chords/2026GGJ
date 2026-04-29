using DG.Tweening;
using GameCore;
using GameCore.Helpers;
using GameCore.RefData;
using SCFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameCore.UI
{
    public class CommonTooltip : MonoBehaviour
    {
        [Header("标题文本")]
        public Text txtName;
        [Header("描述文本")]
        public Text txtDesc;
        [Header("品质文本")]
        public Text txtQuality;
        [Header("等级文本")]
        public Text txtLevel;

        [Header("画布")]
        public CanvasGroup canvasGroup;
        public float fadeInDuratin = 0.2f;
        public float fadeOutDuratin = 0.2f;

        [Header("屏幕边缘间距")]
        public float screenPadding = 10f;

        [Header("各个信息物体")]
        public GameObject goName;
        public GameObject goDesc;
        public GameObject goGrid;
        public GameObject goBuff;

        [Header("格子信息父物体")]
        public GameObject tranParentGrid;

        [Header("buff信息父物体")]
        public GameObject tranParentBuff;

        [Header("buff侧边栏信息父物体")]
        public GameObject tranParentBuffSideInfo;

        private TweenContainer _m_tweenContainer;
        private RectTransform _m_tooltipRect;
        private RectTransform _m_canvasRect;

        private void Awake()
        {
            _m_tooltipRect = GetComponent<RectTransform>();
            _m_tweenContainer = new TweenContainer();

            _m_canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
            if (_m_canvasRect == null)
            {
                Debug.LogWarning("CommonTooltip上不存在rectTran组件");
            }
        }

        private void OnDestroy()
        {
            _m_tweenContainer?.KillAllDoTween();
            _m_tweenContainer = null;
        }

        private void setBaseInfo(string _name,string _desc, int _level,EQualityType _quality = EQualityType.NONE)
        {
            if (txtName != null)
                txtName.text = string.IsNullOrEmpty(_name) ? "默认部位" : _name;

            if (txtDesc != null)
                txtDesc.text = string.IsNullOrEmpty(_desc) ? "默认部位描述" : _desc;

            if (txtQuality != null)
            {
                switch (_quality)
                {
                    case EQualityType.NONE:
                        txtQuality.text = "无效品质";
                        break;
                    case EQualityType.NORMAL:
                        txtQuality.text = "普通";
                        break;
                    case EQualityType.RARE:
                        txtQuality.text = "稀有";
                        break;
                    case EQualityType.PRECIOUS:
                        txtQuality.text = "史诗";
                        break;
                }
            }

            if (txtLevel != null)
            {
                SCCommon.SetGameObjectEnable(txtLevel.gameObject, _level > 1);
                txtLevel.text = "+" + ( _level - 1);
            }
        }
        private void setGridInfo(List<Vector2Int> _occupyPosList,List<Vector2Int> _effectPosList)
        {
            var occSet = GameCommon.ToPositionSet(_occupyPosList);
            var effSet = GameCommon.ToPositionSet(_effectPosList);
            var union = GameCommon.UnionSortedGridPositions(_occupyPosList, _effectPosList);
            for (int i = 0; i < union.Count; i++)
            {
                Vector2Int p = union[i];
                EGridPosType t = GameCommon.GetOccupyEffectCellType(p, occSet, effSet);
                createOneGrid(p, t);
            }
            if (tranParentGrid != null)
                GameCommon.CenterTooltipPreviewGridsUnderParent(tranParentGrid.GetComponent<RectTransform>());
        }
        private void setBuffTooltipRows(List<BuffInfo> buffInfoList)
        {
            if (buffInfoList == null || tranParentBuff == null)
                return;
            for (int i = 0; i < buffInfoList.Count; i++)
            {
                GameObject itemGO = ResourcesHelper.LoadGameObject(GameConst.PREFAB_TOOLTIP_BUFF_ITEM, tranParentBuff.transform);
                var item = itemGO.GetComponent<TooltipBuffItem>();
                if (item != null)
                    item.SetBuffInfo(buffInfoList[i]);
            }
        }

        private static ScrollRect FindBuffSideScrollRect(GameObject sideRoot)
        {
            if (sideRoot == null) return null;
            var s = sideRoot.GetComponent<ScrollRect>();
            if (s != null) return s;
            s = sideRoot.GetComponentInChildren<ScrollRect>(true);
            if (s != null) return s;
            return sideRoot.GetComponentInParent<ScrollRect>();
        }

        /// <summary> 侧栏挂在 ScrollRect 或 Content 上时，组件可能在自身/子级/父级。 </summary>
        private BuffSideAutoScrollView FindBuffSideAutoScrollViewInHierarchy()
        {
            if (tranParentBuffSideInfo == null)
                return null;
            var v = tranParentBuffSideInfo.GetComponent<BuffSideAutoScrollView>();
            if (v != null) return v;
            v = tranParentBuffSideInfo.GetComponentInChildren<BuffSideAutoScrollView>(true);
            if (v != null) return v;
            return tranParentBuffSideInfo.GetComponentInParent<BuffSideAutoScrollView>();
        }

        private RectTransform GetBuffSideItemContentRoot()
        {
            if (tranParentBuffSideInfo == null)
                return null;
            var scroll = FindBuffSideScrollRect(tranParentBuffSideInfo);
            if (scroll != null && scroll.content != null)
                return scroll.content;
            return tranParentBuffSideInfo.GetComponent<RectTransform>();
        }

        /// <summary> 有 ScrollRect 时自动挂上 <see cref="BuffSideAutoScrollView"/>，避免漏挂导致不自动滚动。 </summary>
        private void EnsureBuffSideScrollController()
        {
            if (tranParentBuffSideInfo == null)
                return;
            if (FindBuffSideAutoScrollViewInHierarchy() != null)
                return;
            var scrollRect = FindBuffSideScrollRect(tranParentBuffSideInfo);
            if (scrollRect == null)
                return;
            if (scrollRect.GetComponent<BuffSideAutoScrollView>() == null)
                scrollRect.gameObject.AddComponent<BuffSideAutoScrollView>();
        }

        private void ClearBuffSideItemChildren()
        {
            if (tranParentBuffSideInfo == null)
                return;
            FindBuffSideAutoScrollViewInHierarchy()?.StopScrollTween();
            RectTransform itemParent = GetBuffSideItemContentRoot();
            if (itemParent == null)
                return;
            for (int i = itemParent.childCount - 1; i >= 0; i--)
                SCCommon.DestoryGameObject(itemParent.GetChild(i).gameObject);
        }

        private void setBuffSideItems(IList<EBuffType> buffTypes)
        {
            if (buffTypes == null || buffTypes.Count == 0 || tranParentBuffSideInfo == null)
                return;
            FindBuffSideAutoScrollViewInHierarchy()?.StopScrollTween();

            RectTransform itemParent = GetBuffSideItemContentRoot();
            if (itemParent == null)
                return;
            for (int i = itemParent.childCount - 1; i >= 0; i--)
                SCCommon.DestoryGameObject(itemParent.GetChild(i).gameObject);

            for (int i = 0; i < buffTypes.Count; i++)
            {
                GameObject sideGO = ResourcesHelper.LoadGameObject(GameConst.PREFAB_BUFF_SIDE_ITEM, itemParent);
                var sideItem = sideGO.GetComponent<CommonBuffSideItem>();
                if (sideItem != null)
                    sideItem.Initialize(buffTypes[i]);
            }
        }
        public void setLocalPosition(Vector2 _localPos)
        {
            if (_m_tooltipRect != null)
            {
                _m_tooltipRect.localPosition = _localPos;
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

        #region ShowTip

        public void ShowTooltip(string _name,string _desc, Vector2 _targetLocalPos,EQualityType _quality = EQualityType.NONE)
        {

            setBaseInfo(_name,_desc, 1,_quality);
            Vector2 adaptivePos = calculateAdaptivePosition(_targetLocalPos);
            setLocalPosition(adaptivePos);
            canvasGroup.alpha = 0;
            SCCommon.SetGameObjectEnable(gameObject, true);
            SCCommon.SetGameObjectEnable(goGrid, false);
            SCCommon.SetGameObjectEnable(goBuff, false);
            SCCommon.SetGameObjectEnable(txtQuality.transform.parent.gameObject, _quality!= EQualityType.NONE);
            _m_tweenContainer.RegDoTween(canvasGroup.DOFade(1, fadeInDuratin));
        }

        public void ShowTooltip(PartInfo _partInfo, Vector2 _targetLocalPos, bool _showGridInfo = true)
        {

            Vector2 adaptivePos = calculateAdaptivePosition(_targetLocalPos);
            setLocalPosition(adaptivePos);
            canvasGroup.alpha = 0;
            SCCommon.SetGameObjectEnable(gameObject, true);
            var sideHintTypes = PartTooltipBuffSideHintCollector.CollectSideHintBuffTypes(_partInfo);
            bool hasBuffRows = _partInfo.HasBuff();
            bool showBuffSection = hasBuffRows || sideHintTypes.Count > 0;

            SCCommon.SetGameObjectEnable(goGrid, _showGridInfo);
            SCCommon.SetGameObjectEnable(goBuff, showBuffSection);
            if (tranParentBuff != null)
                SCCommon.SetGameObjectEnable(tranParentBuff, hasBuffRows);
            if (tranParentBuffSideInfo != null)
                SCCommon.SetGameObjectEnable(tranParentBuffSideInfo, sideHintTypes.Count > 0);

            SCCommon.SetGameObjectEnable(txtQuality.gameObject, _partInfo.partRefObj.qualityType != EQualityType.NONE);

            setBaseInfo(_partInfo.partRefObj.partName, PartDescriptionFormat.GetResolvedDescription(_partInfo), _partInfo.partLevel,_partInfo.partRefObj.qualityType);
            if (_showGridInfo)
                setGridInfo(_partInfo.partRefObj.GetOccupyPosList(), _partInfo.localEffectPosList);
            if (hasBuffRows)
                setBuffTooltipRows(_partInfo.buffLogic.buffList);
            if (sideHintTypes.Count > 0)
            {
                EnsureBuffSideScrollController();
                setBuffSideItems(sideHintTypes);
            }
            else
                ClearBuffSideItemChildren();

            if (_m_tooltipRect != null && sideHintTypes.Count > 0)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(_m_tooltipRect);
                FindBuffSideAutoScrollViewInHierarchy()?.RefreshAfterItemsChanged(_m_tooltipRect);
            }
            _m_tweenContainer.RegDoTween(canvasGroup.DOFade(1, fadeInDuratin));
        }
        #endregion

        #region Util

        /// <summary>
        /// 生成单个格子并设置位置
        /// </summary>
        private void createOneGrid(Vector2Int gridPos,EGridPosType posType)
        {
            if (tranParentGrid == null) 
                return;
            GameObject grid = ResourcesHelper.LoadGameObject(GameConst.PREFAB_TOOLTIP_GIRD, tranParentGrid.transform);
            RectTransform rt = grid.GetComponent<RectTransform>();
            float x = gridPos.x * rt.rect.width;
            float y = -gridPos.y * rt.rect.height;
            if (rt != null)
                rt.anchoredPosition = new Vector2(x, y);
            rt.GetComponent<TooltipGrid>().SetGridTShow(posType);
        }
        private Vector2 calculateAdaptivePosition(Vector2 _targetLocalPos)
        {
            if (_m_tooltipRect == null || _m_canvasRect == null)
            {
                Debug.LogWarning("Tooltip/Canvas RectTransform不存在");
                return _targetLocalPos;
            }


            LayoutRebuilder.ForceRebuildLayoutImmediate(_m_tooltipRect);

            Vector3[] corners = new Vector3[4];
            _m_tooltipRect.GetWorldCorners(corners);
            float tooltipWidth = corners[3].x - corners[0].x;
            float tooltipHeight = corners[1].y - corners[0].y;

            Rect canvasRect = _m_canvasRect.rect;
            float canvasLeft = canvasRect.xMin + screenPadding;
            float canvasRight = canvasRect.xMax - screenPadding;
            float canvasBottom = canvasRect.yMin + screenPadding;
            float canvasTop = canvasRect.yMax - screenPadding;

            Vector2 adaptivePos = _targetLocalPos;

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
        #endregion

    }
}