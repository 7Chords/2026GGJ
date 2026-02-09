using SCFrame;
using SCFrame.UI;
using System;
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
        protected override GameObject creatItemGO()
        {
            return ResourcesHelper.LoadGameObject(mono.prefabItemObjName, mono.layoutGroup.transform);
        }

        protected override UIPanelMaskCombinePartContainerItem creatItemPanel(UIMonoMaskCombinePartContainerItem _mono)
        {
            var item = new UIPanelMaskCombinePartContainerItem(_mono, SCUIShowType.INTERNAL);
            return item;
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
            SCMsgCenter.UnregisterMsg(SCMsgConst.PLACE_PART_SUCCESS, onPlacePartSuccess);

            foreach (var item in _m_partItemList)
            {
                item?.HidePanel();
            }
        }

        public override void OnShowPanel()
        {
            SCMsgCenter.RegisterMsg(SCMsgConst.PLACE_PART_SUCCESS, onPlacePartSuccess);
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


        private void onPlacePartSuccess(object[] _objs)
        {
            if (_objs == null || _objs.Length < 2)
                return;
            PartInfo partInfo = _objs[0] as PartInfo;

            if (GameModel.instance.busyPartInfoList.Contains(partInfo))
            {
                GameModel.instance.busyPartInfoList.Remove(partInfo);
                ReloadParts();
            }

        }


    }
}
