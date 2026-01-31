using DG.Tweening;
using GameCore.RefData;
using SCFrame;
using SCFrame.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameCore.UI
{

    public class UIPanelStoreBagItem : _ASCUIPanelBase<UIMonoStoreBagItem>
    {
        private PartInfo _m_partInfo;
        private TweenContainer _m_tweenContainer;

        public UIPanelStoreBagItem(UIMonoStoreBagItem _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_tweenContainer = new TweenContainer();

        }

        public override void BeforeDiscard()
        {
            _m_tweenContainer?.KillAllDoTween();
            _m_tweenContainer = null;
        }

        public override void OnHidePanel()
        {
            mono.btnSell.RemoveClickDown(onBtnSellClickDown);
            GetGameObject().transform.RemoveMouseEnter(onGameObjMouseEnter);
            GetGameObject().transform.RemoveMouseExit(onGameObjMouseExit);
        }

        public override void OnShowPanel()
        {
            mono.btnSell.AddMouseLeftClickDown(onBtnSellClickDown);
            GetGameObject().transform.AddMouseEnter(onGameObjMouseEnter);
            GetGameObject().transform.AddMouseExit(onGameObjMouseExit);

        }

        public void SetInfo(PartInfo _partInfo)
        {
            _m_partInfo = _partInfo;
            refreshShow();
        }

        private void refreshShow()
        {
            if (_m_partInfo == null)
                return;
            mono.imgIcon.sprite = ResourcesHelper.LoadAsset<Sprite>(_m_partInfo.partRefObj.partSpriteObjName);
            mono.txtHealth.text = _m_partInfo.partRefObj.partHealth.ToString();
            GoodsRefObj goodsRefObj = SCRefDataMgr.instance.goodsRefList.refDataList.Find(x => x.partId == _m_partInfo.partRefObj.id);
            if(goodsRefObj!=null)
                mono.txtValue.text = (goodsRefObj.goodsPrice / 2).ToString();
        }

        private void onGameObjMouseExit(PointerEventData arg1, object[] arg2)
        {
            GameCommon.DiscardToolTip();
            _m_tweenContainer.RegDoTween(GetGameObject().transform.DOScale(Vector3.one, mono.scaleChgDuration));
        }

        private void onGameObjMouseEnter(PointerEventData arg1, object[] arg2)
        {
            if (_m_partInfo == null)
                return;
            Vector2 screenPos = Vector2.zero;
            var _canvas = GetGameObject().GetComponentInParent<Canvas>();
            Camera cam = (_canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? _canvas.worldCamera : null;

            // Check direction
            float itemScreenX = RectTransformUtility.WorldToScreenPoint(cam, GetGameObject().transform.position).x;
            bool showOnLeft = itemScreenX > Screen.width * 0.7f; // If in right 30% of screen

            // Offset based on direction
            // Left: Pivot Right-Top (1,1) -> Anchor at (ItemX - border, ItemY)
            // Right: Pivot Left-Top (0,1) -> Anchor at (ItemX + border, ItemY)
            Vector3 offset = showOnLeft ? new Vector3(-40, -20, 0) : new Vector3(40, -20, 0);

            screenPos = RectTransformUtility.WorldToScreenPoint(cam, GetGameObject().transform.position + offset);

            var tooltip = GameCommon.ShowTooltip(_m_partInfo.partRefObj.partName, _m_partInfo.partRefObj.partDesc, screenPos);
            /*if (tooltip != null)
            {
                tooltip.SetPivot(showOnLeft ? new Vector2(1, 1) : new Vector2(0, 1));
            }*/
            _m_tweenContainer.RegDoTween(GetGameObject().transform.DOScale(mono.scaleMouseEnter, mono.scaleChgDuration));

        }

        private void onBtnSellClickDown(PointerEventData arg1, object[] arg2)
        {

            GameCommon.DiscardToolTip();
            SCMsgCenter.SendMsg(SCMsgConst.SELL_PART, _m_partInfo);
        }
    }
}
