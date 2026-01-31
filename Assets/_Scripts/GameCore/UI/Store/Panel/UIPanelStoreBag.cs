using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SCFrame;
using UnityEngine.EventSystems;
using System;
using DG.Tweening;

namespace GameCore.UI
{
    public class UIPanelStoreBag : _ASCUIPanelBase<UIMonoStoreBag>
    {
        private UIPanelStoreBagContainer _m_itemContainer;
        private TweenContainer _m_tweenContainer;
        public UIPanelStoreBag(UIMonoStoreBag _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_itemContainer = new UIPanelStoreBagContainer(mono.monoContainer);
            _m_tweenContainer = new TweenContainer();
        }

        public override void BeforeDiscard()
        {
            _m_itemContainer?.Discard();
            _m_itemContainer = null;
            _m_tweenContainer?.KillAllDoTween();
            _m_tweenContainer = null;
        }

        public override void OnHidePanel()
        {
            mono.btnClose.RemoveClickDown(onBtnCloseClickDown);
            mono.btnClose.RemoveMouseEnter(onBtnCloseMouseEnter);
            mono.btnClose.RemoveMouseExit(onBtnCloseMouseExit);

            _m_itemContainer?.HidePanel();
        }

        public override void OnShowPanel()
        {
            mono.btnClose.AddMouseLeftClickDown(onBtnCloseClickDown);
            mono.btnClose.AddMouseEnter(onBtnCloseMouseEnter);
            mono.btnClose.AddMouseExit(onBtnCloseMouseExit);

            _m_itemContainer?.ShowPanel();
            _m_itemContainer?.SetListInfo(GameModel.instance.busyPartInfoList);
        }

        private void onBtnCloseClickDown(PointerEventData _arg, object[] _objs)
        {
            UICoreMgr.instance.CloseTopNode();
        }

        private void onBtnCloseMouseEnter(PointerEventData arg1, object[] arg2)
        {
            _m_tweenContainer.RegDoTween(mono.btnClose.transform.DOScale(mono.scaleMouseEnter,mono.scaleChgDuration));
        }

        private void onBtnCloseMouseExit(PointerEventData arg1, object[] arg2)
        {
            _m_tweenContainer.RegDoTween(mono.btnClose.transform.DOScale(Vector3.one, mono.scaleChgDuration));
        }
    }
}
