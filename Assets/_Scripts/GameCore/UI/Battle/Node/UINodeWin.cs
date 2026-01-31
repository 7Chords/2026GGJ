using SCFrame;
using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.UI
{
    public class UINodeWin : _ASCUINodeBase
    {
        public UINodeWin(SCUIShowType _showType) : base(_showType)
        {
        }

        public override bool needHideWhenEnterNewSameTypeNode => false;

        public override bool needShowWhenQuitNewSameTypeNode => false;

        public override bool canQuitByEsc => false;

        public override bool canQuitByMouseRight => false;

        public override bool ignoreOnUIList => false;

        public override SCUINodeFuncType nodeFuncType => SCUINodeFuncType.BATTLE;

        public override bool needMoveToBottomWhenHide => false;


        private GameObject _m_panelGO;
        private UIPanelWin _m_deckPanel;
        private UIMonoWin _m_deckMono;
        public override string GetNodeName()
        {
            return nameof(UINodeWin);
        }

        public override string GetResName()
        {
            return "panel_win";
        }

        public override void OnEnterNode()
        {
            _m_panelGO = ResourcesHelper.LoadGameObject(GetResName(), GetRootTransform(), true);
            if (_m_panelGO == null)
            {
                Debug.LogError("未找到资源名为" + GetResName() + "的资源!!!");
                return;
            }
            _m_deckMono = _m_panelGO.GetComponent<UIMonoWin>();
            if (_m_deckMono == null)
            {
                Debug.LogError("资源名为" + GetResName() + "的资源上不存在对应的Mono!!!");
                return;
            }

            _m_deckPanel = new UIPanelWin(_m_deckMono, _m_showType);
            _m_deckPanel.Initialize();
        }

        public override void OnHideNode()
        {
            _m_deckPanel?.HidePanel();
        }

        public override void OnQuitNode()
        {
            _m_deckPanel?.Discard();
        }

        public override void OnShowNode()
        {
            _m_deckPanel?.ShowPanel();
        }
    }
}
