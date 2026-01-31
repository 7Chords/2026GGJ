using SCFrame;
using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.UI
{
    public class UINodeBattleWin : _ASCUINodeBase
    {
        public UINodeBattleWin(SCUIShowType _showType) : base(_showType)
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
        private UIPanelBattleWin _m_battleWinBtnPanel;
        private UIMonoBattleWin _m_battleWinBtnMono;
        public override string GetNodeName()
        {
            return nameof(UINodeBattleWin);
        }

        public override string GetResName()
        {
            return "panel_battle_win";
        }

        public override void OnEnterNode()
        {
            _m_panelGO = ResourcesHelper.LoadGameObject(GetResName(), GetRootTransform(), true);
            if (_m_panelGO == null)
            {
                Debug.LogError("未找到资源名为" + GetResName() + "的资源!!!");
                return;
            }
            _m_battleWinBtnMono = _m_panelGO.GetComponent<UIMonoBattleWin>();
            if (_m_battleWinBtnMono == null)
            {
                Debug.LogError("资源名为" + GetResName() + "的资源上不存在对应的Mono!!!");
                return;
            }

            _m_battleWinBtnPanel = new UIPanelBattleWin(_m_battleWinBtnMono, _m_showType);
            _m_battleWinBtnPanel.Initialize();
        }

        public override void OnHideNode()
        {
            _m_battleWinBtnPanel?.HidePanel();
        }

        public override void OnQuitNode()
        {
            _m_battleWinBtnPanel?.Discard();
        }

        public override void OnShowNode()
        {
            _m_battleWinBtnPanel?.ShowPanel();
        }
    }
}
