using GameCore.RefData;
using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SCFrame;
using UnityEngine.EventSystems;
using System;

namespace GameCore.UI
{
    public class UIPanelEventSelectItem : _ASCUIPanelBase<UIMonoEventSelectItem>
    {
        private EventDialogueRefObj _m_dialogueRefObj;
        public UIPanelEventSelectItem(UIMonoEventSelectItem _mono, SCUIShowType _showType) : base(_mono, _showType)
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
            mono.btnSelect.RemoveClickDown(onBtnSelectClickDown);
        }

        public override void OnShowPanel()
        {
            mono.btnSelect.AddMouseLeftClickDown(onBtnSelectClickDown);
        }

        public void SetInfo(EventDialogueRefObj _refObj)
        {
            _m_dialogueRefObj = _refObj;
            refreshShow();
        }

        private void refreshShow()
        {
            if (_m_dialogueRefObj == null)
                return;
            mono.txtContent.text = _m_dialogueRefObj.content;
        }

        private void onBtnSelectClickDown(PointerEventData _data, object[] _objs)
        {

        }
    }
}
