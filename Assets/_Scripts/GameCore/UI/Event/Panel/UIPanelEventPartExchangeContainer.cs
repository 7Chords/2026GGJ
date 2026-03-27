using SCFrame;
using SCFrame.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.UI
{
    public class UIPanelEventPartExchangeContainer : UIPanelContainerBase<UIMonoCommonContainer, UIPanelEventPartExchangeItem, UIMonoCommonPartItem>
    {
        private List<UIPanelEventPartExchangeItem> _m_itemList;

        public UIPanelEventPartExchangeContainer(UIMonoCommonContainer _mono, SCUIShowType _showType = SCUIShowType.INTERNAL) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_itemList = new List<UIPanelEventPartExchangeItem>();
        }

        public override void BeforeDiscard()
        {
            if (_m_itemList == null)
                return;
            foreach (var item in _m_itemList)
                item?.Discard();
        }

        public override void OnHidePanel()
        {
            if (_m_itemList == null)
                return;
            foreach (var item in _m_itemList)
                item?.HidePanel();
        }

        public override void OnShowPanel()
        {
        }

        protected override GameObject creatItemGO()
        {
            return ResourcesHelper.LoadGameObject(mono.prefabItemObjName, mono.layoutGroup.transform);
        }

        protected override UIPanelEventPartExchangeItem creatItemPanel(UIMonoCommonPartItem _mono)
        {
            return new UIPanelEventPartExchangeItem(_mono, SCUIShowType.INTERNAL);
        }

        public void SetListInfo(List<PartInfo> _infoList, Action<PartInfo> _onSelect)
        {
            if (_infoList == null || _m_itemList == null)
                return;

            int i = 0;
            int count = 0;
            UIPanelEventPartExchangeItem item = null;
            for (i = 0; i < _infoList.Count; i++)
            {
                if (i < _m_itemList.Count)
                    item = _m_itemList[i];
                else
                {
                    GameObject itemGO = creatItemGO();
                    item = creatItemPanel(itemGO.GetComponent<UIMonoCommonPartItem>());
                    _m_itemList.Add(item);
                }
                if (item == null)
                    continue;
                item.SetInfo(_infoList[i], _onSelect);
                item.ShowPanel();
                count++;
            }
            for (i = count; i < _m_itemList.Count; i++)
            {
                item = _m_itemList[i];
                if (item != null)
                    item.HidePanel();
            }
        }
    }
}
