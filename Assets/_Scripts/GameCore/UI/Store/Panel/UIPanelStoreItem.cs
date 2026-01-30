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
            mono.btnPurchase.RemoveClickDown(onClickDonw);
            //SCMsgCenter.UnregisterMsg(SCMsgConst.PURCHASE_GOODS, onPurchaseGoods);
        }

        private void onClickDonw(PointerEventData _arg, object[] _objs)
        {
        }

        public override void OnShowPanel()
        {
            mono.btnPurchase.AddMouseLeftClickDown(onClickDonw);
            //SCMsgCenter.RegisterMsg(SCMsgConst.PURCHASE_GOODS, onPurchaseGoods);
        }

        //private void onPurchaseGoods(object[] _objs)
        //{
        //    if (_objs == null || _objs.Length == 0)
        //        return;
        //    long goodsId = (long)_objs[0];

        //    if (goodsId != _m_goodsInfo.goodsRefObj.id)
        //        return;

        //    refreshShow();
        //}

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
    }
}
