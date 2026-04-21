using GameCore.RefData;
using DG.Tweening;
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
        private TweenContainer _m_tweenContainer;

        public UIPanelEventSelectItem(UIMonoEventSelectItem _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_tweenContainer = new TweenContainer();
        }

        public override void BeforeDiscard()
        {
            _m_tweenContainer?.KillAllDoTween();
            _m_tweenContainer = null;
        }

        public override void OnHidePanel()
        {
            mono.btnSelect.RemoveClickDown(onBtnSelectClickDown);
            mono.btnSelect.RemoveMouseEnter(onBtnSelectMouseEnter);
            mono.btnSelect.RemoveMouseExit(onBtnSelectMouseExit);
            GetGameObject().transform.localScale = Vector3.one;
        }

        public override void OnShowPanel()
        {
            mono.btnSelect.AddMouseLeftClickDown(onBtnSelectClickDown);
            mono.btnSelect.AddMouseEnter(onBtnSelectMouseEnter);
            mono.btnSelect.AddMouseExit(onBtnSelectMouseExit);
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
            SCMsgCenter.SendMsg(SCMsgConst.EVENT_SELECT_CONFIRM, _m_dialogueRefObj);
        }

        private void onBtnSelectMouseEnter(PointerEventData _data, object[] _objs)
        {
            _m_tweenContainer?.RegDoTween(
                GetGameObject().transform.DOScale(mono.scaleMouseEnter, mono.scaleChgDuration));
        }

        private void onBtnSelectMouseExit(PointerEventData _data, object[] _objs)
        {
            _m_tweenContainer?.RegDoTween(
                GetGameObject().transform.DOScale(Vector3.one, mono.scaleChgDuration));
        }
    }
}
