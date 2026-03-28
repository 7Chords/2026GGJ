using GameCore.RefData;
using SCFrame;
using SCFrame.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
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

        private string _m_currentDialogueFullContent;
        private bool _m_dialogueLineRevealComplete;
        public UIPanelEvent(UIMonoEvent _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_selectContainer = new UIPanelEventSelectContainer(mono.monoSelectContainer, SCUIShowType.INTERNAL);
        }

        public override void BeforeDiscard()
        {
            stopDialogueTypewriter();
            _m_selectContainer?.Discard();
        }

        public override void OnHidePanel()
        {
            SCMsgCenter.UnregisterMsg(SCMsgConst.EVENT_SELECT_CONFIRM, onEventSelectConfirm);
            SCMsgCenter.UnregisterMsg(SCMsgConst.EVENT_PART_EXCHANGE_COMPLETED, onEventPartExchangeCompleted);
            mono.imgClickArea.RemoveClickDown(onMouseClickDialogue);
            stopDialogueTypewriter();
            _m_selectContainer?.HidePanel();

        }

        public override void OnShowPanel()
        {
            SCMsgCenter.RegisterMsg(SCMsgConst.EVENT_SELECT_CONFIRM, onEventSelectConfirm);
            SCMsgCenter.RegisterMsg(SCMsgConst.EVENT_PART_EXCHANGE_COMPLETED, onEventPartExchangeCompleted);
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
            stopDialogueTypewriter();
            if (_m_eventDialogueRefObj.dialogueType == EEventDialogueType.STANDARD)
            {
                mono.txtName.text = _m_eventDialogueRefObj.name;
                _m_currentDialogueFullContent = _m_eventDialogueRefObj.content ?? string.Empty;
                if (mono.txtContent != null)
                    mono.txtContent.text = string.Empty;
                if (string.IsNullOrEmpty(_m_currentDialogueFullContent))
                {
                    _m_dialogueLineRevealComplete = true;
                }
                else
                {
                    _m_dialogueLineRevealComplete = false;
                    SCTaskHelper.instance.CreateCoroutine(this, dialogueTypewriterRoutine(), "EventDialogueTypewriter");
                }
            }
            else
            {
                _m_dialogueLineRevealComplete = true;
                _m_currentDialogueFullContent = string.Empty;
            }
        }

        private void stopDialogueTypewriter()
        {
            SCTaskHelper.instance?.KillAllCoroutines(this);
        }

        private IEnumerator dialogueTypewriterRoutine()
        {
            if (mono.txtContent == null)
            {
                _m_dialogueLineRevealComplete = true;
                yield break;
            }
            string full = _m_currentDialogueFullContent ?? string.Empty;
            var sb = new StringBuilder(full.Length);
            float interval = Mathf.Max(0.001f, mono.dialogueTypewriterInterval);
            foreach (char c in full)
            {
                sb.Append(c);
                mono.txtContent.text = sb.ToString();
                yield return new WaitForSeconds(interval);
            }
            _m_dialogueLineRevealComplete = true;
        }

        private void onMouseClickDialogue(PointerEventData _data, object[] _objs)
        {
            if (_m_eventDialogueRefObj == null)
                return;
            if (_m_isSelecting)
                return;
            if (_m_eventDialogueRefObj.dialogueType == EEventDialogueType.STANDARD && !_m_dialogueLineRevealComplete)
            {
                stopDialogueTypewriter();
                if (mono.txtContent != null)
                    mono.txtContent.text = _m_currentDialogueFullContent ?? string.Empty;
                _m_dialogueLineRevealComplete = true;
                return;
            }
            EventHandler.DealEvent(_m_eventDialogueRefObj.eventType);
            // TRAP_BATTLE runs its own TVSwitch into mask-combine + battle; do not run END->map or it cancels that flow.
            if (_m_eventDialogueRefObj.flagType == EEventDialogueFlagType.END
                && _m_eventDialogueRefObj.eventType != EEventType.TRAP_BATTLE)
            {
                AudioMgr.instance.PlaySfx("sfx_click");
                TVSwitchTransition.Run(() =>
                {
                    GameModel.instance.playerInfo.ApplyPendingMapMove();
                    UICoreMgr.instance.CloseTopNode();
                    UICoreMgr.instance.AddNode(new UINodeMap(SCUIShowType.FULL));
                });
            }
            else
            {
                if(_m_eventDialogueRefObj.nextList.Count > 1)
                {
                    _m_isSelecting = true;
                    SCMsgCenter.SendMsg(SCMsgConst.EVENT_START_SELECT, _m_eventDialogueRefObj);
                }
                else
                {
                    bool waitForPartExchange = _m_eventDialogueRefObj.eventType == EEventType.PART_2_PART
                        && GameModel.instance.playerInfo.bagPartInfoList != null
                        && GameModel.instance.playerInfo.bagPartInfoList.Count > 0;
                    if (!waitForPartExchange)
                        advanceToNextSingleDialogue();
                }
            }
        }

        private void advanceToNextSingleDialogue()
        {
            if (_m_eventDialogueRefObj == null || _m_eventDialogueRefObj.nextList == null || _m_eventDialogueRefObj.nextList.Count < 1)
                return;
            _m_eventDialogueId = _m_eventDialogueRefObj.nextList[0];
            _m_eventDialogueRefObj = SCRefDataMgr.instance.eventDialogueRefList.refDataList.Find(x => x.id == _m_eventDialogueId);
            refreshShow();
        }

        private void onEventPartExchangeCompleted(object[] _objs)
        {
            advanceToNextSingleDialogue();
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
