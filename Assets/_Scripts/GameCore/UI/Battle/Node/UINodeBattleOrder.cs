using SCFrame;
using SCFrame.UI;
using UnityEngine;

namespace GameCore.UI
{
    public class UINodeBattleOrder : _ASCUINodeBase
    {
        public UINodeBattleOrder(SCUIShowType _showType) : base(_showType)
        {
        }

        public override bool needHideWhenEnterNewSameTypeNode => false;

        public override bool needShowWhenQuitNewSameTypeNode => false;

        public override bool canQuitByEsc => false;

        public override bool canQuitByMouseRight => false;

        public override bool ignoreOnUIList => false;

        public override SCUINodeFuncType nodeFuncType => SCUINodeFuncType.BATTLE;

        public override bool needMoveToBottomWhenHide => true;

        private GameObject _m_panelGO;
        private UIPanelBattleOrder _m_battleOrderPanel;
        private UIMonoBattleOrder _m_battleOrderMono;
        public override string GetNodeName()
        {
            return nameof(UINodeBattleOrder);
        }

        public override string GetResName()
        {
            return "panel_battle_order";
        }

        public override void OnEnterNode()
        {
            _m_panelGO = ResourcesHelper.LoadGameObject(GetResName(), GetRootTransform(), true);
            if (_m_panelGO == null)
            {
                Debug.LogError("未找到资源名为" + GetResName() + "的资源!!!");
                return;
            }
            _m_battleOrderMono = _m_panelGO.GetComponent<UIMonoBattleOrder>();
            if (_m_battleOrderMono == null)
            {
                Debug.LogError("资源名为" + GetResName() + "的资源上不存在对应的Mono!!!");
                return;
            }

            _m_battleOrderPanel = new UIPanelBattleOrder(_m_battleOrderMono, _m_showType);
            _m_battleOrderPanel.Initialize();
        }

        public override void OnHideNode()
        {
            _m_battleOrderPanel?.HidePanel();
        }

        public override void OnQuitNode()
        {
            _m_battleOrderPanel?.Discard();
        }

        public override void OnShowNode()
        {
            _m_battleOrderPanel?.ShowPanel();
        }
    }
}
