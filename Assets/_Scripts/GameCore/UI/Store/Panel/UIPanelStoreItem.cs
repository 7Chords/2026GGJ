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
            //钱够了并且没有买过 发送购买消息
            if(GameModel.instance.playerInfo.playerMoney >= _m_goodsInfo.goodsRefObj.goodsPrice
                && !_m_goodsInfo.hasBought)
            {
                AudioMgr.instance.PlaySfx("sfx_buy");
                SCMsgCenter.SendMsg(SCMsgConst.PURCHASE_GOODS, _m_goodsInfo.goodsRefObj.id);
                GameCommon.ShowPopTip("获得" + _m_goodsInfo.goodsRefObj.goodsName,Vector2.zero);
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
                PartLevelRefObj partLevelRefObj = SCRefDataMgr.instance.partLevelRefList.refDataList.Find(x => x.id == _m_goodsInfo.goodsRefObj.partId);
                mono.txtPartHealth.text = partLevelRefObj.partHealth.ToString();
            }
            else
            {
                mono.txtHealthPrice.text = _m_goodsInfo.goodsRefObj.goodsPrice.ToString();
            }
        }

        private void refreshHasBuyShow()
        {
            mono.canvasGroup.alpha = _m_goodsInfo.hasBought ? mono.hasPurchaseAlpha : 1;
            SCCommon.SetGameObjectEnable(mono.goHasBuyShowList, _m_goodsInfo.hasBought);
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

            if (_m_goodsInfo.goodsRefObj.goodsType == EGoodsType.PART)
            {
                PartLevelRefObj levelRefObj = SCRefDataMgr.instance.partLevelRefList.refDataList.Find(x => x.id == _m_goodsInfo.goodsRefObj.partId);
                if (levelRefObj == null)
                    return;
                PartRefObj partRefObj = SCRefDataMgr.instance.partRefList.refDataList.Find(x => x.id == levelRefObj.partId);
                PartInfo info = new PartInfo(partRefObj, false, levelRefObj.partLevel);
                GameCommon.ShowTooltip(info,
                    GetGameObject().transform.position);
            }
            else
            {
                GameCommon.ShowTooltip(_m_goodsInfo.goodsRefObj.goodsName,
                    _m_goodsInfo.goodsRefObj.goodsDesc,
                    GetGameObject().transform.position,EQualityType.NONE);
            }
            

            _m_tweenContainer.RegDoTween(GetGameObject().transform.DOScale(mono.scaleMouseEnter, mono.scaleChgDuration));


        }

        private void onBtnPurchaseMouseExit(PointerEventData _arg, object[] _objs)
        {
            GameCommon.DiscardToolTip();
            _m_tweenContainer.RegDoTween(GetGameObject().transform.DOScale(Vector3.one, mono.scaleChgDuration));

        }
    }
}
