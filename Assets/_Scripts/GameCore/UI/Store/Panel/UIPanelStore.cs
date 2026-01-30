using GameCore.RefData;
using SCFrame;
using SCFrame.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.UI
{
    public class UIPanelStore : _ASCUIPanelBase<UIMonoStore>
    {

        private StoreRefObj _m_storeRefObj;

        private List<UIPanelStoreItem> _m_storeItemList;

        private List<GoodsInfo> _m_goodsInfoList;
        public UIPanelStore(UIMonoStore _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_storeItemList = new List<UIPanelStoreItem>();
            UIPanelStoreItem item = null;
            for (int i =0;i<mono.monoItemList.Count;i++)
            {
                if (mono.monoItemList[i] == null)
                    continue;
                item = new UIPanelStoreItem(mono.monoItemList[i],SCUIShowType.INTERNAL);
                _m_storeItemList.Add(item);
            }

            _m_goodsInfoList = new List<GoodsInfo>();
        }

        public override void BeforeDiscard()
        {
            foreach (var item in _m_storeItemList)
                item?.Discard();
            _m_goodsInfoList.Clear();
            _m_goodsInfoList = null;
        }

        public override void OnHidePanel()
        {
            SCMsgCenter.UnregisterMsg(SCMsgConst.PURCHASE_GOODS, onPurchaseGoods);

            foreach (var item in _m_storeItemList)
                item?.HidePanel();
        }

        public override void OnShowPanel()
        {
            SCMsgCenter.RegisterMsg(SCMsgConst.PURCHASE_GOODS, onPurchaseGoods);

            foreach (var item in _m_storeItemList)
                item?.ShowPanel();


            _m_storeRefObj = SCRefDataMgr.instance.storeRefList.refDataList.Find(x => x.id == GameModel.instance.rollStoreId);
            if (_m_storeRefObj == null)
                return;

            GoodsEffectObj effectObj;
            for (int i = 0; i < _m_storeRefObj.goodsList.Count; i++)
            {
                effectObj = _m_storeRefObj.goodsList[i];
                if (effectObj == null)
                    continue;
                GoodsRefObj goodsRefObj = SCRefDataMgr.instance.goodsRefList.refDataList.Find(x => x.id == effectObj.goodsId);
                if (goodsRefObj == null)
                    continue;
                GoodsInfo info = new GoodsInfo(goodsRefObj, effectObj.goodsAmount);
                _m_goodsInfoList.Add(info);
            }
            refreshShow();
        }

        private void onPurchaseGoods(object[] _objs)
        {
            if (_objs == null || _objs.Length == 0)
                return;
            long goodsId = (long)_objs[0];
            GoodsInfo info = _m_goodsInfoList.Find(x => x.goodsRefObj.id == goodsId);
            if (info == null)
                return;
            info.goodsAmount = Mathf.Max(info.goodsAmount - 1, 0);
            refreshShow();
        }

        private void refreshShow()
        {
            if (_m_storeRefObj == null)
                return;

            for(int i =0;i< _m_storeItemList.Count;i++)
            {
                _m_storeItemList[i].SetInfo(_m_goodsInfoList[i]);
            }
        }
    }
}
