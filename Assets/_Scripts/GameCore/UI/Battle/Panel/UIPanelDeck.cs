using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            _m_deckContainer?.HidePanel();

        }

        public override void OnShowPanel()
        {
            _m_deckContainer?.ShowPanel();
            refreshShow();
        }

        private void refreshShow()
        {
            _m_deckContainer.SetListInfo(GameModel.instance.deckPartInfoList);
        }
    }
}
