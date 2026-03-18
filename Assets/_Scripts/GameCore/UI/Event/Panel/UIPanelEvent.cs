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
    public class UIPanelEvent : _ASCUIPanelBase<UIMonoEvent>
    {
        private UIPanelEventSelectContainer _m_selectContainer;

        private long _m_eventDialogueId;
        private EventDialogueRefObj _m_eventDialogueRefObj;
        private bool _m_isSelecting;
        public UIPanelEvent(UIMonoEvent _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_selectContainer = new UIPanelEventSelectContainer(mono.monoSelectContainer, SCUIShowType.INTERNAL);
        }

        public override void BeforeDiscard()
        {
            _m_selectContainer?.Discard();
        }

        public override void OnHidePanel()
        {
            SCMsgCenter.RegisterMsg(SCMsgConst.EVENT_SELECT_CONFIRM, onEventSelectConfirm);
            mono.imgClickArea.RemoveClickDown(onMouseClickDialogue);
            _m_selectContainer?.HidePanel();

        }

        public override void OnShowPanel()
        {
            SCMsgCenter.RegisterMsg(SCMsgConst.EVENT_SELECT_CONFIRM, onEventSelectConfirm);
            mono.imgClickArea.AddMouseLeftClickDown(onMouseClickDialogue);
            _m_eventDialogueId = GameModel.instance.rollEventId;
            _m_eventDialogueRefObj = SCRefDataMgr.instance.eventDialogueRefList.refDataList.Find(x => x.id == _m_eventDialogueId);

            _m_selectContainer?.ShowPanel();
            refreshShow();
        }

        private void refreshShow()
        {
            if (_m_eventDialogueRefObj == null)
                return;
            if(_m_eventDialogueRefObj.dialogueType == EEventDialogueType.STANDARD)
            {
                mono.txtName.text = _m_eventDialogueRefObj.name;
                mono.txtContent.text = _m_eventDialogueRefObj.content;
            }
        }
        private void onMouseClickDialogue(PointerEventData _data, object[] _objs)
        {
            if (_m_eventDialogueRefObj == null)
                return;
            if (_m_isSelecting)
                return;
            if(_m_eventDialogueRefObj.flagType == EEventDialogueFlagType.END)
            {
                AudioMgr.instance.PlaySfx("sfx_click");
                UICoreMgr.instance.CloseTopNode();
                UICoreMgr.instance.AddNode(new UINodeMap(SCUIShowType.FULL));
            }
            else
            {
                if(_m_eventDialogueRefObj.nextList.Count > 1)//大于1说明下一个是选择项
                {
                    _m_isSelecting = true;
                    SCMsgCenter.SendMsg(SCMsgConst.EVENT_START_SELECT, _m_eventDialogueRefObj);
                }
                else
                {
                    _m_eventDialogueId = _m_eventDialogueRefObj.nextList[0];
                    _m_eventDialogueRefObj = SCRefDataMgr.instance.eventDialogueRefList.refDataList.Find(x => x.id == _m_eventDialogueId);
                    refreshShow();
                }
            }
        }
        private void onEventSelectConfirm(object[] _objs)
        {
            if (_objs == null || _objs.Length == 0)
                return;
            EventDialogueRefObj selectDialogueRefObj = _objs[0] as EventDialogueRefObj;
            if (selectDialogueRefObj == null || selectDialogueRefObj.nextList.Count == 0)
                return;
            _m_isSelecting = false;
            _m_eventDialogueId = selectDialogueRefObj.nextList[0];
            _m_eventDialogueRefObj = SCRefDataMgr.instance.eventDialogueRefList.refDataList.Find(x => x.id == _m_eventDialogueId);
            refreshShow();
            SCMsgCenter.SendMsg(SCMsgConst.EVENT_END_SELECT);
        }
    }
}
