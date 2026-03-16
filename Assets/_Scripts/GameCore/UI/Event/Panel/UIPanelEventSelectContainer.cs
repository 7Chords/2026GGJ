using GameCore.RefData;
using SCFrame;
using SCFrame.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.UI
{
    public class UIPanelEventSelectContainer : UIPanelContainerBase<UIMonoCommonContainer, UIPanelEventSelectItem, UIMonoEventSelectItem>
    {
        private List<UIPanelEventSelectItem> _m_selectItemList;//item¡–±Ì

        public UIPanelEventSelectContainer(UIMonoCommonContainer _mono, SCUIShowType _showType = SCUIShowType.INTERNAL) : base(_mono, _showType)
        {
        }
        protected override GameObject creatItemGO()
        {
            return ResourcesHelper.LoadGameObject(mono.prefabItemObjName, mono.layoutGroup.transform);
        }

        protected override UIPanelEventSelectItem creatItemPanel(UIMonoEventSelectItem _mono)
        {
            var item = new UIPanelEventSelectItem(_mono, SCUIShowType.INTERNAL);
            return item;
        }

        public override void AfterInitialize()
        {
            _m_selectItemList = new List<UIPanelEventSelectItem>();
        }

        public override void BeforeDiscard()
        {
            foreach (var item in _m_selectItemList )
            {
                item?.Discard();
            }
        }

        public override void OnHidePanel()
        {
            SCMsgCenter.UnregisterMsg(SCMsgConst.EVENT_START_SELECT, onEventStartSelect);
            SCMsgCenter.UnregisterMsg(SCMsgConst.EVENT_END_SELECT, onEventEndSelect);

        }

        public override void OnShowPanel()
        {
            SCMsgCenter.RegisterMsg(SCMsgConst.EVENT_START_SELECT, onEventStartSelect);
            SCMsgCenter.RegisterMsg(SCMsgConst.EVENT_END_SELECT, onEventEndSelect);

        }

        private void onEventStartSelect(object[] _objs)
        {
            if (_objs == null || _objs.Length == 0)
                return;
            EventDialogueRefObj dialogueRefObj = _objs[0] as EventDialogueRefObj;
            for(int i =0;i<dialogueRefObj.nextList.Count;i++)
            {
                EventDialogueRefObj selectRefObj = SCRefDataMgr.instance.eventDialogueRefList.refDataList
                    .Find(x => x.id == dialogueRefObj.nextList[i]);
                addItem(selectRefObj);
            }

        }

        private void onEventEndSelect(object[] _objs)
        {
            if(_m_selectItemList != null)
            {
                foreach (var item in _m_selectItemList)
                {
                    item?.Discard();
                }
                _m_selectItemList.Clear();
            }
        }

        private void addItem(EventDialogueRefObj _dialogueRefObj)
        {
            GameObject go = creatItemGO();
            UIMonoEventSelectItem itemMono = go.GetComponent<UIMonoEventSelectItem>();
            if (itemMono != null)
            {
                var panel = creatItemPanel(itemMono);
                panel.SetInfo(_dialogueRefObj);
                panel.ShowPanel();
                _m_selectItemList.Add(panel);
            }
        }
    }
}
