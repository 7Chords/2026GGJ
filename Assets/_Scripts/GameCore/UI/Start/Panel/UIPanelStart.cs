using DG.Tweening;
using SCFrame;
using SCFrame.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameCore.UI
{
    public class UIPanelStart : _ASCUIPanelBase<UIMonoStart>
    {
        private TweenContainer _m_tweenContainer;
        public UIPanelStart(UIMonoStart _mono, SCUIShowType _showType) : base(_mono, _showType)
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
            mono.btnStart.RemoveClickDown(onBtnStartClickDown);
            mono.btnExit.RemoveClickDown(onBtnExitClickDown);
            mono.btnStart.RemoveMouseEnter(onBtnStartMouseEnter);
            mono.btnStart.RemoveMouseExit(onBtnStartMouseExit);
            mono.btnExit.RemoveMouseEnter(onBtnExitMouseEnter);
            mono.btnExit.RemoveMouseExit(onBtnExitMouseExit);
        }


        public override void OnShowPanel()
        {
            mono.btnStart.AddMouseLeftClickDown(onBtnStartClickDown);
            mono.btnExit.AddMouseLeftClickDown(onBtnExitClickDown);
            mono.btnStart.AddMouseEnter(onBtnStartMouseEnter);
            mono.btnStart.AddMouseExit(onBtnStartMouseExit);
            mono.btnExit.AddMouseEnter(onBtnExitMouseEnter);
            mono.btnExit.AddMouseExit(onBtnExitMouseExit);
        }

        private void onBtnExitClickDown(PointerEventData arg1, object[] arg2)
        {
            Application.Quit();
        }
        private void onBtnStartClickDown(PointerEventData arg1, object[] arg2)
        {
            AudioMgr.instance.PlaySfx("sfx_click");
            UICoreMgr.instance.CloseTopNode();
            UICoreMgr.instance.AddNode(new UINodeMap(SCUIShowType.FULL));
        }

        private void onBtnStartMouseEnter(PointerEventData arg1, object[] arg2)
        {
            _m_tweenContainer.RegDoTween(mono.btnStart.transform.DOScale(mono.scaleMouseEnter, mono.scaleChgDuration));
        }

        private void onBtnStartMouseExit(PointerEventData arg1, object[] arg2)
        {
            _m_tweenContainer.RegDoTween(mono.btnStart.transform.DOScale(Vector3.one, mono.scaleChgDuration));

        }

        private void onBtnExitMouseEnter(PointerEventData arg1, object[] arg2)
        {
            _m_tweenContainer.RegDoTween(mono.btnExit.transform.DOScale(mono.scaleMouseEnter, mono.scaleChgDuration));
        }

        private void onBtnExitMouseExit(PointerEventData arg1, object[] arg2)
        {
            _m_tweenContainer.RegDoTween(mono.btnExit.transform.DOScale(Vector3.one, mono.scaleChgDuration));

        }
    }


}