using DG.Tweening;
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
            for(int i = 0; i < _occupyPosList.Count; i++)
            {
                createOneGrid(_occupyPosList[i],EGridPosType.OCCUPY);
            }
            if(!_occupyPosList.Vector2IntListEquals(_effectPosList))
            {
                for (int i = 0; i < _effectPosList.Count; i++)
                {
                    createOneGrid(_effectPosList[i], EGridPosType.EFFECT);
                }
            }
            else
            {
                //todo:现在设计的有重叠都是完全重叠的暂时这样写
                for (int i = 0; i < _effectPosList.Count; i++)
                {
                    createOneGrid(_effectPosList[i], EGridPosType.BOTH);
                }
            }
        }
        private void setBuffInfo(List<BuffInfo> _buffInfoList)
        {
            if (_buffInfoList == null)
                return;
            if (tranParentBuff == null)
                return;
            GameObject itemGO = null;
            TooltipBuffItem item = null;
            for (int i =0;i< _buffInfoList.Count;i++)
            {
                itemGO = ResourcesHelper.LoadGameObject(GameConst.PREFAB_TOOLTIP_BUFF_ITEM, tranParentBuff.transform);
                item = itemGO.GetComponent<TooltipBuffItem>();
                if (item != null)
                    item.SetBuffInfo(_buffInfoList[i]);
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
            SCCommon.SetGameObjectEnable(goGrid, _showGridInfo);
            SCCommon.SetGameObjectEnable(goBuff, _partInfo.HasBuff());
            SCCommon.SetGameObjectEnable(txtQuality.gameObject, _partInfo.partRefObj.qualityType != EQualityType.NONE);

            setBaseInfo(_partInfo.partRefObj.partName, _partInfo.levelRefObj.partDesc, _partInfo.partLevel,_partInfo.partRefObj.qualityType);
            if (_showGridInfo)
                setGridInfo(_partInfo.partRefObj.GetOccupyPosList(), _partInfo.localEffectPosList);
            if (_partInfo.HasBuff())
                setBuffInfo(_partInfo.buffLogic.buffList);
            _m_tweenContainer.RegDoTween(canvasGroup.DOFade(1, fadeInDuratin));
        }

        public void ShowTooltip(PartRefObj _partRefObj, Vector2 _targetLocalPos, bool _showGridInfo = true)
        {
            Vector2 adaptivePos = calculateAdaptivePosition(_targetLocalPos);
            setLocalPosition(adaptivePos);
            canvasGroup.alpha = 0;
            SCCommon.SetGameObjectEnable(gameObject, true);
            SCCommon.SetGameObjectEnable(goGrid, _showGridInfo);
            SCCommon.SetGameObjectEnable(goBuff, false);
            SCCommon.SetGameObjectEnable(txtQuality.gameObject, _partRefObj.qualityType != EQualityType.NONE);

            setBaseInfo(_partRefObj.partName, _partRefObj.partDesc, 1,_partRefObj.qualityType);
            if (_showGridInfo)
                setGridInfo(_partRefObj.GetOccupyPosList(), _partRefObj.GetEffectPosList());

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