using SCFrame.UI;
using System.Collections;
using System.Collections.Generic;
using SCFrame;
using UnityEngine;

namespace GameCore.UI
{
    public class UINodeBattle : _ASCUINodeBase
    {
        public UINodeBattle(SCUIShowType _showType) : base(_showType)
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
        private UIPanelBattle _m_battlePanel;
        private UIMonoBattle _m_battleMono;
        public override string GetNodeName()
        {
            return nameof(UINodeBattle);
        }

        public override string GetResName()
        {
            return "panel_battle";
        }

        public override void OnHideNode()
        {
            _m_battlePanel?.HidePanel();
        }

        public override void OnQuitNode()
        {
            _m_battlePanel?.Discard();
        }

        public override void OnShowNode()
        {
            _m_battlePanel?.ShowPanel();
        }
        
        public override void OnEnterNode()
        {
            _m_panelGO = ResourcesHelper.LoadGameObject(GetResName(), GetRootTransform(), true);
            if (_m_panelGO == null)
            {
                return;
            }
            _m_battleMono = _m_panelGO.GetComponent<UIMonoBattle>();
            if (_m_battleMono == null)
            {
                Debug.LogError("��Դ��Ϊ" + GetResName() + "����Դ�ϲ����ڶ�Ӧ��Mono!!!");
                return;
            }

            _m_battlePanel = new UIPanelBattle(_m_battleMono, _m_showType);
            _m_battlePanel.Initialize();
        }

    }
}
