using GameCore.RefData;
using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.UI
{
    public class UIPanelStore : _ASCUIPanelBase<UIMonoStore>
    {

        private StoreRefObj _m_storeRefObj;

        private List<UIPanelStoreItem> storeItemList;
        public UIPanelStore(UIMonoStore _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            storeItemList = new List<UIPanelStoreItem>();
            UIPanelStoreItem item = null;
            for (int i =0;i<mono.monoItemList.Count;i++)
            {
                if (mono.monoItemList[i] == null)
                    continue;
                item = new UIPanelStoreItem(mono.monoItemList[i],SCUIShowType.INTERNAL);
                storeItemList.Add(item);
            }
        }

        public override void BeforeDiscard()
        {
            foreach (var item in storeItemList)
                item?.Discard();
        }

        public override void OnHidePanel()
        {
            foreach (var item in storeItemList)
                item?.HidePanel();
        }

        public override void OnShowPanel()
        {
            _m_storeRefObj = SCRefDataMgr.instance.storeRefList.refDataList.Find(x => x.id == GameModel.instance.rollStoreId);

            foreach (var item in storeItemList)
                item?.ShowPanel();
            refreshShow();
        }

        private void refreshShow()
        {
            if (_m_storeRefObj == null)
                return;
            GoodsEffectObj effectObj;
            for(int i =0;i<_m_storeRefObj.goodsList.Count;i++)
            {
                effectObj = _m_storeRefObj.goodsList[i];
                if (effectObj == null)
                    continue;
                GoodsRefObj goodsRefObj = SCRefDataMgr.instance.goodsRefList.refDataList.Find(x => x.id == effectObj.goodsId);
                if (goodsRefObj == null)
                    continue;
                GoodsInfo info = new GoodsInfo(goodsRefObj, effectObj.goodsAmount);
                storeItemList[i].SetInfo(info);
            }
        }
    }
}
