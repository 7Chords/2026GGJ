using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.UI
{
    public class UIPanelDeck : _ASCUIPanelBase<UIMonoDeck>
    {
        public UIPanelDeck(UIMonoDeck _mono, SCUIShowType _showType) : base(_mono, _showType)
        {
        }

        public override void AfterInitialize()
        {
            throw new System.NotImplementedException();
        }

        public override void BeforeDiscard()
        {
            throw new System.NotImplementedException();
        }

        public override void OnHidePanel()
        {
            throw new System.NotImplementedException();
        }

        public override void OnShowPanel()
        {
            throw new System.NotImplementedException();
        }
    }
}
