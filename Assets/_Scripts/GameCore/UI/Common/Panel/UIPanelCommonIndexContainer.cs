using SCFrame;
using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.UI
{
    public class UIPanelCommonIndexContainer : UIPanelContainerBase<UIMonoCommonContainer, UIPanelCommonIndexItem, UIMonoCommonIndexItem>
    {
        private List<UIPanelCommonIndexItem> _m_itemList;
        public UIPanelCommonIndexContainer(UIMonoCommonContainer _mono, SCUIShowType _showType = SCUIShowType.INTERNAL) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_itemList = new List<UIPanelCommonIndexItem>();
        }

        public override void BeforeDiscard()
        {
            if(_m_itemList != null)
            {
                foreach (var item in _m_itemList)
                {
                    item?.Discard();
                }
            }
        }

        public override void OnHidePanel()
        {
            foreach (var item in _m_itemList)
            {
                item?.HidePanel();
            }
        }

        public override void OnShowPanel()
        {
        }

        protected override GameObject creatItemGO()
        {
            return ResourcesHelper.LoadGameObject(mono.prefabItemObjName, mono.layoutGroup.transform);
        }

        protected override UIPanelCommonIndexItem creatItemPanel(UIMonoCommonIndexItem _mono)
        {
            var item = new UIPanelCommonIndexItem(_mono, SCUIShowType.INTERNAL);
            return item;
        }

        public void SetIndexs(int _count,int _curSelectIndex)
        {
            if (_count < 0)
                return;
            if (_m_itemList == null)
                return;

            int i = 0, count = 0;
            UIPanelCommonIndexItem item = null;
            for (i = 0; i < _count; i++)
            {
                if (i < _m_itemList.Count)
                {
                    item = _m_itemList[i];
                }
                else
                {
                    GameObject itemGO = creatItemGO();
                    item = creatItemPanel(itemGO.GetComponent<UIMonoCommonIndexItem>());

                    _m_itemList.Add(item);
                }
                if (item == null)
                    continue;
                item.SetSelectState(_curSelectIndex == i);
                item.ShowPanel();
                count++;
            }
            //Òþ²Ø¶àÓàµÄ
            for (i = count; i < _m_itemList.Count; i++)
            {
                item = _m_itemList[i];
                if (item == null)
                    continue;
                item.HidePanel();
            }
        }

        public void RefreshShow(int _curSelectIndex)
        {
            for(int i = 0; i < _m_itemList.Count; i++)
            {
                _m_itemList[i].SetSelectState(_curSelectIndex == i);
            }
        }
    }
}
