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
        public UIPanelStoreItem(UIMonoStoreItem _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
        }

        public override void BeforeDiscard()
        {
        }

        public override void OnHidePanel()
        {
            mono.btnPurchase.RemoveClickDown(onBtnPurchaseClickDonw);
            mono.btnPurchase.RemoveMouseEnter(onBtnPurchaseMouseEnter);
        }


        private void onBtnPurchaseClickDonw(PointerEventData _arg, object[] _objs)
        {
            //钱够了直接发送购买消息
            if(GameModel.instance.playerMoney >= _m_goodsInfo.goodsRefObj.goodsPrice
                && _m_goodsInfo.goodsAmount > 0)
                SCMsgCenter.SendMsg(SCMsgConst.PURCHASE_GOODS, _m_goodsInfo.goodsRefObj.id);
        }

        public override void OnShowPanel()
        {
            mono.btnPurchase.AddMouseLeftClickDown(onBtnPurchaseClickDonw);
            mono.btnPurchase.AddMouseEnter(onBtnPurchaseMouseEnter);
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
            mono.imgIcon.sprite = ResourcesHelper.LoadAsset<Sprite>(_m_goodsInfo.goodsRefObj.goodsSpriteObjName);
            mono.txtAmount.text = _m_goodsInfo.goodsAmount.ToString();
            mono.txtPrice.text = _m_goodsInfo.goodsRefObj.goodsPrice.ToString();
        }
        private void onBtnPurchaseMouseEnter(PointerEventData _arg, object[] _objs)
        {

        }
    }
}
