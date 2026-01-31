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
    public class UIPanelStoreItem : _ASCUIPanelBase<UIMonoStoreItem>
    {
        private GoodsInfo _m_goodsInfo;

        private TweenContainer _m_tweenContainer;
        public UIPanelStoreItem(UIMonoStoreItem _mono, SCUIShowType _showType) : base(_mono, _showType)
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
            mono.btnPurchase.RemoveClickDown(onBtnPurchaseClickDonw);
            mono.btnPurchase.RemoveMouseEnter(onBtnPurchaseMouseEnter);
            mono.btnPurchase.RemoveMouseExit(onBtnPurchaseMouseExit);
        }


        private void onBtnPurchaseClickDonw(PointerEventData _arg, object[] _objs)
        {
            //钱够了直接发送购买消息
            if(GameModel.instance.playerMoney >= _m_goodsInfo.goodsRefObj.goodsPrice
                && _m_goodsInfo.goodsAmount > 0)
            {
                AudioMgr.instance.PlaySfx("sfx_buy");
                SCMsgCenter.SendMsg(SCMsgConst.PURCHASE_GOODS, _m_goodsInfo.goodsRefObj.id);
            }
        }

        public override void OnShowPanel()
        {
            mono.btnPurchase.AddMouseLeftClickDown(onBtnPurchaseClickDonw);
            mono.btnPurchase.AddMouseEnter(onBtnPurchaseMouseEnter);
            mono.btnPurchase.AddMouseExit(onBtnPurchaseMouseExit);
        }

        public void SetInfo(GoodsInfo _goodsInfo)
        {
            _m_goodsInfo = _goodsInfo;
            refreshShow();
        }

        private void refreshShow()
        {
            if (_m_goodsInfo == null)
                return;
            refreshHasBuyShow();
            refreshActiveShow();
            mono.imgIcon.sprite = ResourcesHelper.LoadAsset<Sprite>(_m_goodsInfo.goodsRefObj.goodsSpriteObjName);
            if (_m_goodsInfo.goodsRefObj.goodsType == EGoodsType.PART)
            {
                mono.txtPartPrice.text = _m_goodsInfo.goodsRefObj.goodsPrice.ToString();
                PartRefObj partRefObj = SCRefDataMgr.instance.partRefList.refDataList.Find(x => x.id == _m_goodsInfo.goodsRefObj.partId);
                mono.txtPartHealth.text = partRefObj.partHealth.ToString();
            }
            else
            {
                mono.txtHealthPrice.text = _m_goodsInfo.goodsRefObj.goodsPrice.ToString();
            }
        }

        private void refreshHasBuyShow()
        {
            mono.canvasGroup.alpha = _m_goodsInfo.goodsAmount == 0 ? mono.hasPurchaseAlpha : 1;
        }
        private void refreshActiveShow()
        {
            SCCommon.SetGameObjectEnable(mono.goIsPartShowList, _m_goodsInfo.goodsRefObj.goodsType == EGoodsType.PART);
            SCCommon.SetGameObjectEnable(mono.goIsHealthShowList, _m_goodsInfo.goodsRefObj.goodsType == EGoodsType.HEAL);

        }
        private void onBtnPurchaseMouseEnter(PointerEventData _arg, object[] _objs)
        {
            if (_m_goodsInfo == null)
                return;
            AudioMgr.instance.PlaySfx("sfx_mouse_enter");

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

            var tooltip = GameCommon.ShowTooltip(_m_goodsInfo.goodsRefObj.goodsName, _m_goodsInfo.goodsRefObj.goodsDesc, screenPos);
            /*if (tooltip != null)
            {
                tooltip.SetPivot(showOnLeft ? new Vector2(1, 1) : new Vector2(0, 1));
            }*/
            _m_tweenContainer.RegDoTween(GetGameObject().transform.DOScale(mono.scaleMouseEnter, mono.scaleChgDuration));


        }

        private void onBtnPurchaseMouseExit(PointerEventData _arg, object[] _objs)
        {
            GameCommon.DiscardToolTip();
            _m_tweenContainer.RegDoTween(GetGameObject().transform.DOScale(Vector3.one, mono.scaleChgDuration));

        }
    }
}
