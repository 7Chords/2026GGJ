using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SCFrame;
using UnityEngine.EventSystems;
using System;

namespace GameCore.UI
{
    public class UIPanelDeck : _ASCUIPanelBase<UIMonoDeck>
    {
        private UIPanelCommonPartContainer _m_deckContainer;
        public UIPanelDeck(UIMonoDeck _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            _m_deckContainer = new UIPanelCommonPartContainer(mono.monoContainer);
        }

        public override void BeforeDiscard()
        {
            _m_deckContainer?.Discard();
            _m_deckContainer = null;
        }

        public override void OnHidePanel()
        {
            mono.btnClose.RemoveClickDown(onBtnCloseClickDown);

            _m_deckContainer?.HidePanel();

        }

        public override void OnShowPanel()
        {
            mono.btnClose.AddMouseLeftClickDown(onBtnCloseClickDown);
            _m_deckContainer?.ShowPanel();
            refreshShow();
        }

        private void refreshShow()
        {
            _m_deckContainer.SetListInfo(GameModel.instance.deckPartInfoList);
        }


        private void onBtnCloseClickDown(PointerEventData _arg, object[] _objs)
        {
            UICoreMgr.instance.CloseTopNode();
        }
    }
}
