using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.UI
{
    public class UINodeDeck : _ASCUINodeBase
    {
        public UINodeDeck(SCUIShowType _showType) : base(_showType)
        {
        }

        public override bool needHideWhenEnterNewSameTypeNode => throw new System.NotImplementedException();

        public override bool needShowWhenQuitNewSameTypeNode => throw new System.NotImplementedException();

        public override bool canQuitByEsc => throw new System.NotImplementedException();

        public override bool canQuitByMouseRight => throw new System.NotImplementedException();

        public override bool ignoreOnUIList => throw new System.NotImplementedException();

        public override SCUINodeFuncType nodeFuncType => throw new System.NotImplementedException();

        public override bool needMoveToBottomWhenHide => throw new System.NotImplementedException();

        public override string GetNodeName()
        {
            throw new System.NotImplementedException();
        }

        public override string GetResName()
        {
            throw new System.NotImplementedException();
        }

        public override void OnEnterNode()
        {
            throw new System.NotImplementedException();
        }

        public override void OnHideNode()
        {
            throw new System.NotImplementedException();
        }

        public override void OnQuitNode()
        {
            throw new System.NotImplementedException();
        }

        public override void OnShowNode()
        {
            throw new System.NotImplementedException();
        }
    }
}
