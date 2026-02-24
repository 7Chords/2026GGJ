using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SCFrame;
using SCFrame.UI;

namespace GameCore.UI
{
    public class UIPanelStrengthenBagContainer : UIPanelContainerBase<UIMonoCommonContainer, UIPanelStrengthenBagItem, UIMonoStrengthenBagItem>
    {
        private List<UIPanelStrengthenBagItem> _m_itemList;//item列表
        public UIPanelStrengthenBagContainer(UIMonoCommonContainer _mono, SCUIShowType _showType = SCUIShowType.INTERNAL) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_itemList = new List<UIPanelStrengthenBagItem>();
        }

        public override void BeforeDiscard()
        {
            foreach (var item in _m_itemList)
            {
                item?.Discard();
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

        protected override UIPanelStrengthenBagItem creatItemPanel(UIMonoStrengthenBagItem _mono)
        {
            return new UIPanelStrengthenBagItem(_mono, SCUIShowType.INTERNAL);
        }


        public void SetListInfo(List<PartInfo> _infoList)
        {
            if (_infoList == null)
                return;
            if (_m_itemList == null)
                return;

            int i = 0, count = 0;
            UIPanelStrengthenBagItem item = null;
            for (i = 0; i < _infoList.Count; i++)
            {
                if (i < _m_itemList.Count)
                {
                    item = _m_itemList[i];
                }
                else
                {
                    GameObject itemGO = creatItemGO();
                    item = creatItemPanel(itemGO.GetComponent<UIMonoStrengthenBagItem>());

                    _m_itemList.Add(item);
                }
                if (item == null)
                    continue;
                item.SetInfo(_infoList[i],false);
                item.ShowPanel();
                count++;
            }
            //隐藏多余的
            for (i = count; i < _m_itemList.Count; i++)
            {
                item = _m_itemList[i];
                if (item == null)
                    continue;
                item.HidePanel();
            }

        }

        public void RefreshShow(List<PartInfo> _infoList,PartInfo _selectPart) 
        {
            if (_infoList == null)
                return;
            if (_m_itemList == null)
                return;

            int i = 0, count = 0;
            UIPanelStrengthenBagItem item = null;
            for (i = 0; i < _infoList.Count; i++)
            {
                if (i < _m_itemList.Count)
                {
                    item = _m_itemList[i];
                }
                else
                {
                    GameObject itemGO = creatItemGO();
                    item = creatItemPanel(itemGO.GetComponent<UIMonoStrengthenBagItem>());

                    _m_itemList.Add(item);
                }
                if (item == null)
                    continue;
                if(_infoList[i] == _selectPart)
                    item.SetInfo(_infoList[i], true);
                else
                    item.SetInfo(_infoList[i], false);

                count++;
            }
            //隐藏多余的
            for (i = count; i < _m_itemList.Count; i++)
            {
                item = _m_itemList[i];
                if (item == null)
                    continue;
                item.HidePanel();
            }
        }

    }
}
