using SCFrame;
using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.UI
{
    public class UIPanelMaskCombinePartContainer : UIPanelContainerBase<UIMonoCommonContainer, UIPanelMaskCombinePartContainerItem, UIMonoMaskCombinePartContainerItem>
    {
        private List<UIPanelMaskCombinePartContainerItem> _m_partItemList;//item列表

        public UIPanelMaskCombinePartContainer(UIMonoCommonContainer _mono, SCUIShowType _showType = SCUIShowType.INTERNAL) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_partItemList = new List<UIPanelMaskCombinePartContainerItem>();
        }

        public override void BeforeDiscard()
        {
            foreach(var item in _m_partItemList)
            {
                item?.Discard();
            }
        }

        public override void OnHidePanel()
        {
            foreach (var item in _m_partItemList)
            {
                item?.HidePanel();
            }
        }

        public override void OnShowPanel()
        {
            ReloadParts();
        }

        public void ReloadParts()
        {
            if (_m_partItemList != null)
            {
                 foreach(var item in _m_partItemList)
                 {
                    item?.Discard();
                 }
                 _m_partItemList.Clear();
            }
            else
            {
                _m_partItemList = new List<UIPanelMaskCombinePartContainerItem>();
            }

            if (GameModel.instance.busyPartInfoList != null)
            {
                foreach(var info in GameModel.instance.busyPartInfoList)
                {
                    addItem(info);
                }
            }
        }
        
        // Override AddItem to handle initialization
        public void addItem(PartInfo info)
        {
             GameObject go = creatItemGO();
             UIMonoMaskCombinePartContainerItem itemMono = go.GetComponent<UIMonoMaskCombinePartContainerItem>();
             if (itemMono != null)
             {
                 var panel = creatItemPanel(itemMono);
                 panel.SetInfo(info);
                 panel.ShowPanel();
                 _m_partItemList.Add(panel);
             }
        }

        protected override GameObject creatItemGO()
        {
            return ResourcesHelper.LoadGameObject(mono.prefabItemObjName,mono.layoutGroup.transform);
        }


        protected override UIPanelMaskCombinePartContainerItem creatItemPanel(UIMonoMaskCombinePartContainerItem _mono)
        {
            var item = new UIPanelMaskCombinePartContainerItem(_mono, SCUIShowType.INTERNAL);
            return item;
        }

    }
}
